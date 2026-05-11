using BancoInvest.Data;
using BancoInvest.Models;
using BancoInvest.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

// 🔹 BANCO
builder.Services.AddDbContext<BancoInvestContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("ConexaoPadrao"),
        ServerVersion.AutoDetect(
            builder.Configuration.GetConnectionString("ConexaoPadrao")
        )));

builder.Services.AddScoped<SeedService>();

// 🔹 CONFIGURAÇÃO DE CONTROLLERS E JSON (Essencial para Models cheias)
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// 🔹 SESSION
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 🔹 IDENTITY
builder.Services.AddIdentity<Usuario, IdentityRole>()
    .AddEntityFrameworkStores<BancoInvestContext>()
    .AddDefaultTokenProviders();

// 🔐 JWT
var key = Encoding.UTF8.GetBytes("BancoInvest_Super_Secret_Key_2026_!@#_JWT_32chars_min");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

// 📄 SWAGGER CLÁSSICO (Para evitar o 404 e funcionar com Swagger UI)
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Banco Invest API",
        Version = "v1"
    });

    // 🔐 CONFIG JWT NO SWAGGER
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Digite: Bearer {seu_token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
});

var app = builder.Build();

// 📄 PIPELINE SWAGGER
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Gera o JSON
    app.UseSwaggerUI(); // Interface visual padrão em /swagger
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

// 🌱 SEED
using (var scope = app.Services.CreateScope())
{
    var seed = scope.ServiceProvider.GetRequiredService<SeedService>();
    await seed.SeedAsync();
}

app.Run();