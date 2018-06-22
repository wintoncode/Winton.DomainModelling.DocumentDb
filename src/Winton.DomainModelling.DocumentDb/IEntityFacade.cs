using System;
using System.Linq;
using System.Threading.Tasks;

namespace Winton.DomainModelling.DocumentDb
{
    public interface IEntityFacade
    {
        Task<TEntity> Create<TEntity, TEntityId>(TEntity entity)
            where TEntity : Entity<TEntityId>
            where TEntityId : IEquatable<TEntityId>;

        Task Delete<TEntity, TEntityId>(TEntityId id)
            where TEntity : Entity<TEntityId>
            where TEntityId : IEquatable<TEntityId>;

        IQueryable<TEntity> Query<TEntity, TEntityId>()
            where TEntity : Entity<TEntityId>
            where TEntityId : IEquatable<TEntityId>;

        Task<TEntity> Read<TEntity, TEntityId>(TEntityId id)
            where TEntity : Entity<TEntityId>
            where TEntityId : IEquatable<TEntityId>;

        Task<TEntity> Upsert<TEntity, TEntityId>(TEntity entity)
            where TEntity : Entity<TEntityId>
            where TEntityId : IEquatable<TEntityId>;
    }
}