using EzSCIM.EfCore;
using EzSCIM.IntegrationTests.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace EzSCIM.IntegrationTests.Data.Repositories;

/// <summary>
/// Entity Framework implementation of IUserGroupDataRepository for integration tests.
/// All CRUD logic is provided by <see cref="EfScimRepositoryBase{TUser,TGroup,TContext}"/>.
/// This class only maps the DbSets from <see cref="ScimDbContext"/>.
/// </summary>
public class EfUserGroupDataRepository
    : EfScimRepositoryBase<UserEntity, GroupEntity, ScimDbContext>
{
    public EfUserGroupDataRepository(ScimDbContext context) : base(context) { }

    protected override DbSet<UserEntity>  Users  => Context.Users;
    protected override DbSet<GroupEntity> Groups => Context.Groups;
}

