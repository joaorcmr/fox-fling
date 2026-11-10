using System.Collections.Generic;
using UnityEngine;

// As moedas do jogador e o Cofrinho (o "income"). Guarda o RESTO
// fracionário: com ×1,25 um pardal de 1 moeda vale 1,25, e esses quartos de
// moeda se somam em vez de sumir no arredondamento.
public class Economia
{
    readonly DadosEconomia dados;
    float resto;

    public int Moedas { get; private set; }
    public int NivelCofrinho { get; private set; }
    public int ComprasDeReparo { get; private set; }

    public Economia(DadosEconomia dados)
    {
        this.dados = dados;
        Moedas = dados.moedasIniciais;
    }

    public float Multiplicador => NivelCofrinho == 0 ? 1f : dados.cofrinho[NivelCofrinho - 1].multiplicador;

    public NivelCofrinho ProximoCofrinho => NivelCofrinho < dados.cofrinho.Count ? dados.cofrinho[NivelCofrinho] : null;

    // Soma o valor ao resto e credita só a parte inteira. Devolve quantas moedas
    // entraram agora (é o "+N" que aparece na tela).
    public int Creditar(float valor)
    {
        resto += valor;
        int inteiro = Mathf.FloorToInt(resto + 1e-4f); // folga para erro de arredondamento do float
        resto = Mathf.Max(0f, resto - inteiro);
        Moedas += inteiro;
        return inteiro;
    }

    public bool Gastar(int custo)
    {
        if (custo > Moedas) return false;
        Moedas -= custo;
        return true;
    }

    public bool ComprarCofrinho()
    {
        var proximo = ProximoCofrinho;
        if (proximo == null || !Gastar(proximo.custo)) return false;
        NivelCofrinho++;
        return true;
    }

    // Cada reparo da Torre fica mais caro que o anterior (issue #14).
    public int CustoProximoReparo => dados.custoReparoBase + dados.custoReparoIncremento * ComprasDeReparo;

    public bool ComprarReparo()
    {
        if (!Gastar(CustoProximoReparo)) return false;
        ComprasDeReparo++;
        return true;
    }

    public int Bonus(int onda) => dados.bonusOndaBase + dados.bonusOndaPorOnda * onda;

    // 0,7f vezes 120 dá 83,99999 no float: a folga evita perder uma moeda na venda.
    public int ValorVenda(int gasto) => Mathf.FloorToInt(gasto * dados.fracaoVenda + 1e-3f);

    // Na troca de fase todos os porcos voltam para a carteira (por padrão, tudo o que foi gasto neles).
    public int ValorTrocaDeFase(int gasto) => Mathf.FloorToInt(gasto * dados.fracaoTrocaDeFase + 1e-3f);

    // Em quantas ondas o PRÓXIMO nível do Cofrinho se paga, contando a partir de
    // proximaOnda (1 = primeira). As moedas-base de cada onda são conhecidas de
    // antemão, então a conta é exata. null = não se paga mais até a última onda.
    public int? OndasParaPagar(IReadOnlyList<float> moedasBasePorOnda, int proximaOnda)
    {
        var proximo = ProximoCofrinho;
        if (proximo == null) return null;
        float ganho = proximo.multiplicador - Multiplicador;
        if (ganho <= 0f) return null;

        float soma = 0f;
        int ondas = 0;
        for (int onda = Mathf.Max(1, proximaOnda); onda <= moedasBasePorOnda.Count; onda++)
        {
            soma += moedasBasePorOnda[onda - 1] * ganho;
            ondas++;
            if (soma >= proximo.custo - 1e-3f) return ondas;
        }
        return null;
    }
}
