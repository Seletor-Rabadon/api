using api.Interfaces;
using api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins("http://localhost:5050")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); 

//dependency injection
builder.Services.AddScoped<IDataBaseService, DatabaseService>();
builder.Services.AddScoped<IRiotService, RiotService>();
builder.Services.AddScoped<IAIService, AIService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api");
        c.RoutePrefix = string.Empty; 
    });
}

// Add CORS middleware (must be before UseAuthorization and MapControllers)
app.UseCors("AllowLocalhost");

app.UseAuthorization();
app.MapControllers();

app.Run();