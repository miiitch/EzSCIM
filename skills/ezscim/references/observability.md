# Observability Reference (optional)

`IScimOperationCallbacks` lets a host application observe SCIM repository operations (last read, last
update, last delete, etc.) and capture errors, to feed its own monitoring/UI or its own logging
mechanism.

**This is fully optional and purely additive.** It does not replace the `ILogger<T>` logging EzSCIM's
controllers already do internally — both run independently. If you never register a callback, nothing
changes: no extra dependency, no wrapper, no overhead.

---

## Implement a callback

```csharp
public class MyScimMonitoringCallbacks : ScimOperationCallbacksBase
{
    private readonly ILogger<MyScimMonitoringCallbacks> _logger;

    public MyScimMonitoringCallbacks(ILogger<MyScimMonitoringCallbacks> logger) => _logger = logger;

    public override Task OnOperationCompletedAsync(ScimOperationContext context)
    {
        // e.g. update "last sync" timestamps shown in your own admin UI
        MyMetrics.RecordScimOperation(context.ResourceKind, context.OperationKind, context.Duration);
        return Task.CompletedTask;
    }

    public override Task OnErrorAsync(ScimOperationContext context, Exception exception)
    {
        // Relay into your app's own logging/alerting mechanism — this does NOT replace
        // the internal ILogger<T> logging EzSCIM already does in its controllers.
        _logger.LogError(exception, "SCIM {Operation} failed for {Resource} {Id}",
            context.OperationKind, context.ResourceKind, context.ResourceId);
        return Task.CompletedTask;
    }
}
```

`ScimOperationCallbacksBase` provides no-op virtual methods, so you only override what you need. You
can implement `IScimOperationCallbacks` directly instead if you prefer.

`ScimOperationContext` carries: `ResourceKind` (`User`/`Group`), `OperationKind`
(`Read`/`Search`/`Create`/`Update`/`Patch`/`Delete`), `ResourceId` (nullable), `Result` (nullable —
null on error), `Duration`, `Timestamp`.

---

## Register in `Program.cs`

Must be called **after** `IScimRepository` is registered:

```csharp
builder.Services.AddScoped<IScimRepository, MyScimRepository>();

builder.Services.AddScimOperationCallback<MyScimMonitoringCallbacks>();
```

Multiple independent subscribers are supported — call `AddScimOperationCallback` more than once (e.g.
one for metrics, one for audit logging):

```csharp
builder.Services.AddScimOperationCallback<MetricsCallbacks>();
builder.Services.AddScimOperationCallback<AuditLogCallbacks>();
```

Calling `AddScimOperationCallback` before `IScimRepository` is registered throws
`InvalidOperationException` at startup.

---

## What it does under the hood

Registering a callback wraps your `IScimRepository` in an internal decorator that measures each
operation, notifies every registered callback, and — on error — notifies `OnErrorAsync` then rethrows
the **original** exception unchanged (so existing error handling in EzSCIM's controllers is unaffected).
A bug in your own callback is caught and logged internally; it never breaks the SCIM request pipeline.
