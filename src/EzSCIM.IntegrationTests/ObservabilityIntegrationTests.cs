using System.Net;
using System.Net.Http.Json;
using EzSCIM.Models;
using EzSCIM.Observability;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace EzSCIM.IntegrationTests;

/// <summary>
/// Verifies <see cref="IScimOperationCallbacks"/> observes real HTTP round-trips through EzSCIM's
/// controllers — in particular Update/Patch/Delete, which have no controller-level try/catch today
/// and were previously unobservable end-to-end (see <c>ScimUsersController</c>).
/// </summary>
[Collection("ObservabilityIntegration")]
public class ObservabilityIntegrationTests(ScimWebApplicationFactory factory)
{
    private (HttpClient Client, RecordingScimOperationCallbacks Recorder) CreateObservedClient()
    {
        var recorder = new RecordingScimOperationCallbacks();
        var observedFactory = factory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => services.AddScimOperationCallback(recorder)));
        return (observedFactory.CreateClient(), recorder);
    }

    private static async Task<string> CreateUserAsync(HttpClient client, string suffix)
    {
        var response = await client.PostAsJsonAsync("/scim/Users", new ScimUser
        {
            UserName = $"observability-{suffix}-{Guid.NewGuid():N}@example.com"
        });
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<ScimUser>();
        created.ShouldNotBeNull();
        return created!.Id!;
    }

    [Fact]
    public async Task UpdateUser_OverHttp_NotifiesUpdateOperationCompleted()
    {
        var (client, recorder) = CreateObservedClient();
        var userId = await CreateUserAsync(client, "update");
        recorder.Clear();

        var response = await client.PutAsJsonAsync($"/scim/Users/{userId}", new ScimUser
        {
            UserName = $"observability-update-changed-{Guid.NewGuid():N}@example.com",
            Active = false
        });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        recorder.Completed.ShouldContain(ctx =>
            ctx.ResourceKind == ScimResourceKind.User &&
            ctx.OperationKind == ScimOperationKind.Update &&
            ctx.ResourceId == userId);
    }

    [Fact]
    public async Task PatchUser_OverHttp_NotifiesPatchOperationCompleted()
    {
        var (client, recorder) = CreateObservedClient();
        var userId = await CreateUserAsync(client, "patch");
        recorder.Clear();

        var patchRequest = new ScimPatchRequest
        {
            Operations =
            [
                new ScimPatchOperation { Op = "replace", Path = "active", Value = false }
            ]
        };
        var response = await client.PatchAsJsonAsync($"/scim/Users/{userId}", patchRequest);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        recorder.Completed.ShouldContain(ctx =>
            ctx.ResourceKind == ScimResourceKind.User &&
            ctx.OperationKind == ScimOperationKind.Patch &&
            ctx.ResourceId == userId);
    }

    [Fact]
    public async Task DeleteUser_OverHttp_NotifiesDeleteOperationCompleted()
    {
        var (client, recorder) = CreateObservedClient();
        var userId = await CreateUserAsync(client, "delete");
        recorder.Clear();

        var response = await client.DeleteAsync($"/scim/Users/{userId}");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        recorder.Completed.ShouldContain(ctx =>
            ctx.ResourceKind == ScimResourceKind.User &&
            ctx.OperationKind == ScimOperationKind.Delete &&
            ctx.ResourceId == userId);
    }

    [Fact]
    public async Task GetUser_WhenNotFoundOverHttp_StillNotifiesOperationCompletedWithNullResult()
    {
        var (client, recorder) = CreateObservedClient();
        recorder.Clear();

        var response = await client.GetAsync("/scim/Users/does-not-exist");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        recorder.Completed.ShouldContain(ctx =>
            ctx.ResourceKind == ScimResourceKind.User &&
            ctx.OperationKind == ScimOperationKind.Read &&
            ctx.ResourceId == "does-not-exist" &&
            ctx.Result == null);
    }

    private sealed class RecordingScimOperationCallbacks : IScimOperationCallbacks
    {
        private readonly List<ScimOperationContext> _completed = [];

        public IReadOnlyList<ScimOperationContext> Completed
        {
            get { lock (_completed) return _completed.ToList(); }
        }

        public Task OnOperationCompletedAsync(ScimOperationContext context)
        {
            lock (_completed) _completed.Add(context);
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(ScimOperationContext context, Exception exception) => Task.CompletedTask;

        public void Clear()
        {
            lock (_completed) _completed.Clear();
        }
    }
}

/// <summary>
/// Collection definition for observability integration tests. Uses its own PostgreSQL container
/// for test isolation from the other integration test collections.
/// </summary>
[CollectionDefinition("ObservabilityIntegration")]
public class ObservabilityIntegrationCollection : ICollectionFixture<ScimWebApplicationFactory>
{
}
