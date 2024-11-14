using EmailHubApp.Manager;
using EmailHubApp.Manager.Contract;
using EmailHubApp.Repository;
using EmailHubApp.Repository.Contract;
using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

builder.Services.Configure<IISServerOptions>(options => {options.AllowSynchronousIO = true;});
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

//AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

//AddTransient
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IUserManager, UserManager>();

builder.Services.AddTransient<IUserTypeRepository, UserTypeRepository>();
builder.Services.AddTransient<IUserTypeManager, UserTypeManager>();

builder.Services.AddTransient<IOperationsRepository, OperationsRepository>();
builder.Services.AddTransient<IOperationsManager, OperationsManager>();

builder.Services.AddTransient<ISearchQueryRepository, SearchQueryRepository>();
builder.Services.AddTransient<ISearchQueryManager, SearchQueryManager>();

builder.Services.AddTransient<ISearchedDataRepository, SearchedDataRepository>();
builder.Services.AddTransient<ISearchedDataManager, SearchedDataManager>();

builder.Services.AddTransient<ISearchRequirementsRepository, SearchRequirementsRepository>();
builder.Services.AddTransient<ISearchRequirementsManager, SearchRequirementsManager>();

#region Session Setup 1
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(20);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });
#endregion

var app = builder.Build();

#region Session Setup 2
    app.UseSession();
#endregion

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
