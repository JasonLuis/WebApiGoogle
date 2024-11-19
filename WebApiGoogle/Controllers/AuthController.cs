using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using WebApiGoogle.Shared.Dados.Models;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;


    public AuthController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;

    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        var redirectUrl = Url.Action(nameof(HandleExternalLogin), "Auth");
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);

        //return Ok(redirectUrl);
    }

    [HttpGet("auth/external-login")]
    public async Task<IActionResult> HandleExternalLogin()
     {
        var result = await HttpContext.AuthenticateAsync();
        if (!result.Succeeded || result.Principal == null)
        {
            return Unauthorized("Falha na autenticação externa.");
        }

        var claims = result.Principal.Claims.ToList();
        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(email))
        {
            return BadRequest("Não foi possível recuperar o email do provedor externo.");
        }

        // Tenta encontrar o usuário no banco de dados
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            // Cria um novo usuário se não existir
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true // Confirmado, pois vem do Gmail
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                return BadRequest(createResult.Errors);
            }

            // Associa o perfil de acesso ao usuário
            var roleResult = await _userManager.AddToRoleAsync(user, "DefaultProfile"); // Troque para o nome do seu perfil
            if (!roleResult.Succeeded)
            {
                return BadRequest(roleResult.Errors);
            }
        }

        // Garante que o login externo está associado ao usuário
        var authResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
        if (!authResult.Succeeded || authResult.Principal == null)
        {
            Console.WriteLine("Falha na autenticação externa via AuthenticateAsync.");
            return Unauthorized("Erro ao processar autenticação.");
        }

        // Caso funcione, converta as informações para uso posterior
        var info = new ExternalLoginInfo(authResult.Principal, GoogleDefaults.AuthenticationScheme, authResult.Principal.Identity.Name, "Google");
        //var info = await _signInManager.GetExternalLoginInfoAsync();
        //if (info == null)
        //{
        //    return Unauthorized("Erro ao associar login externo.");
        //}


        var loginResult = await _userManager.AddLoginAsync(user, info);
        var profile = await _userManager.IsInRoleAsync(user, "DefaultProfile");
        if (!loginResult.Succeeded && !profile)
        {
            return BadRequest(loginResult.Errors);
        }

        // Faz o login do usuário
        await _signInManager.SignInAsync(user, isPersistent: false);

        return Ok("Login realizado com sucesso.");
    }
}