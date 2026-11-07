using NUnit.Framework;

// Onde muros e armas podem ser colocados: grade de 0,25 e dentro da faixa do chão.
public class ColocacaoTests
{
    [Test]
    public void XDaColocacao_GrudaNaGrade()
    {
        Assert.AreEqual(1.25f, Regras.XDaColocacao(1.3f, 0.5f), 1e-4f);
        Assert.AreEqual(1.5f, Regras.XDaColocacao(1.4f, 0.5f), 1e-4f);
        Assert.AreEqual(-2f, Regras.XDaColocacao(-2.05f, 0.5f), 1e-4f);
    }

    [Test]
    public void XDaColocacao_NaoSaiDaFaixaPelaEsquerda()
    {
        float largura = 0.4f;
        Assert.AreEqual(Regras.ColocacaoXMin + largura / 2f, Regras.XDaColocacao(-50f, largura), 1e-4f);
    }

    [Test]
    public void XDaColocacao_NaoSaiDaFaixaPelaDireita()
    {
        float largura = 1.2f;
        Assert.AreEqual(Regras.ColocacaoXMax - largura / 2f, Regras.XDaColocacao(50f, largura), 1e-4f);
    }

    [Test]
    public void XDaColocacao_ItemLargoNuncaPassaDasBordas()
    {
        float largura = 2f;
        for (float x = -10f; x <= 10f; x += 0.37f)
        {
            float r = Regras.XDaColocacao(x, largura);
            Assert.GreaterOrEqual(r - largura / 2f, Regras.ColocacaoXMin - 1e-4f, $"mouse em {x}");
            Assert.LessOrEqual(r + largura / 2f, Regras.ColocacaoXMax + 1e-4f, $"mouse em {x}");
        }
    }
}
