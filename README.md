# Winton.DomainModelling.DocumentDb

[![Build status](https://ci.appveyor.com/api/projects/status/9472sx6gw9y7lq0q?svg=true)](https://ci.appveyor.com/project/wintoncode/winton-domainmodelling-documentdb/branch/master)
[![Travis Build Status](https://travis-ci.org/wintoncode/Winton.DomainModelling.DocumentDb.svg?branch=master)](https://travis-ci.org/wintoncode/Winton.DomainModelling.DocumentDb)
[![NuGet version](https://img.shields.io/nuget/v/Winton.DomainModelling.DocumentDb.svg)](https://www.nuget.org/packages/Winton.DomainModelling.DocumentDb)
[![NuGet version](https://img.shields.io/nuget/vpre/Winton.DomainModelling.DocumentDb.svg)](https://www.nuget.org/packages/Winton.DomainModelling.DocumentDb)

A facade library useful for [Entity](https://github.com/wintoncode/Winton.DomainModelling.Abstractions#entity) and Value Object operations in DocumentDb (SQL API).

This implementations allow multiple types to be transparently stored in one collection using 'wrapper' documents with type discriminators and namespaced IDs (for entities). It can be tempting for those from a traditional SQL background to provision a separate collection per type. However, this is often unnecessarily expensive, especially if much of the reserved throughput for a given collection is unused. Taking advantage of the "schemaless" nature of a document store, such as DocumentDb, can both reduce cost and simplify infrastructural complexity. This implementation provides an easy way to work with a single collection within a bounded context (within which persisted type names are unique) while outwardly still achieving the desired level of strong typing. There really is a schema, but the database doesn't need to know about it.

## Types

### IEntityFacade

An abstraction layer over [Entity](https://github.com/wintoncode/Winton.DomainModelling.Abstractions#entity) CRUD operations in DocumentDb. Provides strong typed Create, Read, **Upsert**, Delete, and Query methods.

### EntityFacade

The default implementation of `IEntityFacade`. The Create method supports automatic ID generation for string-serializable ID types, otherwise IDs must be set before creating.

Note that this implementation is currently **incompatible with partitioned collections**. This restriction could potentially be lifted in a future version, at the expense of implementation complexity (and probably a leakier abstraction). However, for applications requiring large collections, where partitioning is actually needed, the conveniences provided by this facade are unlikely to be suitable anyway.

### IValueObjectFacade

An abstraction layer over Value Object operations in DocumentDb. Provides strong typed Create, Delete, and Query methods.

### ValueObjectFacade

The default implementation of `IValueObjectFacade`.

Note that this implementation is currently **incompatible with partitioned collections**. This restriction could potentially be lifted in a future version, at the expense of implementation complexity (and probably a leakier abstraction). However, for applications requiring large collections, where partitioning is actually needed, the conveniences provided by this facade are unlikely to be suitable anyway.

## Usage

The constructors for both `EntityFacade` and `ValueObjectFacade` take a [Database](https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.documents.database), a [DocumentCollection](https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.documents.documentcollection), and an [IDocumentClient](https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.documents.idocumentclient). Of course, resolving all dependencies from a DI container is preferred, but for clarity they can be manually constructed as

```csharp
Database database = new Database { Id = "AllTheData" };
DocumentCollection documentCollection = new DocumentCollection { Id = "AccountingData" };
IDocumentClient documentClient = new DocumentClient(...);

IEntityFacade entityFacade = new EntityFacade(database, documentCollection, documentClient);
IValueObjectFacade valueObjectFacade = new ValueObjectFacade(database, documentCollection, documentClient);
```

Consider some application with an "Accounting" domain containing 2 entity types, `Account` and `Transaction`, each with a [strong typed](https://tech.winton.com/2017/06/strong-typing-a-pattern-for-more-robust-code/) ID. Note that both ID types use [SingleValueConverter](https://github.com/wintoncode/Winton.Extensions.Serialization.Json#singlevalueconverter), so they are string-serializable. The "Accounting" domain also contains a value object type, `AccountType`, used as reference data.

```csharp
[JsonConverter(typeof(SingleValueConverter))]
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

[JsonConverter(typeof(SingleValueConverter))]
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
    [JsonConstructor]
    public AccountType(
        string name,
        decimal rate,
        ...)
        : base(id)
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
    Task<AccountId> Create(Account account);

    Task<Account> Get(AccountId id);

    ...
}

public interface ITransactionRepository
{
    Task<TransactionId> Create(Transaction transaction);

    IEnumerable<Transaction> GetAllSentBy(AccountId accountId);

    ...
}

public interface IAccountTypeRepository
{
    Task Create(AccountType accountType);

    IEnumerable<AccountType> GetAll();

    ...
}
```

The respective implementations of these repositories, potentially defined in a separate "Persistence" layer, would simply be thin wrappers around the `IEntityFacade` or `IValueObjectFacade`.

```csharp
internal sealed class AccountRepository : IAccountRepository
{
    private readonly IEntityFacade _entityFacade;

    public AccountRepository(IEntityFacade entityFacade)
    {
        _entityFacade = entityFacade;
    }

    public async Task<AccountId> Create(Account account)
    {
        return (await _entityFacade.Create<Account, AccountId>(account)).Id;
    }

    public async Task<Account> Get(AccountId id)
    {
        return await _entityFacade.Read<Account, AccountId>(id);
    }

    ...
}

internal sealed class TransactionRepository : ITransactionRepository
{
    private readonly IEntityFacade _entityFacade;

    public TransactionRepository(IEntityFacade entityFacade)
    {
        _entityFacade = entityFacade;
    }

    public async Task<TransactionId> Create(Transaction transaction)
    {
        return (await _entityFacade.Create<Transaction, TransactionId>(transaction)).Id;
    }

    public IEnumerable<Transaction> GetAllSentBy(AccountId accountId)
    {
        return _entityFacade.Query<Transaction, TransactionId>()
                            .Where(t => t.Sender == accountId)
                            .AsEnumerable();
    }

    ...
}

internal sealed class AccountTypeRepository : IAccountTypeRepository
{
    private readonly IValueObjectFacade _valueObjectFacade;

    public AccountTypeRepository(IValueObjectFacade valueObjectFacade)
    {
        _valueObjectFacade = valueObjectFacade;
    }

    public async Task Create(AccountType accountType)
    {
        await _valueObjectFacade.Create<AccountType>(accountType);
    }

    public IEnumerable<AccountType> GetAll()
    {
        return _valueObjectFacade.Query<AccountType>()
                                 .AsEnumerable();
    }

    ...
}
```

These repositories will store their respective types in a single shared collection, using the type names to discriminate between each type and namespace the IDs (for entities). Therefore, these names should be considered part of the document schema, and would require a data migration if they were changed as part of a domain refactoring.