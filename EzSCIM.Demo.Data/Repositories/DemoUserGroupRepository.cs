using EzSCIM.Demo.Data.Entities;
using EzSCIM.EfCore;
using Microsoft.EntityFrameworkCore;

namespace EzSCIM.Demo.Data.Repositories;

/// <summary>
/// EF Core data repository for SCIM users and groups.
/// All CRUD logic is provided by <see cref="EfScimRepositoryBase{TUser,TGroup,TContext}"/>.
/// Uses <see cref="ScimDbContextBase"/> so it works with any provider-specific subclass.
/// </summary>
public class DemoUserGroupRepository
    : EfScimRepositoryBase<DemoUserEntity, DemoGroupEntity, ScimDbContextBase>
{
    public DemoUserGroupRepository(ScimDbContextBase context) : base(context) { }

    protected override DbSet<DemoUserEntity> Users => Context.Users;
    protected override DbSet<DemoGroupEntity> Groups => Context.Groups;
}

