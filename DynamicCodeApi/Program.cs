using Infra.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// TODO Add KVS implementation to your controller
// builder.Services.AddSingleton<IKeyValueStore,YourKeyValueStoreImpl>();

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

app.UseHttpsRedirection();
app.MapControllers();

app.Run();