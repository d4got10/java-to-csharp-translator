using Shared.Logs;
using SyntaxAnalysis;

var builder = WebApplication.CreateBuilder();

// Add services to the container.
builder.Services.AddControllersWithViews();

var logger = new LazyLogger();
builder.Services.AddScoped<LazyLogger>();
builder.Services.AddScoped<SyntaxAnalyzer>(sp => new SyntaxAnalyzer(args[0], args[1], sp.GetService<LazyLogger>()!));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

//app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();