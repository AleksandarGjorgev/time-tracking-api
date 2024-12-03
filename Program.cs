var builder = WebApplication.CreateBuilder(args);

// Dodaj Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dodaj kontrolerje
builder.Services.AddControllers();

var app = builder.Build();

// Nastavitve za aplikacijo
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
