using System.Collections.Generic;
using NUnit.Framework;

// As moedas são a moeda do jogo. A decisão central (mais porcos agora ou mais
// moedas depois) depende destas contas estarem certas até a última moeda.
public class EconomiaTests
{
    static DadosEconomia Dados(int moedas = 300)
    {
        return new DadosEconomia
        {
            moedasIniciais = moedas,
            cofrinho = new List<NivelCofrinho>
            {
                new NivelCofrinho { multiplicador = 1.25f, custo = 250 },
                new NivelCofrinho { multiplicador = 1.5f, custo = 500 },
            },
        };
    }

    [Test]
    public void Creditar_AcumulaFracoes()
    {
        // Um pardal de 1 moeda com ×1,25: a fração não some, ela se acumula.
        var e = new Economia(Dados(0));

        Assert.AreEqual(1, e.Creditar(1.25f));
        Assert.AreEqual(1, e.Creditar(1.25f));
        Assert.AreEqual(1, e.Creditar(1.25f));
        Assert.AreEqual(2, e.Creditar(1.25f));
        Assert.AreEqual(5, e.Moedas);
    }

    [Test]
    public void Gastar_SemMoedas_NaoMuda()
    {
        var e = new Economia(Dados(300));

        Assert.IsFalse(e.Gastar(301));
        Assert.AreEqual(300, e.Moedas);
        Assert.IsTrue(e.Gastar(120));
        Assert.AreEqual(180, e.Moedas);
    }

    [Test]
    public void Cofrinho_SobeMultiplicador()
    {
        var e = new Economia(Dados(300));
        Assert.AreEqual(1f, e.Multiplicador);

        Assert.IsTrue(e.ComprarCofrinho());
        Assert.AreEqual(1.25f, e.Multiplicador, 1e-5f);
        Assert.AreEqual(50, e.Moedas);
        Assert.AreEqual(1, e.NivelCofrinho);

        Assert.IsFalse(e.ComprarCofrinho()); // faltam moedas para o nível 2
        Assert.AreEqual(50, e.Moedas);

        e.Creditar(500f);
        Assert.IsTrue(e.ComprarCofrinho());
        Assert.AreEqual(1.5f, e.Multiplicador, 1e-5f);
        Assert.IsNull(e.ProximoCofrinho);
        Assert.IsFalse(e.ComprarCofrinho()); // já está no máximo
    }

    [Test]
    public void Bonus_DependeDaOnda()
    {
        var e = new Economia(Dados());

        Assert.AreEqual(30, e.Bonus(1));
        Assert.AreEqual(75, e.Bonus(10));
    }

    [Test]
    public void ValorVenda_ArredondaParaBaixo()
    {
        var e = new Economia(Dados());

        Assert.AreEqual(84, e.ValorVenda(120));
        Assert.AreEqual(157, e.ValorVenda(225));
    }

    // Na troca de fase os porcos voltam pelo valor inteiro (fracaoTrocaDeFase = 1).
    [Test]
    public void ValorTrocaDeFase_DevolveTudoPorPadrao_EArredondaParaBaixo()
    {
        Assert.AreEqual(220, new Economia(Dados()).ValorTrocaDeFase(220));

        var d = Dados();
        d.fracaoTrocaDeFase = 0.7f;
        Assert.AreEqual(84, new Economia(d).ValorTrocaDeFase(120), "0,7 × 120 dá 83,99999 no float: a folga não perde a moeda");
        d.fracaoTrocaDeFase = 0.5f;
        Assert.AreEqual(112, new Economia(d).ValorTrocaDeFase(225));
    }

    [Test]
    public void OndasParaPagar()
    {
        // O próximo nível dá +0,25 por moeda-base e custa 50: com 100 moedas-base
        // por onda, rende 25 por onda e se paga em 2. Começando na onda 4, não
        // dá mais tempo.
        var e = new Economia(new DadosEconomia
        {
            moedasIniciais = 0,
            cofrinho = new List<NivelCofrinho> { new NivelCofrinho { multiplicador = 1.25f, custo = 50 } },
        });
        var moedasBase = new List<float> { 100f, 100f, 100f, 100f };

        Assert.AreEqual(2, e.OndasParaPagar(moedasBase, 1));
        Assert.IsNull(e.OndasParaPagar(moedasBase, 4));
    }

    [Test]
    public void ComprarReparo_CadaCompraFicaMaisCara()
    {
        var e = new Economia(new DadosEconomia { moedasIniciais = 1000, custoReparoBase = 80, custoReparoIncremento = 40 });

        Assert.AreEqual(80, e.CustoProximoReparo);
        Assert.IsTrue(e.ComprarReparo());
        Assert.AreEqual(920, e.Moedas);

        Assert.AreEqual(120, e.CustoProximoReparo, "o segundo reparo custa 40 a mais que o primeiro");
        Assert.IsTrue(e.ComprarReparo());
        Assert.AreEqual(800, e.Moedas);

        Assert.AreEqual(160, e.CustoProximoReparo);
    }

    [Test]
    public void ComprarReparo_SemMoedas_NaoCobraNemContaComoCompra()
    {
        var e = new Economia(new DadosEconomia { moedasIniciais = 50, custoReparoBase = 80 });

        Assert.IsFalse(e.ComprarReparo());
        Assert.AreEqual(50, e.Moedas);
        Assert.AreEqual(0, e.ComprasDeReparo);
        Assert.AreEqual(80, e.CustoProximoReparo, "não contou como compra: o custo não subiu");
    }
}
