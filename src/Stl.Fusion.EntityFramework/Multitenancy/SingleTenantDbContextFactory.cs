using Microsoft.EntityFrameworkCore;
using Stl.Fusion.EntityFramework.Internal;
using Stl.Multitenancy;

namespace Stl.Fusion.EntityFramework;

public sealed class SingleTenantDbContextFactory<TDbContext> : IMultitenantDbContextFactory<TDbContext>
    where TDbContext : DbContext
{
    private IDbContextFactory<TDbContext> DbContextFactory { get; }

    public SingleTenantDbContextFactory(IDbContextFactory<TDbContext> dbContextFactory)
        => DbContextFactory = dbContextFactory;

    public TDbContext CreateDbContext(Tenant tenant)
        => tenant == Tenant.Default || tenant == Tenant.Any
            ? DbContextFactory.CreateDbContext()
            : throw Errors.DefaultDbContextFactoryDoesNotSupportMultitenancy();
}