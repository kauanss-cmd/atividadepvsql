using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using SQLite;
using SisVenda.Models;

namespace SisVenda.Services
{
    public static class ClienteRepository
    {
        static readonly SQLiteAsyncConnection connection =
            new(Path.Combine(FileSystem.AppDataDirectory, "sisvenda.db3"));
        static bool initialized;

        public static ObservableCollection<Cliente> Clientes { get; } = new ObservableCollection<Cliente>();

        public static Cliente? GetById(int id) => Clientes.FirstOrDefault(c => c.Id == id);

        public static async Task InitializeAsync()
        {
            if (initialized)
            {
                return;
            }

            await connection.CreateTableAsync<ClienteEntity>();
            await LoadAsync();
            initialized = true;
        }

        public static async Task<Cliente?> GetByIdAsync(int id)
        {
            await InitializeAsync();

            var entity = await connection.FindAsync<ClienteEntity>(id);
            return entity is null ? null : ToModel(entity);
        }

        public static async Task SaveAsync(Cliente cliente)
        {
            await InitializeAsync();

            var entity = ToEntity(cliente);

            if (entity.Id > 0)
            {
                await connection.UpdateAsync(entity);
            }
            else
            {
                await connection.InsertAsync(entity);
                cliente.Id = entity.Id;
            }

            UpsertInCollection(ToModel(entity));
        }

        public static async Task RemoveAsync(Cliente cliente)
        {
            await InitializeAsync();

            if (cliente.Id <= 0)
            {
                return;
            }

            await connection.DeleteAsync<ClienteEntity>(cliente.Id);

            var existing = GetById(cliente.Id);
            if (existing is not null)
            {
                Clientes.Remove(existing);
            }
        }

        static async Task LoadAsync()
        {
            var entities = await connection.Table<ClienteEntity>().OrderBy(x => x.Nome).ToListAsync();

            Clientes.Clear();
            foreach (var entity in entities)
            {
                Clientes.Add(ToModel(entity));
            }
        }

        static void UpsertInCollection(Cliente cliente)
        {
            var existing = GetById(cliente.Id);
            if (existing is null)
            {
                Clientes.Add(cliente);
                return;
            }

            var index = Clientes.IndexOf(existing);
            if (index >= 0)
            {
                Clientes[index] = cliente;
            }
        }

        static ClienteEntity ToEntity(Cliente cliente)
        {
            return new ClienteEntity
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                CNPJ = cliente.CNPJ,
                CPF = cliente.CPF,
                RG = cliente.RG,
                Telefone = cliente.Telefone,
                Celular = cliente.Celular,
                Email = cliente.Email,
                Endereco = cliente.Endereco,
                Numero = cliente.Numero,
                Complemento = cliente.Complemento,
                Bairro = cliente.Bairro,
                CEP = cliente.CEP,
                CidadeId = cliente.Cidade?.Id,
                CidadeNome = cliente.Cidade?.Nome,
                EstadoId = cliente.Cidade?.Estado?.Id,
                EstadoSigla = cliente.Cidade?.Estado?.Sigla,
                EstadoNome = cliente.Cidade?.Estado?.Nome
            };
        }

        static Cliente ToModel(ClienteEntity entity)
        {
            Estado? estado = null;
            if (entity.EstadoId.HasValue || !string.IsNullOrWhiteSpace(entity.EstadoNome) || !string.IsNullOrWhiteSpace(entity.EstadoSigla))
            {
                estado = new Estado
                {
                    Id = entity.EstadoId ?? 0,
                    Nome = entity.EstadoNome,
                    Sigla = entity.EstadoSigla
                };
            }

            Cidade? cidade = null;
            if (entity.CidadeId.HasValue || !string.IsNullOrWhiteSpace(entity.CidadeNome))
            {
                cidade = new Cidade
                {
                    Id = entity.CidadeId ?? 0,
                    Nome = entity.CidadeNome,
                    Estado = estado
                };
            }

            return new Cliente
            {
                Id = entity.Id,
                Nome = entity.Nome,
                CNPJ = entity.CNPJ,
                CPF = entity.CPF,
                RG = entity.RG,
                Telefone = entity.Telefone,
                Celular = entity.Celular,
                Email = entity.Email,
                Endereco = entity.Endereco,
                Numero = entity.Numero,
                Complemento = entity.Complemento,
                Bairro = entity.Bairro,
                CEP = entity.CEP,
                Cidade = cidade
            };
        }

        class ClienteEntity
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }
            public string? Nome { get; set; }
            public string? CNPJ { get; set; }
            public string? CPF { get; set; }
            public string? RG { get; set; }
            public string? Telefone { get; set; }
            public string? Celular { get; set; }
            public string? Email { get; set; }
            public string? Endereco { get; set; }
            public string? Numero { get; set; }
            public string? Complemento { get; set; }
            public string? Bairro { get; set; }
            public string? CEP { get; set; }
            public int? CidadeId { get; set; }
            public string? CidadeNome { get; set; }
            public int? EstadoId { get; set; }
            public string? EstadoSigla { get; set; }
            public string? EstadoNome { get; set; }
        }
    }
}
