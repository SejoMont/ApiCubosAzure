using ApiCubosAzure.Models;
using ApiCubosAzure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace ApiCubosAzure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CubosController : ControllerBase
    {
        private RepositoryCubos repo;

        public CubosController(RepositoryCubos repo)
        {
            this.repo = repo;
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<List<Cubo>>> GetCubos()
        {
            return await this.repo.GetCubos();
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<List<Cubo>>> GetCubosMarca(string marca)
        {
            return await this.repo.GetCubosMarca(marca);
        }

        [Authorize]
        [HttpGet]
        [Route("[action]")]
        public async Task<ActionResult<List<CompraCubo>>> GetComprasCubos()
        {
            Claim claim = HttpContext.User.FindFirst(x => x.Type == "UserData");


            string jsonUser = claim.Value;
            Usuario user = JsonConvert.DeserializeObject<Usuario>(jsonUser);


            return await this.repo.GetComprasCubos(user.IdUsuario);
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<IActionResult> ComprarCubo([FromBody] CompraCubo compra)
        {
            Claim claim = HttpContext.User.FindFirst(x => x.Type == "UserData");

            string jsonUser = claim.Value;
            Usuario user = JsonConvert.DeserializeObject<Usuario>(jsonUser);

            int lastId = await repo.GetUltimoIdCompra() + 1;

            compra.IdPedido = lastId;
            compra.IdUsuario = user.IdUsuario;


            await repo.ComprarCuboAsync(compra);
            return Ok();

        }

    }
}
