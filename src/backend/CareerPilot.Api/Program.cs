// CareerPilot AI — API composition root.
//
// Phase 1: host bootstrap only. No business logic, no authentication,
// no persistence. Layer registration (AddApplication / AddInfrastructure)
// is wired here in later phases.

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Only self-redirect locally. In containers TLS terminates at the
    // reverse proxy / ingress, so redirecting here has no HTTPS port to
    // target and warns on every request.
    app.UseHttpsRedirection();
}

app.MapHealthChecks("/health");

app.Run();
