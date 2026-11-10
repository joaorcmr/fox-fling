using System;
using System.Collections.Generic;

// As ondas na simulação: soltar a próxima, ir soltando os pássaros na hora
// certa, o fim de onda com o bônus de moedas e o Auto. Também as consultas que
// a interface mostra antes de cada onda (resumo e estreias).
public partial class Simulacao
{
    public int Onda { get; private set; }     // em andamento, ou a última concluída (0 antes da primeira)
    public int TotalOndas => Dados.ondas.Count;

    readonly List<float> moedasBasePorOnda = new List<float>();
    public IReadOnlyList<float> MoedasBasePorOnda => moedasBasePorOnda;

    readonly GeradorOnda gerador = new GeradorOnda();
    readonly List<GrupoOnda> nascidos = new List<GrupoOnda>();
    float esperaAuto;

    public event Action<int> OndaComecou;
    public event Action<int, int> OndaTerminou;           // onda, bônus

    public bool SoltarOnda()
    {
        if (Estado != EstadoJogo.Preparando && Estado != EstadoJogo.Intervalo) return false;
        if (Onda >= TotalOndas) return false;
        Onda++;
        Estado = EstadoJogo.EmOnda;
        gerador.Iniciar(Dados.ondas[Onda - 1]);
        OndaComecou?.Invoke(Onda);
        return true;
    }

    // ------------------------------------------------------------------
    // Consultas para a interface
    // ------------------------------------------------------------------
    public List<KeyValuePair<EspeciePassaro, int>> ResumoOnda(int numero)
    {
        var resumo = new List<KeyValuePair<EspeciePassaro, int>>();
        if (numero < 1 || numero > TotalOndas) return resumo;
        foreach (var g in Dados.ondas[numero - 1].grupos)
        {
            int i = resumo.FindIndex(k => k.Key == g.especie);
            if (i < 0) resumo.Add(new KeyValuePair<EspeciePassaro, int>(g.especie, g.quantidade));
            else resumo[i] = new KeyValuePair<EspeciePassaro, int>(g.especie, resumo[i].Value + g.quantidade);
        }
        return resumo;
    }

    // Pássaros que aparecem pela primeira vez nesta onda ("Novo: Urubu").
    public List<EspeciePassaro> Estreias(int numero)
    {
        var vistos = new HashSet<EspeciePassaro>();
        for (int onda = 1; onda < numero && onda <= TotalOndas; onda++)
            foreach (var g in Dados.ondas[onda - 1].grupos) vistos.Add(g.especie);
        var novos = new List<EspeciePassaro>();
        foreach (var kv in ResumoOnda(numero))
            if (!vistos.Contains(kv.Key)) novos.Add(kv.Key);
        return novos;
    }

    // ------------------------------------------------------------------
    // Partes do passo
    // ------------------------------------------------------------------
    void NascerDaOnda()
    {
        nascidos.Clear();
        gerador.Atualizar(Dt, nascidos);
        foreach (var g in nascidos) Nascer(g.especie, g.multiplicadorVida, 0f);
    }

    void EsperarAuto()
    {
        esperaAuto -= Dt;
        if (esperaAuto <= 0f) SoltarOnda();
    }

    void TerminarOnda()
    {
        int bonus = Economia.Bonus(Onda);
        Economia.Creditar(bonus);
        OndaTerminou?.Invoke(Onda, bonus);
        if (Onda >= TotalOndas)
        {
            Estado = EstadoJogo.Vitoria;
            FimDeJogo?.Invoke(true);
        }
        else
        {
            // Se a PRÓXIMA onda é a primeira de uma fase nova, troca de mapa já
            // aqui: o jogador vê o mapa novo e tem o Intervalo inteiro (ou a
            // espera do Auto) para se preparar, em vez de levar a onda em cima
            // sem ter colocado porco nenhum. Com o Auto ligado, essa espera é
            // mais longa que o normal (mudou o mapa: os porcos de antes
            // voltaram como moedas, dá tempo de recolocar antes de a onda chegar).
            int faseAntes = Fase;
            AtualizarFase(Onda + 1);
            Estado = EstadoJogo.Intervalo;
            esperaAuto = Fase != faseAntes ? Dados.economia.esperaAutoNovaFase : Dados.economia.esperaAuto;
        }
    }

    // ------------------------------------------------------------------
    // Moedas-base de cada onda (sem o Cofrinho), para o cartão do Cofrinho calcular
    // em quantas ondas o investimento se paga
    // ------------------------------------------------------------------
    void CalcularMoedasDasOndas()
    {
        foreach (var onda in Dados.ondas) moedasBasePorOnda.Add(MoedasDaOnda(onda));
    }

    float MoedasDaOnda(DadosOnda onda)
    {
        float total = 0f;
        foreach (var g in onda.grupos) total += g.quantidade * MoedasDe(g.especie, 0);
        return total;
    }

    // Moedas de um pássaro mais as dos filhos que ele solta (a pata e os patinhos).
    float MoedasDe(EspeciePassaro especie, int profundidade)
    {
        if (!Dados.passaros.TryGetValue(especie, out var dados)) return 0f;
        float total = dados.moedas;
        if (dados.quantidadeFilhos > 0 && profundidade < 3)
            total += dados.quantidadeFilhos * MoedasDe(dados.filho, profundidade + 1);
        return total;
    }
}
