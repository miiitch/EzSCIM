using Microsoft.Extensions.DependencyInjection;
using Moq;
using EzSCIM.Models;
using EzSCIM.Observability;
using EzSCIM.Repositories;
using Shouldly;

namespace EzSCIM.UnitTests.Observability;

public class ObservableScimRepositoryTests
{
    private static (IServiceProvider Provider, Mock<IScimRepository> InnerRepository) BuildProvider(
        Action<IServiceCollection> configureCallbacks)
    {
        var innerRepository = new Mock<IScimRepository>();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(innerRepository.Object);
        configureCallbacks(services);

        return (services.BuildServiceProvider(), innerRepository);
    }

    [Fact]
    public void AddScimOperationCallback_WithoutIScimRepositoryRegistered_Throws()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IScimOperationCallbacks>());

        Should.Throw<InvalidOperationException>(() =>
            services.AddScimOperationCallback(Mock.Of<IScimOperationCallbacks>()));
    }

    [Fact]
    public async Task GetUserAsync_OnSuccess_NotifiesCallbackWithCorrectContext()
    {
        var mockCallback = new Mock<IScimOperationCallbacks>();
        var (provider, innerRepository) = BuildProvider(services => services.AddScimOperationCallback(mockCallback.Object));

        var user = new ScimUser { Id = "42", UserName = "test@example.com" };
        innerRepository.Setup(r => r.GetUserAsync("42")).ReturnsAsync(user);

        var repository = provider.GetRequiredService<IScimRepository>();
        var result = await repository.GetUserAsync("42");

        result.ShouldBe(user);
        mockCallback.Verify(c => c.OnOperationCompletedAsync(It.Is<ScimOperationContext>(ctx =>
            ctx.ResourceKind == ScimResourceKind.User &&
            ctx.OperationKind == ScimOperationKind.Read &&
            ctx.ResourceId == "42" &&
            ReferenceEquals(ctx.Result, user))), Times.Once);
        mockCallback.Verify(c => c.OnErrorAsync(It.IsAny<ScimOperationContext>(), It.IsAny<Exception>()), Times.Never);
    }

    [Fact]
    public async Task CreateGroupAsync_OnSuccess_NotifiesCallbackWithCreateOperationKind()
    {
        var mockCallback = new Mock<IScimOperationCallbacks>();
        var (provider, innerRepository) = BuildProvider(services => services.AddScimOperationCallback(mockCallback.Object));

        var group = new ScimGroup { Id = "g1", DisplayName = "Admins" };
        innerRepository.Setup(r => r.CreateGroupAsync(It.IsAny<ScimGroup>())).ReturnsAsync(group);

        var repository = provider.GetRequiredService<IScimRepository>();
        await repository.CreateGroupAsync(new ScimGroup { DisplayName = "Admins" });

        mockCallback.Verify(c => c.OnOperationCompletedAsync(It.Is<ScimOperationContext>(ctx =>
            ctx.ResourceKind == ScimResourceKind.Group &&
            ctx.OperationKind == ScimOperationKind.Create)), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_WhenInnerThrows_NotifiesOnErrorAndRethrowsOriginalException()
    {
        var mockCallback = new Mock<IScimOperationCallbacks>();
        var (provider, innerRepository) = BuildProvider(services => services.AddScimOperationCallback(mockCallback.Object));

        var thrown = new InvalidOperationException("boom");
        innerRepository.Setup(r => r.DeleteUserAsync("42")).ThrowsAsync(thrown);

        var repository = provider.GetRequiredService<IScimRepository>();

        var caught = await Should.ThrowAsync<InvalidOperationException>(() => repository.DeleteUserAsync("42"));
        caught.ShouldBeSameAs(thrown);

        mockCallback.Verify(c => c.OnErrorAsync(
            It.Is<ScimOperationContext>(ctx =>
                ctx.ResourceKind == ScimResourceKind.User &&
                ctx.OperationKind == ScimOperationKind.Delete &&
                ctx.ResourceId == "42"),
            thrown), Times.Once);
        mockCallback.Verify(c => c.OnOperationCompletedAsync(It.IsAny<ScimOperationContext>()), Times.Never);
    }

    [Fact]
    public async Task OnOperationCompletedAsync_WhenCallbackThrows_ExceptionIsSwallowedAndResultStillReturned()
    {
        var faultyCallback = new Mock<IScimOperationCallbacks>();
        faultyCallback
            .Setup(c => c.OnOperationCompletedAsync(It.IsAny<ScimOperationContext>()))
            .ThrowsAsync(new Exception("callback bug"));

        var (provider, innerRepository) = BuildProvider(services => services.AddScimOperationCallback(faultyCallback.Object));

        var user = new ScimUser { Id = "1", UserName = "u@example.com" };
        innerRepository.Setup(r => r.GetUserAsync("1")).ReturnsAsync(user);

        var repository = provider.GetRequiredService<IScimRepository>();
        var result = await repository.GetUserAsync("1");

        result.ShouldBe(user);
    }

    [Fact]
    public async Task MultipleRegisteredCallbacks_AreAllNotified()
    {
        var firstCallback = new Mock<IScimOperationCallbacks>();
        var secondCallback = new Mock<IScimOperationCallbacks>();

        var (provider, innerRepository) = BuildProvider(services =>
        {
            services.AddScimOperationCallback(firstCallback.Object);
            services.AddScimOperationCallback(secondCallback.Object);
        });

        var user = new ScimUser { Id = "7", UserName = "multi@example.com" };
        innerRepository.Setup(r => r.GetUserAsync("7")).ReturnsAsync(user);

        var repository = provider.GetRequiredService<IScimRepository>();
        await repository.GetUserAsync("7");

        firstCallback.Verify(c => c.OnOperationCompletedAsync(It.IsAny<ScimOperationContext>()), Times.Once);
        secondCallback.Verify(c => c.OnOperationCompletedAsync(It.IsAny<ScimOperationContext>()), Times.Once);
    }

    [Fact]
    public async Task WithoutAnyCallbackRegistered_RepositoryBehavesIdenticallyToInner()
    {
        var services = new ServiceCollection();
        var innerRepository = new Mock<IScimRepository>();
        services.AddSingleton(innerRepository.Object);

        var user = new ScimUser { Id = "1", UserName = "plain@example.com" };
        innerRepository.Setup(r => r.GetUserAsync("1")).ReturnsAsync(user);

        var provider = services.BuildServiceProvider();
        var repository = provider.GetRequiredService<IScimRepository>();

        repository.ShouldBeSameAs(innerRepository.Object);
        (await repository.GetUserAsync("1")).ShouldBe(user);
    }
}
