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

    // Na troca de fase, TODOS os porcos saem do mapa e o que foi gasto neles
    // volta para a carteira: os porcos recolhidos e o total de moedas devolvidas.
    // Dispara logo antes do FaseComecou (só quando havia porco no mapa).
    public event Action<IReadOnlyList<Porco>, int> PorcosRecolhidos;

    // Em que fase cai a onda (1 = a primeira onda do jogo).
    public int FaseDaOnda(int onda) => TotalFases <= 1 ? 1 : Mathf.Clamp((onda - 1) / OndasPorFase + 1, 1, TotalFases);

    // A onda fecha a fase dela? (Ao terminar essa onda, o mapa troca.)
    public bool UltimaOndaDaFase(int onda) => onda >= 1 && onda < TotalOndas && FaseDaOnda(onda + 1) != FaseDaOnda(onda);

    // Quanto este porco devolve na troca de fase (a taxa fica na Economia).
    public int ValorTrocaDeFase(Porco p) => p == null ? 0 : Economia.ValorTrocaDeFase(p.Gasto);

    // Chamado ao terminar cada onda (ver TerminarOnda), com a PRÓXIMA onda:
    // troca de fase se ela é a primeira de uma fase nova. O mapa muda e todos
    // os porcos são recolhidos (ver RecolherPorcos).
    void AtualizarFase(int onda)
    {
        if (TotalFases <= 1) return;
        int fase = FaseDaOnda(onda);
        if (fase == Fase) return;
        Fase = fase;

        var novoMapa = Dados.fases[fase - 1];
        Dados.mapa = novoMapa;
        Trilha = new Trilha(novoMapa.pontos, novoMapa.raioCanto);
        RecolherPorcos();
        FaseComecou?.Invoke(fase);
    }

    // Os lugares do mapa velho não servem no novo (o caminho é outro), então
    // todos os porcos saem do mapa e o que foi gasto em cada um — compra,
    // melhorias e especialização — volta para a carteira, na fração
    // fracaoTrocaDeFase da economia (1 = tudo). O jogador monta a defesa de
    // novo no mapa novo sem perder o que já tinha investido.
    void RecolherPorcos()
    {
        if (porcos.Count == 0) return; // nada para recolher: nem credita, nem dispara evento à toa
        var recolhidos = new List<Porco>(porcos);
        porcos.Clear();
        int total = 0;
        foreach (var p in recolhidos) total += Economia.Creditar(ValorTrocaDeFase(p));
        PorcosRecolhidos?.Invoke(recolhidos, total);
    }
}
