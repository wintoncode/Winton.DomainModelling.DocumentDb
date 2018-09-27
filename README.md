# Winton.DomainModelling.DocumentDb

[![Build status](https://ci.appveyor.com/api/projects/status/9472sx6gw9y7lq0q?svg=true)](https://ci.appveyor.com/project/wintoncode/winton-domainmodelling-documentdb/branch/master)
[![Travis Build Status](https://travis-ci.org/wintoncode/Winton.DomainModelling.DocumentDb.svg?branch=master)](https://travis-ci.org/wintoncode/Winton.DomainModelling.DocumentDb)
[![NuGet version](https://img.shields.io/nuget/v/Winton.DomainModelling.DocumentDb.svg)](https://www.nuget.org/packages/Winton.DomainModelling.DocumentDb)
[![NuGet version](https://img.shields.io/nuget/vpre/Winton.DomainModelling.DocumentDb.svg)](https://www.nuget.org/packages/Winton.DomainModelling.DocumentDb)

A facade library useful for [Entity](https://github.com/wintoncode/Winton.DomainModelling.Abstractions#entity) and Value Object operations in DocumentDb (SQL API).

This implementations allow multiple types to be transparently stored in one collection using 'wrapper' documents with type discriminators and namespaced IDs (for entities). It can be tempting for those from a traditional SQL background to provision a separate collection per type. However, this is often unnecessarily expensive, especially if much of the reserved throughput for a given collection is unused. Taking advantage of the "schemaless" nature of a document store, such as DocumentDb, can both reduce cost and simplify infrastructural complexity. This implementation provides an easy way to work with a single collection within a bounded context (within which persisted type names are unique) while outwardly still achieving the desired level of strong typing. There really is a schema, but the database doesn't need to know about it.

## Facade Interfaces

Note that the default implementations are currently **incompatible with partitioned collections**. This restriction could potentially be lifted in a future version, at the expense of implementation complexity (and probably a leakier abstraction). However, for applications requiring large collections, where partitioning is actually needed, the conveniences provided by this facade are unlikely to be suitable anyway.

### IEntityFacade

An abstraction layer over [Entity](https://github.com/wintoncode/Winton.DomainModelling.Abstractions#entity) CRUD operations in DocumentDb. Provides strong typed Create, Read, **Upsert**, Delete, and Query methods. The Create method supports automatic ID generation for string-serializable ID types, otherwise IDs must be set before creating.

### IValueObjectFacade

An abstraction layer over Value Object operations in DocumentDb. Provides strong typed Create, Delete, and Query methods.

## Domain Object Persistence

The default implementations of both `IEntityFacade` and `IValueObjectFacade` should be created from their provided factories. These can each be constructed from an [IDocumentClient](https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.documents.idocumentclient). Their Create methods both take a [Database](https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.documents.database) and a [DocumentCollection](https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.documents.documentcollection). Of course, resolving all dependencies from a DI container is preferred, but for clarity they can be manually constructed as

```csharp
IDocumentClient documentClient = new DocumentClient(...);

Database database = new Database { Id = "AllTheData" };
DocumentCollection documentCollection = new DocumentCollection { Id = "AccountingData" };

IEntityFacadeFactory entityFacadeFactory = new EntityFacadeFactory(documentClient);
IValueObjectFacadeFactory valueObjectFacadeFactory = new ValueObjectFacadeFactory(documentClient);

IEntityFacade<Account, AccountId> accountFacade = entityFacadeFactory.Create<Account, AccountId>(database, documentCollection);
IEntityFacade<Transaction, TransactionId> transactionFacade = entityFacadeFactory.Create<Transaction, TransactionId>(database, documentCollection);
IValueObjectFacade<AccountType> accountTypeFacade = valueObjectFacadeFactory.Create<AccountType>(database, documentCollection);
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

The respective implementations of these repositories, potentially defined in a separate persistence layer, would simply be thin wrappers around the `IEntityFacade` or `IValueObjectFacade`.

```csharp
internal sealed class AccountRepository : IAccountRepository
{
    private readonly IEntityFacade<Account, AccountId> _entityFacade;

    public AccountRepository(IEntityFacade<Account, AccountId> entityFacade)
    {
        _entityFacade = entityFacade;
    }

    public async Task<AccountId> Create(Account account)
    {
        return (await _entityFacade.Create(account)).Id;
    }

    public async Task<Account> Get(AccountId id)
    {
        return await _entityFacade.Read(id);
    }

    ...
}

internal sealed class TransactionRepository : ITransactionRepository
{
    private readonly IEntityFacade<Transaction, TransactionId> _entityFacade;

    public TransactionRepository(IEntityFacade<Transaction, TransactionId> entityFacade)
    {
        _entityFacade = entityFacade;
    }

    public async Task<TransactionId> Create(Transaction transaction)
    {
        return (await _entityFacade.Create(transaction)).Id;
    }

    public IEnumerable<Transaction> GetAllSentBy(AccountId accountId)
    {
        return _entityFacade.Query(t => t.Sender == accountId);
    }

    ...
}

internal sealed class AccountTypeRepository : IAccountTypeRepository
{
    private readonly IValueObjectFacade<AccountType> _valueObjectFacade;

    public AccountTypeRepository(IValueObjectFacade<AccountType> valueObjectFacade)
    {
        _valueObjectFacade = valueObjectFacade;
    }

    public async Task Create(AccountType accountType)
    {
        await _valueObjectFacade.Create(accountType);
    }

    public IEnumerable<AccountType> GetAll()
    {
        return _valueObjectFacade.Query();
    }

    ...
}
```

These repositories will store their respective types in a single shared collection, using the type names to discriminate between each type and namespace the IDs (for entities). Therefore, these names should be considered part of the document schema, and would require a data migration if they were changed as part of a domain refactoring.

## Data Transfer Object Persistence

In most applications, it is desirable to completely separate persistence concerns from domain logic. The above examples slightly violate this principle by leaking some Json serialization details into the domain objects. However, this can be easily avoided, at the expense of having to define some Data Transfer Objects (DTOs), and mapping functions. Consider essentially the same domain objects, but with all serialization logic removed.

```csharp
public struct AccountId : IEquatable<AccountId>
{
    public static explicit operator string(AccountId id) { ... }

    public static explicit operator AccountId(string value) { ... }

    ...
}

...

public struct TransactionId : IEquatable<TransactionId>
{
    public static explicit operator string(TransactionId id) { ... }

    public static explicit operator TransactionId(string value) { ... }

    ...
}

...

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

The persistence layer DTOs, including serialization logic, for these simple types would then look very similar to the original domain objects.

```csharp
internal struct AccountDto
{
    [JsonConstructor]
    public AccountDto(
        string id,
        AccountTypeDto type,
        ...)
    {
        Type = type;
        ...
    }

    public string Id { get; }

    public AccountTypeDto Type { get; }

    ...
}

internal struct TransactionDto
{
    [JsonConstructor]
    public TransactionDto(
        string id,
        string sender,
        string recipient,
        ...)
    {
        Sender = sender;
        Recipient = recipient;
        ...
    }

    public string Id { get; }

    public string Sender { get; }

    public string Recipient { get; }

    ...
}

internal struct AccountTypeDto
{
    [JsonConstructor]
    public AccountTypeDto(
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

The overloaded implementations of both `IEntityFacade` and `IValueObjectFacade` can be created from the same factories, but by specifying the appropriate DTO and mapping function. They can be manually constructed as

```csharp
IEntityFacade<Account, AccountId, AccountDto> accountFacade = entityFacadeFactory.Create<Account, AccountId, AccountDto>(
    database,
    documentCollection,
    a => new AccountDto((string)a.Id, new AccountTypeDto(a.Type.Name, a.Type.Rate)),
    d => new Account((AccountId)d.Id, new AccountType(d.Type.Name, d.Type.Rate)));
IEntityFacade<Transaction, TransactionId, TransactionDto> transactionFacade = entityFacadeFactory.Create<Transaction, TransactionId, TransactionDto>(
    database,
    documentCollection,
    t => new TransactionDto((string)t.Id, (string)t.Sender, (string)t.Recipient),
    d => new Transaction((TransactionId)d.Id, (AccountId)d.Sender, (AccountId)d.Recipient));
IValueObjectFacade<AccountType, AccountTypeDto> accountTypeFacade = valueObjectFacadeFactory.Create<AccountType, AccountTypeDto>(
    database,
    documentCollection,
    at => new AccountTypeDto(at.Name, at.Rate),
    d => new AccountType(d.Name, d.Rate));
```

The new repository implementations would then use the overloaded facade interfaces.

```csharp
internal sealed class AccountRepository : IAccountRepository
{
    private readonly IEntityFacade<Account, AccountId, AccountDto> _entityFacade;

    public AccountRepository(IEntityFacade<Account, AccountId, AccountDto> entityFacade)
    {
        _entityFacade = entityFacade;
    }

    public async Task<AccountId> Create(Account account)
    {
        return (await _entityFacade.Create(account)).Id;
    }

    public async Task<Account> Get(AccountId id)
    {
        return await _entityFacade.Read(id);
    }

    ...
}

internal sealed class TransactionRepository : ITransactionRepository
{
    private readonly IEntityFacade<Transaction, TransactionId, TransactionDto> _entityFacade;

    public TransactionRepository(IEntityFacade<Transaction, TransactionId, TransactionDto> entityFacade)
    {
        _entityFacade = entityFacade;
    }

    public async Task<TransactionId> Create(Transaction transaction)
    {
        return (await _entityFacade.Create(transaction)).Id;
    }

    public IEnumerable<Transaction> GetAllSentBy(AccountId accountId)
    {
        var sender = (string)accountId;
        return _entityFacade.Query(t => t.Sender == sender);
    }

    ...
}

internal sealed class AccountTypeRepository : IAccountTypeRepository
{
    private readonly IValueObjectFacade<AccountType, AccountTypeDto> _valueObjectFacade;

    public AccountTypeRepository(IValueObjectFacade<AccountType, AccountTypeDto> valueObjectFacade)
    {
        _valueObjectFacade = valueObjectFacade;
    }

    public async Task Create(AccountType accountType)
    {
        await _valueObjectFacade.Create(accountType);
    }

    public IEnumerable<AccountType> GetAll()
    {
        return _valueObjectFacade.Query();
    }

    ...
}
```

Of course, this method results in more code overall, due to the need to define the DTOs, and lambdas to perform the mappings. However, these types and conversions are very simple, and full separation of domain and persistence concerns has been achieved. As before, these repositories will store their respective types in a single shared collection, using the **domain object** type names to discriminate between each type.