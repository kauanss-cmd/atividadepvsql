using System;
using System.Collections.Generic;
using System.Linq;

namespace SisVenda.Models
{
    public class Pedido
    {
        public Pedido(int id, DateTime data, Cliente cliente)
        {
            Id = id;
            Data = data;
            Cliente = cliente;
        }

        public int Id { get; set; }
        public DateTime Data { get; set; }
        public Cliente Cliente { get; set; } = null!;
        public List<ItemPedido> Itens { get; set; } = [];
        public decimal ValorTotal => Itens.Sum(item => item.ValorTotal);
        public string Observacao { get; set; } = string.Empty;

        public override string ToString() => $"{Id} - {Data:dd/MM/yyyy} - {Cliente.Nome} - {ValorTotal:C} - {Observacao}";
    }

    public class ItemPedido
    {
        public Produto Produto { get; set; } = null!;
        public int Qtd { get; set; }
        public decimal Valor { get; set; }

        public ItemPedido(Produto produto, int qtd, decimal valor)
        {
            Produto = produto;
            Qtd = qtd;
            Valor = valor;
        }

        public decimal ValorTotal => Qtd * Valor;

        public override string ToString() =>
            $"{Produto.Nome} | Qtd: {Qtd} | Unitário: {Valor:C} | Total: {ValorTotal:C}";
    }
}
