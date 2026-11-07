using System.Collections.Generic;
using UnityEngine;

// As penas do jogador e o Ninho de Penas (o "income"). Guarda o RESTO
// fracionário: com ×1,25 um pardal de 1 pena vale 1,25, e esses quartos de
// pena se somam em vez de sumir no arredondamento.
public class Economia
{
    readonly DadosEconomia dados;
    float resto;

    public int Penas { get; private set; }
    public int NivelNinho { get; private set; }

    public Economia(DadosEconomia dados)
    {
        this.dados = dados;
        Penas = dados.penasIniciais;
    }

    public float Multiplicador => NivelNinho == 0 ? 1f : dados.ninho[NivelNinho - 1].multiplicador;

    public NivelNinho ProximoNinho => NivelNinho < dados.ninho.Count ? dados.ninho[NivelNinho] : null;

    // Soma o valor ao resto e credita só a parte inteira. Devolve quantas penas
    // entraram agora (é o "+N" que aparece na tela).
    public int Creditar(float valor)
    {
        resto += valor;
        int inteiro = Mathf.FloorToInt(resto + 1e-4f); // folga para erro de arredondamento do float
        resto = Mathf.Max(0f, resto - inteiro);
        Penas += inteiro;
        return inteiro;
    }

    public bool Gastar(int custo)
    {
        if (custo > Penas) return false;
        Penas -= custo;
        return true;
    }

    public bool ComprarNinho()
    {
        var proximo = ProximoNinho;
        if (proximo == null || !Gastar(proximo.custo)) return false;
        NivelNinho++;
        return true;
    }

    public int Bonus(int onda) => dados.bonusOndaBase + dados.bonusOndaPorOnda * onda;

    // 0,7f vezes 120 dá 83,99999 no float: a folga evita perder uma pena na venda.
    public int ValorVenda(int gasto) => Mathf.FloorToInt(gasto * dados.fracaoVenda + 1e-3f);

    // Em quantas ondas o PRÓXIMO nível do Ninho se paga, contando a partir de
    // proximaOnda (1 = primeira). As penas-base de cada onda são conhecidas de
    // antemão, então a conta é exata. null = não se paga mais até a última onda.
    public int? OndasParaPagar(IReadOnlyList<float> penasBasePorOnda, int proximaOnda)
    {
        var proximo = ProximoNinho;
        if (proximo == null) return null;
        float ganho = proximo.multiplicador - Multiplicador;
        if (ganho <= 0f) return null;

        float soma = 0f;
        int ondas = 0;
        for (int onda = Mathf.Max(1, proximaOnda); onda <= penasBasePorOnda.Count; onda++)
        {
            soma += penasBasePorOnda[onda - 1] * ganho;
            ondas++;
            if (soma >= proximo.custo - 1e-3f) return ondas;
        }
        return null;
    }
}
