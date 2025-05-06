using ContactForm.API.Constants;
using ContactForm.API.Extensions;
using FastEndpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();
builder.Services.ConfigureAppCors(builder.Configuration);
builder.Services.ConfigureAppOptions(builder.Configuration);
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(AppConstant.CorsPolicyName);

app.UseHttpsRedirection();

app.MapGet("/", () => "Contact Form API is running!");

app.MapHealthChecks("/health");

app.UseFastEndpoints();

app.Run();
