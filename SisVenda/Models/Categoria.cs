using SQLite;

namespace SisVenda.Models
{
    public class Categoria
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string? Nome { get; set; }
    }
}
