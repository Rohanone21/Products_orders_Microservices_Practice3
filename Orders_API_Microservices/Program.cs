using Microsoft.EntityFrameworkCore;
using Orders_API_Microservices.Data;

var builder = WebApplication.CreateBuilder(args);

// ---------------- CORS FIX ----------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMVC", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
// -------------------------------------------

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("constr")));

builder.Services.AddHttpClient("ProductService", client =>
{
    client.BaseAddress = new Uri("https://localhost:7259/api/products/");
});

builder.Services.AddScoped<ProductService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ---------------- Enable CORS ----------------
app.UseCors("AllowMVC");
// ---------------------------------------------

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
