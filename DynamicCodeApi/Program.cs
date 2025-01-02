using Infra.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add KVS implementation to the container
builder.Services.AddSingleton<IKeyValueStore, RedisKVS>();

// Add services to the container
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();