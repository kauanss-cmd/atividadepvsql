using System.Text.Json.Serialization;

namespace SisVenda.Models
{
    public class Cidade
    {
        public int Id { get; set; }
        public string? Nome { get; set; }

        [JsonIgnore]
        public Estado? Estado { get; set; }
    }
}
