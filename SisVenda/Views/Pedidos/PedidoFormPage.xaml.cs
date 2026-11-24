using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.Maui.Controls;
using SisVenda.Models;
using SisVenda.Services;

namespace SisVenda
{
    [QueryProperty(nameof(PedidoId), "id")]
    public partial class PedidoFormPage : ContentPage
    {
        int pedidoId = 0;
        readonly ObservableCollection<ItemPedido> itensPedido = [];

        public string PedidoId
        {
            get => pedidoId.ToString();
            set
            {
                if (int.TryParse(value, out var id))
                {
                    pedidoId = id;
                }
            }
        }

        public PedidoFormPage()
        {
            InitializeComponent();
            itensCollectionView.ItemsSource = itensPedido;
            dataPicker.Date = DateTime.Today;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await ClienteRepository.InitializeAsync();

            clientePicker.ItemsSource = ClienteRepository.Clientes;
            produtoPicker.ItemsSource = ProdutoRepository.Produtos;

            if (pedidoId > 0)
            {
                var pedido = PedidoRepository.GetById(pedidoId);
                if (pedido != null)
                {
                    PopulateForm(pedido);
                    return;
                }
            }

            ClearForm();
        }

        private async void AdicionarItem_Clicked(object sender, EventArgs e)
        {
            if (produtoPicker.SelectedItem is not Produto produto)
            {
                await DisplayAlert("Validação", "Selecione um produto.", "OK");
                return;
            }

            if (!int.TryParse(qtdEntry.Text, out var qtd) || qtd <= 0)
            {
                await DisplayAlert("Validação", "Quantidade inválida.", "OK");
                return;
            }

            if (!TryParseValor(valorEntry.Text, out var valor) || valor <= 0)
            {
                await DisplayAlert("Validação", "Valor unitário inválido.", "OK");
                return;
            }

            itensPedido.Add(new ItemPedido(produto, qtd, valor));
            qtdEntry.Text = string.Empty;
            valorEntry.Text = string.Empty;
            produtoPicker.SelectedIndex = -1;
            AtualizarTotal();
        }

        private void RemoverItem_Clicked(object sender, EventArgs e)
        {
            if (sender is Button b && b.CommandParameter is ItemPedido item)
            {
                itensPedido.Remove(item);
                AtualizarTotal();
            }
        }

        private async void Salvar_Clicked(object sender, EventArgs e)
        {
            var dataPedido = dataPicker.Date ?? DateTime.Today;

            if (clientePicker.SelectedItem is not Cliente cliente)
            {
                await DisplayAlert("Validação", "Selecione um cliente.", "OK");
                return;
            }

            if (!itensPedido.Any())
            {
                await DisplayAlert("Validação", "Adicione ao menos um item ao pedido.", "OK");
                return;
            }

            if (pedidoId > 0)
            {
                var pedido = PedidoRepository.GetById(pedidoId);
                if (pedido != null)
                {
                    pedido.Data = dataPedido;
                    pedido.Cliente = cliente;
                    pedido.Observacao = observacaoEditor.Text?.Trim() ?? string.Empty;
                    pedido.Itens = itensPedido
                        .Select(i => new ItemPedido(i.Produto, i.Qtd, i.Valor))
                        .ToList();
                }
            }
            else
            {
                var novo = new Pedido(
                    PedidoRepository.Pedidos.Any() ? PedidoRepository.Pedidos.Max(x => x.Id) + 1 : 1,
                    dataPedido,
                    cliente)
                {
                    Observacao = observacaoEditor.Text?.Trim() ?? string.Empty,
                    Itens = itensPedido
                        .Select(i => new ItemPedido(i.Produto, i.Qtd, i.Valor))
                        .ToList()
                };

                PedidoRepository.Add(novo);
            }

            await Shell.Current.GoToAsync("..");
        }

        private async void Cancelar_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }

        static bool TryParseValor(string? texto, out decimal valor)
        {
            if (decimal.TryParse(texto, NumberStyles.Number, new CultureInfo("pt-BR"), out valor))
            {
                return true;
            }

            return decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out valor);
        }

        void PopulateForm(Pedido pedido)
        {
            dataPicker.Date = pedido.Data;
            observacaoEditor.Text = pedido.Observacao;

            clientePicker.SelectedItem = ClienteRepository.GetById(pedido.Cliente.Id) ?? pedido.Cliente;

            itensPedido.Clear();
            foreach (var item in pedido.Itens)
            {
                var produto = ProdutoRepository.GetById(item.Produto.Id) ?? item.Produto;
                itensPedido.Add(new ItemPedido(produto, item.Qtd, item.Valor));
            }

            produtoPicker.SelectedIndex = -1;
            qtdEntry.Text = string.Empty;
            valorEntry.Text = string.Empty;
            AtualizarTotal();
        }

        void ClearForm()
        {
            clientePicker.SelectedIndex = -1;
            dataPicker.Date = DateTime.Today;
            observacaoEditor.Text = string.Empty;
            produtoPicker.SelectedIndex = -1;
            qtdEntry.Text = string.Empty;
            valorEntry.Text = string.Empty;
            itensPedido.Clear();
            AtualizarTotal();
        }

        void AtualizarTotal()
        {
            var total = itensPedido.Sum(i => i.ValorTotal);
            totalLabel.Text = $"Total: {total:C}";
        }
    }
}
