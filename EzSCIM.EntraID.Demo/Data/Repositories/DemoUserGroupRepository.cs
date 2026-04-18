using EzSCIM.EfCore;
using EzSCIM.EntraID.Demo.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace EzSCIM.EntraID.Demo.Data.Repositories;

/// <summary>
/// EF Core data repository for the demo SCIM API.
/// All CRUD logic is provided by <see cref="EfScimRepositoryBase{TUser,TGroup,TContext}"/>.
/// </summary>
public class DemoUserGroupRepository
    : EfScimRepositoryBase<DemoUserEntity, DemoGroupEntity, DemoScimDbContext>
{
    public DemoUserGroupRepository(DemoScimDbContext context) : base(context) { }

    protected override DbSet<DemoUserEntity> Users => Context.Users;
    protected override DbSet<DemoGroupEntity> Groups => Context.Groups;
}

