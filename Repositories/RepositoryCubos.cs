using ApiCubosAzure.Data;
using ApiCubosAzure.Models;
using Azure.Security.KeyVault.Secrets;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace ApiCubosAzure.Repositories
{
    public class RepositoryCubos
    {
        private CubosContext context;
        private SecretClient secretClient;

        public RepositoryCubos(CubosContext context, SecretClient secret)
        {
            this.context = context;
            this.secretClient = secret;
        }

        public async Task<List<CompraCubo>> GetComprasCubos(int iduser)
        {
            return await this.context.CompraCubos.Where(x => x.IdUsuario == iduser).ToListAsync();
        }

        public async Task<List<Cubo>> GetCubos()
        {
            List<Cubo> cubos = await this.context.Cubos.ToListAsync();

            KeyVaultSecret secret = await this.secretClient.GetSecretAsync("blobsmc");

            string bloburl = secret.Value;

            foreach (var cubo in cubos)
            {
                cubo.Imagen = bloburl + "cubos/" + cubo.Imagen;
            }

            return cubos;
        }

        public async Task<List<Cubo>> GetCubosMarca(string marca)
        {
            List<Cubo> cubos = await this.context.Cubos.Where(x => x.Marca == marca).ToListAsync();

            KeyVaultSecret secret = await this.secretClient.GetSecretAsync("blobsmc");

            string bloburl = secret.Value;

            foreach (var cubo in cubos)
            {
                cubo.Imagen = bloburl + "cubos/" + cubo.Imagen;
            }
            return cubos;
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
