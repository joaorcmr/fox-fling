using System.Collections.Generic;
using NUnit.Framework;

// As penas são a moeda do jogo. A decisão central (mais porcos agora ou mais
// penas depois) depende destas contas estarem certas até a última pena.
public class EconomiaTests
{
    static DadosEconomia Dados(int penas = 300)
    {
        return new DadosEconomia
        {
            penasIniciais = penas,
            ninho = new List<NivelNinho>
            {
                new NivelNinho { multiplicador = 1.25f, custo = 250 },
                new NivelNinho { multiplicador = 1.5f, custo = 500 },
            },
        };
    }

    [Test]
    public void Creditar_AcumulaFracoes()
    {
        // Um pardal de 1 pena com ×1,25: a fração não some, ela se acumula.
        var e = new Economia(Dados(0));

        Assert.AreEqual(1, e.Creditar(1.25f));
        Assert.AreEqual(1, e.Creditar(1.25f));
        Assert.AreEqual(1, e.Creditar(1.25f));
        Assert.AreEqual(2, e.Creditar(1.25f));
        Assert.AreEqual(5, e.Penas);
    }

    [Test]
    public void Gastar_SemPenas_NaoMuda()
    {
        var e = new Economia(Dados(300));

        Assert.IsFalse(e.Gastar(301));
        Assert.AreEqual(300, e.Penas);
        Assert.IsTrue(e.Gastar(120));
        Assert.AreEqual(180, e.Penas);
    }

    [Test]
    public void Ninho_SobeMultiplicador()
    {
        var e = new Economia(Dados(300));
        Assert.AreEqual(1f, e.Multiplicador);

        Assert.IsTrue(e.ComprarNinho());
        Assert.AreEqual(1.25f, e.Multiplicador, 1e-5f);
        Assert.AreEqual(50, e.Penas);
        Assert.AreEqual(1, e.NivelNinho);

        Assert.IsFalse(e.ComprarNinho()); // faltam penas para o nível 2
        Assert.AreEqual(50, e.Penas);

        e.Creditar(500f);
        Assert.IsTrue(e.ComprarNinho());
        Assert.AreEqual(1.5f, e.Multiplicador, 1e-5f);
        Assert.IsNull(e.ProximoNinho);
        Assert.IsFalse(e.ComprarNinho()); // já está no máximo
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

    [Test]
    public void OndasParaPagar()
    {
        // O próximo nível dá +0,25 por pena-base e custa 50: com 100 penas-base
        // por onda, rende 25 por onda e se paga em 2. Começando na onda 4, não
        // dá mais tempo.
        var e = new Economia(new DadosEconomia
        {
            penasIniciais = 0,
            ninho = new List<NivelNinho> { new NivelNinho { multiplicador = 1.25f, custo = 50 } },
        });
        var penasBase = new List<float> { 100f, 100f, 100f, 100f };

        Assert.AreEqual(2, e.OndasParaPagar(penasBase, 1));
        Assert.IsNull(e.OndasParaPagar(penasBase, 4));
    }
}
