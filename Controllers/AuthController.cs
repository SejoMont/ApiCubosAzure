using ApiCubosAzure.Helpers;
using ApiCubosAzure.Models;
using ApiCubosAzure.Repositories;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Numerics;
using System.Security.Claims;
using static System.Reflection.Metadata.BlobBuilder;

namespace ApiCubosAzure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private RepositoryCubos repo;
        private HelperActionServicesOAuth helper;
        private SecretClient secretClient;

        public AuthController(RepositoryCubos repo,
           HelperActionServicesOAuth helper, SecretClient secret)
        {
            this.repo = repo;
            this.helper = helper;
            this.secretClient = secret;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult> Login(Login model)
        {
            //buscamos al empleado en nuestro repo
            Usuario user = await this.repo.LoginAsync(model.Email, model.Pass);

            if (user == null)
            {
                return Unauthorized();
            }
            else
            {
                SigningCredentials credentials =
                    new SigningCredentials(
                        this.helper.GetKeyToken(),
                        SecurityAlgorithms.HmacSha256);

                string jsonUser =
                    JsonConvert.SerializeObject(user);

                Claim[] info = new[]
                {
                    new Claim("UserData", jsonUser)
                };

                JwtSecurityToken token = new JwtSecurityToken(
                    claims: info,
                    issuer: this.helper.Issuer,
                    audience: this.helper.Audience,
                    signingCredentials: credentials,
                    expires: DateTime.UtcNow.AddMinutes(30),
                    notBefore: DateTime.UtcNow
                    );

                return Ok(
                    new
                    {
                        response =
                        new JwtSecurityTokenHandler()
                        .WriteToken(token)
                    });
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Registro(Usuario user)
        {
            int lastId = await repo.GetUltimoId() + 1;

            user.IdUsuario = lastId;

            await this.repo.RegistrarUsuarioAsync(user);
            return Ok();
        }

        [Authorize]
        [HttpGet]
        [Route("[action]")]
        public async Task<ActionResult<Usuario>> Perfil()
        {
            Claim claim = HttpContext.User.FindFirst(x => x.Type == "UserData");

            string jsonUser = claim.Value;
            Usuario user = JsonConvert.DeserializeObject<Usuario>(jsonUser);

            KeyVaultSecret secret = await this.secretClient.GetSecretAsync("blobsmc");

            string bloburl = secret.Value;

            user.Imagen = bloburl + "usuarios/" + user.Imagen;
            return user;

        }
    }
}