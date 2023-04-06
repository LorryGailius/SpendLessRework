global using SpendLess.Server.Models;
global using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MudBlazor.Services;
using SpendLess.Server.Services;
using System.Text;
using SpendLess.Server.Extensions;
using Autofac.Extensions.DependencyInjection;
using Autofac;
using Autofac.Extras.DynamicProxy;
using SpendLess.Server.Interceptor;
using SpendLess.Server.Controllers;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<UnhandledExceptionLogger>();
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>(builder =>
    {
        builder.RegisterType<TransactionsController>()
        .EnableInterfaceInterceptors()
        .InterceptedBy(typeof(UnhandledExceptionLogger))
        .InstancePerDependency();
        builder.RegisterType<AuthServices>().As<IAuthServices>()
        .EnableInterfaceInterceptors();
        
        //builder.RegisterType<AuthServices>().As<IAuthServices>()
        //.EnableInterfaceInterceptors().InterceptedBy(typeof(UnhandledExceptionLogger))
        //.InstancePerDependency();
    });

var dir = Environment.CurrentDirectory + "\\Logs\\exceptions-.log";
Log.Logger = new LoggerConfiguration()
                 .WriteTo.File(Environment.CurrentDirectory + "\\Logs\\exceptions-.log", rollingInterval: RollingInterval.Day)
                 .CreateLogger();
// Add services to the container.
builder.Services.AddMudServices();
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpClient();
builder.Services.AddDbContext<SpendLessContext>();
builder.Services.AddScoped<IAuthServices, AuthServices>();
builder.Services.AddScoped<IDatabaseService, DatabaseService>();
builder.Services.AddScoped<ITransactionsService, TransactionsService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();



app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

//app.UseRouting();


app.MapRazorPages();

app.UseAuthentication();
//app.UseStaticFiles();
app.UseAuthorization();
app.MapBlazorHub();
app.UseRateLimiting();
app.MapControllers();
/*app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();

});*/

app.MapFallbackToFile("index.html");
app.Run();