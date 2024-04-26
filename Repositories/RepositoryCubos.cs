using ApiCubosAzure.Data;
using ApiCubosAzure.Models;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace ApiCubosAzure.Repositories
{
    public class RepositoryCubos
    {
        private CubosContext context;

        public RepositoryCubos(CubosContext context)
        {
            this.context = context;
        }

        public async Task<List<CompraCubo>> GetComprasCubos(int iduser)
        {
            return await this.context.CompraCubos.Where(x => x.IdUsuario == iduser).ToListAsync();
        }

        public async Task<List<Cubo>> GetCubos()
        {
            return await this.context.Cubos.ToListAsync();
        }

        public async Task<List<Cubo>> GetCubosMarca(string marca)
        {
            List<Cubo> personajes = await this.context.Cubos.Where(x => x.Marca == marca).ToListAsync();

            return personajes;
        }

        public async Task RegistrarUsuarioAsync(Usuario user)
        {
            this.context.Usuarios.Add(user);
            await this.context.SaveChangesAsync();
        }

        public async Task ComprarCuboAsync(CompraCubo compra)
        {
            this.context.CompraCubos.Add(compra);
            await this.context.SaveChangesAsync();
        }

        public async Task<Usuario> LoginAsync(string email, string pass)
        {
            return await this.context.Usuarios.Where(x => x.Email == email && x.Pass == pass).FirstOrDefaultAsync();
        }

        public async Task<int> GetUltimoId()
        {
            var ultimoId = await this.context.Usuarios
                                            .MaxAsync(p => (int?)p.IdUsuario);

            return ultimoId ?? 1;
        }

        public async Task<int> GetUltimoIdCompra()
        {
            var ultimoId = await this.context.CompraCubos
                                            .MaxAsync(p => (int?)p.IdPedido);

            return ultimoId ?? 1;
        }
    }
}
