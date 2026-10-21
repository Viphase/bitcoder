// Точка входа: собирает приложение и связывает адреса с контроллерами.
// Здесь нет ни расчётов, ни разметки.

var builder = WebApplication.CreateBuilder(args);

// MVC: контроллеры + представления Razor (.cshtml).
builder.Services.AddControllersWithViews();

// Адреса в нижнем регистре: /lesson, /practice.
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/home/error");
}

// Для 404 и других кодов без страницы показываем нашу страницу ошибки.
app.UseStatusCodePagesWithReExecute("/home/error", "?code={0}");

app.UseStaticFiles();   // wwwroot/css/site.css
app.UseRouting();

// Единственный маршрут: /{контроллер}/{действие}. Пустой адрес — Home/Index.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}");

app.Run();
