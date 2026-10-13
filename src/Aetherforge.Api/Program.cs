using Aetherforge.Domain;
using Aetherforge.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(builder.Configuration["listen"] ?? "http://127.0.0.1:5080");
var stateFile = builder.Configuration["state-file"] ?? Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../data/integration-state.json"));
builder.Services.AddSingleton(new IntegrationStore(stateFile));
builder.Services.AddHostedService<IdleWorker>();
var app = builder.Build();
app.MapGet("/health", () => Results.Ok(new { status = "ok", milestone = "integration", authority = "local-test-account" }));
app.MapGet("/prototype/team", () => Results.Ok(StarterParty.Create().Select(p => new { name = p.Name, role = p.Role.ToString(), hp = p.MaxHealth, attack = p.Attack, defense = p.Defense })));
app.MapGet("/integration/state", (IntegrationStore store) => Results.Ok(store.Snapshot()));
app.MapPost("/integration/idle/heartbeat", (IntegrationStore store) => { store.Heartbeat(DateTimeOffset.UtcNow); return Results.Ok(store.Snapshot()); });
foreach (var action in new[] { "transfer", "equip", "unequip", "hit" })
    app.MapPost("/integration/" + action, (ItemCommand command, IntegrationStore store) =>
    {
        try { return Results.Ok(store.Execute(action, command)); }
        catch (InvalidOperationException error) { return Results.Conflict(new { message = error.Message }); }
    });
app.Run();

sealed class IdleWorker(IntegrationStore store) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        while (await timer.WaitForNextTickAsync(stoppingToken)) store.Tick(DateTimeOffset.UtcNow);
    }
}
