using Core; // 👈 1. ضيفنا دي عشان يشوف AddCoreServices
using FluentValidation.AspNetCore;
using Infrastructure;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);
// أ) البنية التحتية (Database, Identity, Repositories, SignalR)
builder.Services.AddInfrastructureServices(builder.Configuration);

// ب) قلب المشروع (Services, AutoMapper, Validators) 👈 2. ضيفنا السطر المهم ده
builder.Services.AddCoreServices(builder.Configuration);
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ProSync API",
        Version = "v1",
        Description = "Multi-tenant SaaS Project Management Platform"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token in the text box below.\r\n\r\nExample: '12345abcdef'"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});



builder.Services.AddControllers();
 builder.Services.AddOpenApi();

 var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ProSync API v1");
        options.DocumentTitle = "ProSync API Docs";
    });
}

app.UseHttpsRedirection();

 app.UseAuthorization();


 app.MapControllers();

 app.Run();
        