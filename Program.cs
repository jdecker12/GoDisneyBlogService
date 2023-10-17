using GoDisneyBlog.Data;
using GoDisneyBlog.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json.Serialization;
using AutoMapper;
using Microsoft.AspNetCore.CookiePolicy;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
//    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

var _config = builder.Configuration.GetSection("Tokens");
var _configConnect = builder.Configuration;
builder.Services.AddDbContext<GoDisneyContext>(cfg =>
{
    cfg.UseSqlServer(_configConnect.GetConnectionString("GoDisneyConnectionStringDev"));
});


builder.Services.AddIdentity<StoreUser, IdentityRole>(cfg =>
{
    cfg.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<GoDisneyContext>();

builder.Services.AddAuthentication()
                .AddCookie()
                .AddJwtBearer(cfg =>
                {
                    cfg.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = _configConnect["Tokens:Issuer"],
                        ValidAudience = _configConnect["Tokens:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configConnect["Tokens:Key"]))
                    };
                });

builder.Services.Configure<CookiePolicyOptions>(options =>
{

    options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
    options.OnAppendCookie = cookieContext =>
        CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
    options.OnDeleteCookie = cookieContext =>
        CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
});
var mappingConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new GoDisneyMappingProfile());
});

IMapper mapper = mappingConfig.CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWidget", builder =>
    {
        builder.WithOrigins("https://youtube-nocookie.com/")
               .AllowAnyHeader()
               .AllowAnyMethod();
        builder.WithOrigins("http://localhost:4200") 
           .AllowAnyHeader()
           .AllowAnyMethod();
    });
});

//builder.Services.AddAutoMapper(typeof(WebApplication));
builder.Services.AddMvc()
                .AddJsonOptions(opt => opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
builder.Services.AddTransient<GoDisneySeeder>();

builder.Services.AddScoped<IGoDisneyRepository, GoDisneyRepository>();
builder.Services.AddScoped<IImageRepository, ImageRepository>();
builder.Services.AddScoped<IContactRepository, ContactRepository>();


void CheckSameSite(HttpContext httpContext, CookieOptions options)
{
    if (options.SameSite == SameSiteMode.None)
    {
        var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
        if (DisallowsSameSiteNone(userAgent))
        {
            options.SameSite = SameSiteMode.Unspecified;
        }
    }
}

 bool DisallowsSameSiteNone(string userAgent)
{
    // Check if the user agent contains any of the known disallowed strings.
    // For example, some versions of Chrome on iOS disallow SameSite=None.
    // Add more strings as needed.
    return userAgent.Contains("CPU iPhone OS 12")
        || userAgent.Contains("iPad; CPU OS 12")
        || userAgent.Contains("Chrome/5")
        || userAgent.Contains("Chrome/6");
}


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseCors("AllowWidget");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext =  services.GetRequiredService<GoDisneyContext>();
    var seeder = services.GetService<GoDisneySeeder>();

    dbContext.Database.EnsureCreated();
    await seeder!.SeedAsync();  
}

app.Use(async (context, next) =>
{
    await next();

    if (context.Response.StatusCode == 404 && !Path.HasExtension(context.Request.Path.Value))
    {
        context.Request.Path = "/index.html";
        await next();
    }
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
       name: "default",
       pattern: "{controller=Fallback}/{action=Index}/{param?}"
    );
});

app.Run();