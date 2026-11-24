namespace SisVenda.Models
{
    public class Estado
    {
        public int Id { get; set; }
        public string? Sigla { get; set; }
        public string? Nome { get; set; }
        public List<Cidade>? Cidades { get; set; }

        public string NomeComSigla => string.IsNullOrWhiteSpace(Sigla)
            ? Nome ?? string.Empty
            : $"{Nome} ({Sigla})";
    }
}
