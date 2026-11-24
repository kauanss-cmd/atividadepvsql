using SisVenda.Models;
using SisVenda.Services;

namespace SisVenda
{
    [QueryProperty(nameof(CategoriaId), "id")]
    public partial class CategoriaFormPage : ContentPage
    {
        int categoriaId;

        public string CategoriaId
        {
            get => categoriaId.ToString();
            set
            {
                if (int.TryParse(value, out var id))
                {
                    categoriaId = id;
                }
            }
        }

        public CategoriaFormPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CategoriaRepository.InitializeAsync();

            if (categoriaId > 0)
            {
                var categoria = await CategoriaRepository.GetByIdAsync(categoriaId);
                if (categoria is not null)
                {
                    nomeEntry.Text = categoria.Nome;
                    return;
                }
            }

            nomeEntry.Text = string.Empty;
        }

        private async void Salvar_Clicked(object sender, EventArgs e)
        {
            var nome = nomeEntry.Text?.Trim();

            if (string.IsNullOrWhiteSpace(nome))
            {
                await DisplayAlertAsync("Validação", "Nome é obrigatório.", "OK");
                return;
            }

            var categoria = categoriaId > 0
                ? await CategoriaRepository.GetByIdAsync(categoriaId) ?? new Categoria { Id = categoriaId }
                : new Categoria();

            categoria.Nome = nome;
            await CategoriaRepository.SaveAsync(categoria);

            await Shell.Current.GoToAsync("..");
        }

        private async void Cancelar_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
