using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Maui.Controls;
using SisVenda.Models;
using SisVenda.Services;

namespace SisVenda
{
    [QueryProperty(nameof(ClientId), "id")]
    public partial class ClienteFormPage : ContentPage
    {
        int clientId;
        List<Estado> estados = new();
        bool estadosCarregados;

        public string ClientId
        {
            get => clientId.ToString();
            set
            {
                if (int.TryParse(value, out var id))
                {
                    clientId = id;
                }
            }
        }

        public ClienteFormPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await ClienteRepository.InitializeAsync();

            if (!estadosCarregados)
            {
                await CarregarEstadosECidadesAsync();
            }

            if (clientId > 0)
            {
                var cliente = await ClienteRepository.GetByIdAsync(clientId);
                if (cliente != null)
                {
                    PopulateForm(cliente);
                }
            }
            else
            {
                ClearForm();
            }
        }

        async Task CarregarEstadosECidadesAsync()
        {
            try
            {
                await using var stream = await FileSystem.OpenAppPackageFileAsync("estados-cidades.json");
                var lista = await JsonSerializer.DeserializeAsync<List<Estado>>(stream, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                estados = lista?
                    .Where(e => e.Id > 0 && !string.IsNullOrWhiteSpace(e.Nome))
                    .OrderBy(e => e.Nome)
                    .ToList() ?? new List<Estado>();

                foreach (var estado in estados)
                {
                    if (estado.Cidades is null)
                    {
                        continue;
                    }

                    foreach (var cidade in estado.Cidades)
                    {
                        cidade.Estado = estado;
                    }
                }

                estadoPicker.ItemsSource = estados;
                estadosCarregados = true;
            }
            catch
            {
                estados = new List<Estado>();
                estadoPicker.ItemsSource = estados;
            }
        }

        void EstadoPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (estadoPicker.SelectedItem is not Estado estadoSelecionado)
            {
                cidadePicker.ItemsSource = null;
                cidadePicker.SelectedIndex = -1;
                return;
            }

            var cidades = estadoSelecionado.Cidades?
                .Where(c => c.Id > 0 && !string.IsNullOrWhiteSpace(c.Nome))
                .OrderBy(c => c.Nome)
                .ToList() ?? new List<Cidade>();

            cidadePicker.ItemsSource = cidades;
            cidadePicker.SelectedIndex = -1;
        }

        async void Salvar_Clicked(object sender, EventArgs e)
        {
            var nome = nomeEntry.Text?.Trim();
            var email = emailEntry.Text?.Trim();
            var cidadeSelecionada = cidadePicker.SelectedItem as Cidade;

            if (string.IsNullOrWhiteSpace(nome))
            {
                await DisplayAlert("Validação", "Nome é obrigatório.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                await DisplayAlert("Validação", "Email é obrigatório.", "OK");
                return;
            }

            var cliente = clientId > 0
                ? await ClienteRepository.GetByIdAsync(clientId) ?? new Cliente { Id = clientId }
                : new Cliente();

            cliente.Nome = nome;
            cliente.CNPJ = cnpjEntry.Text;
            cliente.CPF = cpfEntry.Text;
            cliente.RG = rgEntry.Text;
            cliente.Telefone = telefoneEntry.Text;
            cliente.Celular = celularEntry.Text;
            cliente.Email = email;
            cliente.Endereco = enderecoEntry.Text;
            cliente.Numero = numeroEntry.Text;
            cliente.Complemento = complementoEntry.Text;
            cliente.Bairro = bairroEntry.Text;
            cliente.Cidade = cidadeSelecionada;
            cliente.CEP = cepEntry.Text;

            await ClienteRepository.SaveAsync(cliente);

            if (clientId <= 0)
            {
                clientId = cliente.Id;
            }

            await Shell.Current.GoToAsync("..");
        }

        async void Cancelar_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }

        void PopulateForm(Cliente c)
        {
            nomeEntry.Text = c.Nome;
            cnpjEntry.Text = c.CNPJ;
            cpfEntry.Text = c.CPF;
            rgEntry.Text = c.RG;
            telefoneEntry.Text = c.Telefone;
            celularEntry.Text = c.Celular;
            emailEntry.Text = c.Email;
            enderecoEntry.Text = c.Endereco;
            numeroEntry.Text = c.Numero;
            complementoEntry.Text = c.Complemento;
            bairroEntry.Text = c.Bairro;
            cepEntry.Text = c.CEP;

            var estado = estados.FirstOrDefault(e =>
                c.Cidade?.Estado?.Id > 0 && e.Id == c.Cidade.Estado.Id ||
                (!string.IsNullOrWhiteSpace(c.Cidade?.Estado?.Nome) && string.Equals(e.Nome, c.Cidade.Estado.Nome, StringComparison.OrdinalIgnoreCase)) ||
                (c.Cidade?.Id > 0 && (e.Cidades?.Any(x => x.Id == c.Cidade.Id) ?? false)) ||
                (!string.IsNullOrWhiteSpace(c.Cidade?.Nome) && (e.Cidades?.Any(x => string.Equals(x.Nome, c.Cidade.Nome, StringComparison.OrdinalIgnoreCase)) ?? false)));

            if (estado == null)
            {
                estadoPicker.SelectedIndex = -1;
                cidadePicker.ItemsSource = null;
                cidadePicker.SelectedIndex = -1;
                return;
            }

            estadoPicker.SelectedItem = estado;

            var cidades = estado.Cidades?
                .OrderBy(x => x.Nome)
                .ToList() ?? new List<Cidade>();

            cidadePicker.ItemsSource = cidades;

            var cidade = cidades.FirstOrDefault(x =>
                (c.Cidade?.Id > 0 && x.Id == c.Cidade.Id) ||
                string.Equals(x.Nome, c.Cidade?.Nome, StringComparison.OrdinalIgnoreCase));

            cidadePicker.SelectedItem = cidade;
        }

        void ClearForm()
        {
            nomeEntry.Text = string.Empty;
            cnpjEntry.Text = string.Empty;
            cpfEntry.Text = string.Empty;
            rgEntry.Text = string.Empty;
            telefoneEntry.Text = string.Empty;
            celularEntry.Text = string.Empty;
            emailEntry.Text = string.Empty;
            enderecoEntry.Text = string.Empty;
            numeroEntry.Text = string.Empty;
            complementoEntry.Text = string.Empty;
            bairroEntry.Text = string.Empty;
            cepEntry.Text = string.Empty;
            estadoPicker.SelectedIndex = -1;
            cidadePicker.ItemsSource = null;
            cidadePicker.SelectedIndex = -1;
        }
    }
}
