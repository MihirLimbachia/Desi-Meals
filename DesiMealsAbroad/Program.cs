using DesiMealsAbroad.Postgres;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);


var configuration = builder.Configuration;
builder.Services.AddScoped<PostgresQueryRunner>(provider =>
{
    string? connectionString = configuration.GetConnectionString("PostgresConnectionURL");
    return new PostgresQueryRunner(connectionString);
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

