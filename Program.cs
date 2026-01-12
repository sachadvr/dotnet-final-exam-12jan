using dotnet.Components;
using dotnet.Endpoints;
using dotnet.Models;
using dotnet.Services;

if (args.Length > 0 && args[0] == "--maze")
{
    if (args.Length < 2)
    {
        Console.Error.WriteLine("dotnet run -- --maze file.txt");
        return;
    }

    var mazePath = Path.GetFullPath(args[1]);
    if (!File.Exists(mazePath))
    {
        Console.Error.WriteLine($"Fichier pas trouvé: {mazePath}");
        return;
    }

    try
    {
        var input = File.ReadAllText(mazePath);
        var maze = new Maze(input);
        var distance = maze.GetDistance();
        var path = maze.GetShortestPath();

        Console.WriteLine($"Distance: {distance}");
        Console.WriteLine("Chemin:");
        foreach (var (row, col) in path)
        {
            Console.WriteLine($"{row},{col}");
        }
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Maze processing failed: {ex.Message}");
    }

    return;
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Enregistrement des services
builder.Services.AddSingleton<IProductService, ProductService>();
builder.Services.AddSingleton<IDiscountService, DiscountService>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Mapping des endpoints de l'API
app.MapProductEndpoints();
app.MapOrderEndpoints();

app.Run();
