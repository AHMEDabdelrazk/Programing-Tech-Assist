using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TechRoad.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers & JSON options
builder.Services.AddControllers();

// Services & Dependency Injection
builder.Services.AddSingleton<IPasswordHasherService, PasswordHasherService>();
builder.Services.AddSingleton<ITokenService, JwtTokenService>();
builder.Services.AddSingleton<IJsonStorageService, JsonStorageService>();
builder.Services.AddScoped<IProgressService, ProgressService>();
builder.Services.AddScoped<IRoadmapService, RoadmapService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("React", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// JWT Authentication Configuration
var jwtSecret = builder.Configuration["Jwt:SecretKey"];
if (string.IsNullOrWhiteSpace(jwtSecret))
{
    if (builder.Environment.IsDevelopment())
    {
        jwtSecret = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        builder.Configuration["Jwt:SecretKey"] = jwtSecret;
    }
    else
    {
        throw new InvalidOperationException("Jwt:SecretKey must be configured through user secrets or an environment variable.");
    }
}

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ProgrammingTechAssistAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ProgrammingTechAssistClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Swagger Documentation with Bearer Token Authorization
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Programming Tech Assist API",
        Version = "v1",
        Description = "Programming Tech Assist learning roadmap API with JWT authentication and JSON storage."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter 'Bearer' [space] followed by your JWT token. Example: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6...'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Programming Tech Assist API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("React");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();