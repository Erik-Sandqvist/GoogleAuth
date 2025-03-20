using Api.Data;
using Api.Services;
using Api.TokenHandler;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
//builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>
    (o => o.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Api.TokenHandler.Constant.Scheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddScheme<AuthenticationSchemeOptions,
    GoogleAccessTokenAuthenticationHandler>(Api.TokenHandler.Constant.Scheme, null)
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Google:ClientId"]!;
    options.ClientSecret = builder.Configuration["Google:ClientSecret"]!;
    //options.SaveTokens = true;
    options.CallbackPath = $"/{builder.Configuration["Google:RedirectUri"]}";
    //options.CallbackPath = "/signin-google";     
});

builder.Services.AddAuthorization();
builder.Services.AddScoped<IGoogleAuthHelper, GoogleAuthHelperService>();
builder.Services.AddScoped<IGoogleAuthorization, GoogleAuthorizationService>();
var app = builder.Build();

app.UseCors(builder =>
{
    builder.AllowAnyHeader()
    .AllowAnyMethod()
    .WithOrigins("https://localhost:7210");
});

if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.UseSwagger(); 
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication().UseAuthorization();

app.MapControllers();

app.Run();
