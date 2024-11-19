using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiGoogle.Shared.Dados.Banco;
using WebApiGoogle.Shared.Dados.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<WebApiGoogleContext>((options) =>
{
    options
        .UseSqlServer(builder.Configuration
            ["ConnectionStrings:AuthDb"])
        .UseLazyLoadingProxies();
});

builder.Services
    .AddIdentityApiEndpoints<ApplicationUser>()
    .AddEntityFrameworkStores<WebApiGoogleContext>();


builder.Services.AddAuthentication(
    options =>
    {
        //options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    }
)
.AddCookie()
.AddGoogle(googleOptions => {
    googleOptions.ClientId = "";
    googleOptions.ClientSecret = "";
    googleOptions.ClaimActions.MapJsonKey("urn:google:userid", "sub");
    googleOptions.ClaimActions.MapJsonKey("urn:google:email", "email");
    googleOptions.ClaimActions.MapJsonKey("urn:google:profile", "picture");
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options => options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

var app = builder.Build();

app.UseCors(options =>
{
    options.AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader();
});


app.UseAuthentication();
app.UseAuthorization();

app.MapGroup("auth").MapIdentityApi<ApplicationUser>().WithTags("Autorização");

app.MapPost("auth/logout", async ([FromServices] SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Ok();
}).RequireAuthorization().WithTags("Autorização");

app.UseHttpsRedirection();

app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();


