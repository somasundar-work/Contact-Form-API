using System.Threading.RateLimiting;
using ContactForm.API.Constants;
using ContactForm.API.Extensions;
using FastEndpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();
builder.Services.ConfigureAppCors(builder.Configuration);
builder.Services.ConfigureAppOptions(builder.Configuration);
builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ApiVersionReader = new HeaderApiVersionReader("X-Api-Version");
});
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }
        )
    );
});
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.useGlobalErrorHandling();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseCors(AppConstant.CorsPolicyName);

app.MapGet("/", () => "Contact Form API is running!")
    .WithName("GetRoot")
    .WithTags("Root Api")
    .Produces<string>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

app.MapHealthChecks("/health");

app.UseFastEndpoints();

app.Run();
