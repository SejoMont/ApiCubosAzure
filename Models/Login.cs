using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCubosAzure.Models
{
    public class Login
    {
        public string Email { get; set; }
        public string Pass { get; set; }
    }
}
