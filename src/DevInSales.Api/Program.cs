using DevInSales.Core.Data.Context;
using DevInSales.Core.Entities;
using DevInSales.Core.Interfaces;
using DevInSales.Core.Services;
using DevInSales.EFCoreApi.Core.Interfaces;
using DevInSales.Identity.Data;
using DevInSales.Identity.Services;
using DevInSales.Application.Interfaces.Services;
using DevInSales.Api;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Identity;
using DevInSales.Identity.Config;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQLConnection"))
);
builder.Services.AddDbContext<IdentityDataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQLConnection"))
);

builder.Services.AddDefaultIdentity<IdentityUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<IdentityDataContext>()
                .AddDefaultTokenProviders();

builder.Services.AddScoped<IIdentityService, IdentityService>();            
builder.Services.AddScoped<ISaleService, SaleService>();
builder.Services.AddScoped<IDeliveryService, DeliveryService>();
builder.Services.AddScoped<ISaleProductService, SaleProductService>();
builder.Services.AddScoped<IStateService, StateService>();
builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IUserService, UserService>();

var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration.GetSection("JwtOptions:SecurityKey").Value));

// Authentication Setup: 
builder.Services.Configure<JwtOptions>(options => 
{
    var jwtAppSettingOptions = builder.Configuration.GetSection(nameof(JwtOptions));

    options.Issuer = jwtAppSettingOptions[nameof(JwtOptions.Issuer)];
    options.Audience = jwtAppSettingOptions[nameof(JwtOptions.Audience)];
    options.SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);
    options.AccessTokenExpiration = int.Parse(jwtAppSettingOptions[nameof(JwtOptions.AccessTokenExpiration)] ?? "0");
    options.RefreshTokenExpiration = int.Parse(jwtAppSettingOptions[nameof(JwtOptions.RefreshTokenExpiration)] ?? "0");
});

builder.Services.Configure<IdentityOptions>(options => 
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
});

builder.Services.AddAuthentication(options => 
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => 
{
    var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration.GetSection("JwtOptions:Issuer").Value,
            
            ValidateAudience = true,
            ValidAudience = builder.Configuration.GetSection("JwtOptions:Audience").Value,
            
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = securityKey,
            RequireExpirationTime = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    
    options.TokenValidationParameters = tokenValidationParameters;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DevInSales API",
        Version = "v1",
        Description = "Projeto 2 do módulo 2 do curso DevInHouse da turma WPP",
        Contact = new OpenApiContact
        {
            Name = "Turma WPP",
            Url = new Uri("https://github.com/DEVin-Way2-Pixeon-Paradigma/M2P2-DEVinSales")
        }
    });
    var xmlFile = "DevInSales.Api.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseCors(builder => builder
    .SetIsOriginAllowed(origin => true)
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());
app.Run();
