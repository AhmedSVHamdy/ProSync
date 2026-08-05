using Core; // 👈 1. ضيفنا دي عشان يشوف AddCoreServices
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);
// أ) البنية التحتية (Database, Identity, Repositories, SignalR)
builder.Services.AddInfrastructureServices(builder.Configuration);

// ب) قلب المشروع (Services, AutoMapper, Validators) 👈 2. ضيفنا السطر المهم ده
builder.Services.AddCoreServices(builder.Configuration);


// Add services to the container.

builder.Services.AddControllers();
 // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
 builder.Services.AddOpenApi();

 var app = builder.Build();

 // Configure the HTTP request pipeline.
 if (app.Environment.IsDevelopment())
 {
     app.MapOpenApi();
 }

 app.UseHttpsRedirection();

 app.UseAuthorization();


 app.MapControllers();

 app.Run();
        