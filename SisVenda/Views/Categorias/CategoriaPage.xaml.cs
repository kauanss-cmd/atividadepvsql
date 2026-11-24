using SisVenda.Services;

namespace SisVenda
{
    public partial class CategoriaPage : ContentPage
    {
        public CategoriaPage()
        {
            InitializeComponent();
            categoriasCollectionView.ItemsSource = CategoriaRepository.Categorias;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CategoriaRepository.InitializeAsync();
        }

        private async void ToolbarItem_Novo_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(CategoriaFormPage));
        }

        private async void Editar_Clicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is int id)
            {
                await Shell.Current.GoToAsync($"{nameof(CategoriaFormPage)}?id={id}");
            }
        }

        private async void Excluir_Clicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is int id)
            {
                var categoria = CategoriaRepository.GetById(id);
                if (categoria is null)
                {
                    return;
                }

                var confirmar = await DisplayAlertAsync("Confirmar", $"Excluir {categoria.Nome}?", "Sim", "Não");
                if (!confirmar)
                {
                    return;
                }

                await CategoriaRepository.RemoveAsync(categoria);
            }
        }
    }
}
