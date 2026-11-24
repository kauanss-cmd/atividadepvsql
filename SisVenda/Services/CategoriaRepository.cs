using System.Collections.ObjectModel;
using Microsoft.Maui.Storage;
using SQLite;
using SisVenda.Models;

namespace SisVenda.Services
{
    public static class CategoriaRepository
    {
        static readonly SQLiteAsyncConnection connection =
            new(Path.Combine(FileSystem.AppDataDirectory, "sisvenda.db3"));
        static bool initialized;

        public static ObservableCollection<Categoria> Categorias { get; } = new();

        public static Categoria? GetById(int id) => Categorias.FirstOrDefault(c => c.Id == id);

        public static async Task InitializeAsync()
        {
            if (initialized)
            {
                return;
            }

            await connection.CreateTableAsync<Categoria>();
            await LoadAsync();
            initialized = true;
        }

        public static async Task<Categoria?> GetByIdAsync(int id)
        {
            await InitializeAsync();
            return await connection.FindAsync<Categoria>(id);
        }

        public static async Task LoadAsync()
        {
            await connection.CreateTableAsync<Categoria>();
            var categorias = await connection.Table<Categoria>().OrderBy(c => c.Nome).ToListAsync();

            Categorias.Clear();
            foreach (var categoria in categorias)
            {
                Categorias.Add(categoria);
            }
        }

        public static async Task SaveAsync(Categoria categoria)
        {
            await InitializeAsync();

            if (categoria.Id > 0)
            {
                await connection.UpdateAsync(categoria);
            }
            else
            {
                await connection.InsertAsync(categoria);
            }

            await LoadAsync();
        }

        public static async Task RemoveAsync(Categoria categoria)
        {
            await InitializeAsync();

            if (categoria.Id <= 0)
            {
                return;
            }

            await connection.DeleteAsync<Categoria>(categoria.Id);
            await LoadAsync();
        }
    }
}
