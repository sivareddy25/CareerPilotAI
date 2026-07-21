using CareerPilot.Api.Extensions;
using CareerPilot.Application;
using CareerPilot.Infrastructure;

// CareerPilot AI — API composition root.
//
// Each layer owns its own registration. This file states the composition and
// nothing else: no business logic, no service wiring detail.

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddApi(builder.Configuration);

var app = builder.Build();

app.UseApiPipeline();

app.Run();
