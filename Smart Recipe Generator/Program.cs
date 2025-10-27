var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Register application services
builder.Services.AddHttpClient("ai-client");
builder.Services.AddScoped<Smart_Recipe_Generator.Services.IRecipeService, Smart_Recipe_Generator.Services.RecipeService>();
builder.Services.AddScoped<Smart_Recipe_Generator.Services.IAiRecipeService, Smart_Recipe_Generator.Services.AiRecipeService>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
