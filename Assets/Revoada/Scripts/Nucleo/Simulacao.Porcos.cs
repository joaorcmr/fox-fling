using System;
using System.Collections.Generic;
using UnityEngine;

// Os comandos de porco: comprar, vender e mudar o modo de alvo. São imediatos
// (não esperam um Passo), por isso funcionam com o jogo pausado.
public partial class Simulacao
{
    readonly List<Porco> porcos = new List<Porco>();
    public IReadOnlyList<Porco> Porcos => porcos;

    public event Action<Porco> PorcoCriado;
    public event Action<Porco> PorcoVendido;

    int proximoIdPorco = 1;

    public bool PodeColocar(EspeciePorco especie, Vector2 posicao)
    {
        if (Acabou || !Dados.porcos.TryGetValue(especie, out var dados)) return false;
        return Colocacao.Valida(posicao, dados.raio, Dados.mapa, Trilha, porcos);
    }

    public Resultado Comprar(EspeciePorco especie, Vector2 posicao, out Porco porco)
    {
        porco = null;
        if (Acabou || !Dados.porcos.TryGetValue(especie, out var dados)) return Resultado.Indisponivel;
        if (!Colocacao.Valida(posicao, dados.raio, Dados.mapa, Trilha, porcos)) return Resultado.LugarInvalido;
        if (!Economia.Gastar(dados.custo)) return Resultado.SemPenas;

        porco = new Porco(proximoIdPorco++, dados, posicao) { Gasto = dados.custo };
        porcos.Add(porco);
        PorcoCriado?.Invoke(porco);
        return Resultado.Ok;
    }

    public int ValorVenda(Porco p) => p == null ? 0 : Economia.ValorVenda(p.Gasto);

    // Devolve as penas que voltaram (0 se o porco já não está no jogo).
    public int Vender(Porco p)
    {
        if (Acabou || p == null || !porcos.Remove(p)) return 0;
        int valor = Economia.ValorVenda(p.Gasto);
        Economia.Creditar(valor);
        PorcoVendido?.Invoke(p);
        return valor;
    }

    public void MudarModo(Porco p, ModoAlvo modo)
    {
        if (Acabou || p == null) return;
        p.Modo = modo;
    }
}
