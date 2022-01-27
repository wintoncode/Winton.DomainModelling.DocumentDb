# [DEPRECATED] Winton.DomainModelling.DocumentDb

> :warning: **This repo is no longer maintained**: We recommend using a library that supports the [Microsoft.Azure.Cosmos SDK](https://www.nuget.org/packages/Microsoft.Azure.Cosmos/)

[![Appveyor](https://ci.appveyor.com/api/projects/status/9472sx6gw9y7lq0q?svg=true)](https://ci.appveyor.com/project/wintoncode/winton-domainmodelling-documentdb/branch/master)
[![Travis CI](https://travis-ci.com/wintoncode/Winton.DomainModelling.DocumentDb.svg?branch=master)](https://travis-ci.com/wintoncode/Winton.DomainModelling.DocumentDb)
[![NuGet version](https://img.shields.io/nuget/v/Winton.DomainModelling.DocumentDb.svg)](https://www.nuget.org/packages/Winton.DomainModelling.DocumentDb)
[![NuGet version](https://img.shields.io/nuget/vpre/Winton.DomainModelling.DocumentDb.svg)](https://www.nuget.org/packages/Winton.DomainModelling.DocumentDb)

Provides a repository interface and implementation for entities and value objects on top of DocumentDb (SQL API).

This implementation allows multiple types to be transparently stored in one collection using 'wrapper' documents with type discriminators and namespaced IDs (for entities).
It can be tempting for those from a traditional SQL background to provision a separate collection per type.
However, this is often unnecessarily expensive, especially if much of the reserved throughput for a given collection is unused.
Taking advantage of the "schemaless" nature of a document store, such as DocumentDb, can both reduce cost and simplify infrastructural complexity.
This implementation provides an easy way to work with a single collection within a bounded context (within which persisted type names are unique) while outwardly still achieving the desired level of strong typing.
There really is a schema, but the database doesn't need to know about it.

## Repositories

Note that the default implementations are currently **incompatible with partitioned collections**.
This restriction could potentially be lifted in a future version, at the expense of implementation complexity (and probably a leakier abstraction).
However, for applications requiring large collections, where partitioning is actually needed, the conveniences provided by these repositories are unlikely to be suitable anyway.

### IEntityRepository

An abstraction layer over entity CRUD operations in DocumentDb.
Provides strong typed Put, Read, Delete, and Query methods.
The object being passed to the repository must be serialisable as JSON using Newtonsoft, as per the requirements of DocumentDB.
The object must also be fully formed and its ID must be set.
Really on the persistence layer to set an entity's ID is a leaky abstraction.
It is much better for the domain model to take responsibility for fully creating entities and it is trivial to create a GUID to use as an ID, which is what DocumentDB would do anyway.

### IValueRepository

An abstraction layer over value object operations in DocumentDb.
Provides strong typed Put, Delete, and Query methods.

## Setup

The default implementations of both `IEntityRepository` and `IValueRepository` should be created from their provided factories.
This library exposes an `IServiceCollection` extension method called `AddDomainModellingDocumentDb` which should be called so that the `IEntityRepositoryFactory` and `IValueRepositoryFactory` are available via dependency injection.
For example, in the simplest form it can be called as so:

```csharp
IDocumentClient documentClient = new DocumentClient(...);
serviceCollection.AddDomainModellingDocumentDb(_ => Task.FromResult(documentClient));
```

The provided callback is used by the library to get an [`IDocumentClient`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.documents.idocumentclient).
As per Microsoft's recommendations for using `IDocumentClient`, this should be a singleton.
It is the responsibility of the consuming application to ensure that this function returns the same instance each time it is invoked.
This library makes no attempt to ensure the `IDocumentClient` is a singleton.

The `documentClientFactory` function is async and also has access to the `IServiceProvider`.
This allows more complex creation scenarios.
For example, consider a situation in which the `AzureServiceTokenProvider` is used to obtain keys from Key Vault for DocumentDB:

```csharp
internal class DocumentClientFactory
{
    private readonly Lazy<Task<IDocumentClient>> _documentClient;

    public DocumentClientFactory(IOptions<DocumentDbOptions> options)
    {
        _documentClient = new Lazy<Task<IDocumentClient>>(
            async () =>
            {
                var keyBundle = await new KeyVaultClient(
                    new KeyVaultClient.AuthenticationCallback(
                        new AzureServiceTokenProvider().KeyVaultTokenCallback))
                    .GetSecretAsync(new SecretIdentifier(options.Value.SecretId).Identifier);

                return new DocumentClient(options.Value.Uri, keyBundle.Value);
            });
    }

    public Task<IDocumentClient> Create() => _documentClient.Value;
}
```

We can then register this factory with the `IServiceCollection` and use it as the `documentClientFactory` function.

```csharp
serviceCollection
    .AddSingleton<DocumentClientFactory>()
    .AddDomainModellingDocumentDb(
        provider => provider
            .GetRequiredService<DocumentClientFactory>()
            .Create());
```

## Usage

Consider some application with an "Accounting" domain containing two entity types, `Account` and `Transaction`. The "Accounting" domain also contains a value object type, `AccountType`, used as reference data.

```csharp
public struct AccountId : IEquatable<AccountId>
{
    ...
}

public sealed class Account : Entity<AccountId>
{
    public Account(
        AccountId id,
        AccountType type,
        ...)
        : base(id)
    {
        Type = type;
        ...
    }

    public AccountType Type { get; }

    ...
}

public struct TransactionId : IEquatable<TransactionId>
{
    ...
}

public sealed class Transaction : Entity<TransactionId>
{
    public Transaction(
        TransactionId id,
        AccountId sender,
        AccountId recipient,
        ...)
        : base(id)
    {
        Sender = sender;
        Recipient = recipient;
        ...
    }

    public AccountId Sender { get; }

    public AccountId Recipient { get; }

    ...
}

public struct AccountType : IEquatable<AccountType>
{
    public AccountType(
        string name,
        decimal rate,
        ...)
    {
        Name = name;
        Rate = rate;
        ...
    }

    public string Name { get; }

    public decimal Rate { get; }

    ...
}
```

These types could each have their own repository interfaces, defined within the "Accounting" domain.

```csharp
public interface IAccountRepository
{
    Task Put(Account account);

    Task<Account> Get(AccountId id);

    ...
}

public interface ITransactionRepository
{
    Task Put(Transaction transaction);

    IEnumerable<Transaction> GetAllSentBy(AccountId accountId);

    ...
}

public interface IAccountTypeRepository
{
    Task Put(AccountType accountType);

    IEnumerable<AccountType> GetAll();

    ...
}
```

The respective implementations of these repositories, potentially defined in a separate persistence layer, would simply be thin wrappers around the `IEntityRepository` or `IValueRepository`.

```csharp
internal sealed class AccountRepository : IAccountRepository
{
    private readonly IEntityRepositoryFactory _entityRepositoryFactory;

    public AccountRepository(IEntityRepositoryFactory entityRepositoryFactory)
    {
        _entityRepositoryFactory = entityRepositoryFactory;
    }

    public async Task Put(Account account)
    {
        var repository = await CreateRepository();
        return await _entityRepository.Put(account);
    }

    public async Task<Account> Get(AccountId id)
    {
        var repository = await CreateRepository();
        return await repository.Read(id);
    }

    private Task<IEntityRepository<AccountDto>> CreateRepository() => _entityRepositoryFactory.Create<AccountDto>(
            new Database { Id = "ExampleApp" },
            new DocumentCollection { Id = "ExampleApp" },
            "Account",
            dto => dto.Id);
}

internal sealed class TransactionRepository : ITransactionRepository
{
    private readonly IEntityRepositoryFactory _entityRepositoryFactory;

    public TransactionRepository(IEntityRepositoryFactory entityRepositoryFactory)
    {
        _entityRepositoryFactory = entityRepositoryFactory;
    }

    public async Task Put(Transaction transaction)
    {
        var repository = await CreateRepository();
        return await _entityRepository.Put(account);
    }

    public IEnumerable<Transaction> GetAllSentBy(AccountId accountId)
    {
        var repository = await CreateRepository();
        return repository.Query(t => t.Sender == (string)accountId);
    }

    private Task<IEntityRepository<TransactionDto>> CreateRepository() => _entityRepositoryFactory.Create<TransactionDto>(
            new Database { Id = "ExampleApp" },
            new DocumentCollection { Id = "ExampleApp" },
            "Transaction",
            dto => dto.Id);
}

internal sealed class AccountTypeRepository : IAccountTypeRepository
{
    private readonly IValueRepositoryFactory _valueRepositoryFactory;

    public AccountTypRepository(IValueRepositoryFactory valueRepositoryFactory)
    {
        _valueRepositoryFactory = valueRepositoryFactory;
    }

    public async Task Put(AccountType accountType)
    {
        var repository = await CreateRepository();
        await repository.Put(accountType);
    }

    public IEnumerable<AccountType> GetAll()
    {
        var repository = await CreateRepository();
        return repository.Query();
    }

    private Task<IValueRepository<AccountTypeDto>> CreateRepository() => _valueRepositoryFactory.Create<AccountTypeDto>(
            new Database { Id = "ExampleApp" },
            new DocumentCollection { Id = "ExampleApp" },
            "AccountType",
            dto => dto.Id);
}
```

These repositories will store their respective types in a single shared collection, using the type names to discriminate between each type and namespace the IDs (for entities).
Therefore, these names should be considered part of the document schema, and would require a data migration if they were ever changed.
For this reason we recommended specifying them as string literals rather than doing something like `typeof(TEntity).Name` as this would change if the domain model was refactored and therefore couples the domain and persistence layers in a undesirable way.

Also notice that, in the example above, the types being persisted a DTOs, for example `AccountDto`.
You are free to persist whatever data you want using these repositories providing they are serialisable, but given that serialisation is not a concern of the domain model, it is usually preferable to define a DTO that is a serialisable version of the domain model objects.
This again allows for greater freedom when refactoring the domain because the domain does not have worry about what affect a refactoring might have on the shape of the stored data.
The compiler will instead inform the developer that there is an inconsistency between the domain model and DTO representation.
