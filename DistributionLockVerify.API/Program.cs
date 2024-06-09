using Medallion.Threading;
using Medallion.Threading.Postgres;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
string connectionString = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=yourpassword;";
builder.Services.AddSingleton<IDistributedLockProvider>(sp =>
    new PostgresDistributedSynchronizationProvider(connectionString));
builder.Services.AddSingleton<IDistributedReaderWriterLockProvider>(sp=> new PostgresDistributedSynchronizationProvider(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
