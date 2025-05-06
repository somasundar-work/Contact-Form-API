using ContactForm.API.Models;
using FastEndpoints;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints();
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.Configure<SmtpInfoOptions>(builder.Configuration.GetSection("smtp"));
builder.Services.Configure<ProfileOptions>(builder.Configuration.GetSection("profile"));
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.MapGet("/", () => "Contact Form API is running!");

app.MapHealthChecks("/health");

app.UseFastEndpoints();

app.Run();
