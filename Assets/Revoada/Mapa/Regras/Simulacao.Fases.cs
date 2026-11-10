using System;
using System.Collections.Generic;
using UnityEngine;

// O jogo pode se dividir em FASES: grupos iguais de ondas, cada um com o seu
// próprio mapa (outro caminho até a Torre, outro turno do dia). Só entra em
// jogo quando o DadosJogo chega com 2 ou mais fases (Dados.fases); sem isso
// (como em todo cenário de teste), Fase fica sempre 1 e nada aqui faz efeito —
// o jogo roda do jeito que sempre rodou, num mapa só.
public partial class Simulacao
{
    public int TotalFases => Dados.fases.Count;
    public int Fase { get; private set; } = 1;

    // Quantas ondas cada fase tem (as 30 ondas viram 3 fases de 10).
    int OndasPorFase => TotalFases > 1 ? Mathf.Max(1, TotalOndas / TotalFases) : TotalOndas;

    public event Action<int> FaseComecou; // número da fase (1, 2, 3...)

    // Chamado ao soltar cada onda: troca de fase se a onda que vai começar é a
    // primeira de uma fase nova. O mapa muda; os porcos que ficaram em cima da
    // trilha nova, perto demais da Torre nova ou de um obstáculo novo somem do
    // mapa (voltam 70% do que foi gasto, a mesma taxa de vender) — os outros
    // continuam exatamente como estavam, nível e tudo.
    void AtualizarFase(int onda)
    {
        if (TotalFases <= 1) return;
        int fase = Mathf.Min(TotalFases, (onda - 1) / OndasPorFase + 1);
        if (fase == Fase) return;
        Fase = fase;

        var novoMapa = Dados.fases[fase - 1];
        Dados.mapa = novoMapa;
        Trilha = new Trilha(novoMapa.pontos, novoMapa.raioCanto);
        AjustarPorcosParaNovoMapa();
        FaseComecou?.Invoke(fase);
    }

    // Mantém quem ainda cabe no mapa novo (testado um a um, na ordem em que
    // foram colocados: dois porcos que só colidiam entre si continuam os dois,
    // porque nenhum "ficou" antes do outro de verdade); remove e credita 70%
    // do gasto de quem não cabe mais.
    void AjustarPorcosParaNovoMapa()
    {
        if (porcos.Count == 0) return;
        var antigos = new List<Porco>(porcos);
        var mantidos = new List<Porco>();
        var removidos = new List<Porco>();
        foreach (var p in antigos)
        {
            if (Colocacao.Valida(p.Posicao, p.Dados.raio, Dados.mapa, Trilha, mantidos)) mantidos.Add(p);
            else removidos.Add(p);
        }
        if (removidos.Count == 0) return; // nada mudou: nem credita, nem dispara evento à toa

        porcos.Clear();
        porcos.AddRange(mantidos);
        foreach (var p in removidos)
        {
            Economia.Creditar(Economia.ValorVenda(p.Gasto));
            PorcoVendido?.Invoke(p);
        }
        RecalcularAuras();
    }
}
