using NUnit.Framework;
using UnityEngine;

// Testes das regras "puras" do jogo: contas que não dependem de cena nem de física.
public class RegrasTests
{
    static void AssertV(Vector2 esperado, Vector2 obtido)
    {
        Assert.AreEqual(esperado.x, obtido.x, 1e-4f, "x");
        Assert.AreEqual(esperado.y, obtido.y, 1e-4f, "y");
    }

    [Test]
    public void Estrelas_DerrotaNaoDaEstrela()
    {
        Assert.AreEqual(0, Regras.Estrelas(0, 100, false));
    }

    [Test]
    public void Estrelas_SobemComAPorcentagemDaPontuacaoMaxima()
    {
        Assert.AreEqual(1, Regras.Estrelas(10, 100, true));
        Assert.AreEqual(2, Regras.Estrelas(45, 100, true));
        Assert.AreEqual(2, Regras.Estrelas(74, 100, true));
        Assert.AreEqual(3, Regras.Estrelas(75, 100, true));
    }

    [Test]
    public void Estrelas_PontuacaoMaximaZeroNaoDivide()
    {
        Assert.AreEqual(3, Regras.Estrelas(5, 0, true));
    }

    [Test]
    public void PontuacaoMaxima_SomaBonusDasRaposasQueSobram()
    {
        // A primeira raposa sempre é usada, então o bônus máximo é (n - 1) raposas.
        Assert.AreEqual(24000, Regras.PontuacaoMaxima(14000, 2));
    }

    [Test]
    public void DanoDoImpulso_IgnoraImpactosFracos()
    {
        Assert.AreEqual(0f, Regras.DanoDoImpulso(0.5f, 1f, 10f));
    }

    [Test]
    public void DanoDoImpulso_ProporcionalAoImpulso()
    {
        Assert.AreEqual(30f, Regras.DanoDoImpulso(3f, 1f, 10f), 1e-4f);
    }

    [Test]
    public void VelocidadeDeLancamento_ApontaParaOLadoOpostoDaPuxada()
    {
        AssertV(new Vector2(8, 0), Regras.VelocidadeDeLancamento(Vector2.zero, new Vector2(-1, 0), 1.5f, 8f));
    }

    [Test]
    public void VelocidadeDeLancamento_RespeitaAPuxadaMaxima()
    {
        AssertV(new Vector2(12, 0), Regras.VelocidadeDeLancamento(Vector2.zero, new Vector2(-3, 0), 1.5f, 8f));
    }

    [Test]
    public void LimitarPuxada_PrendeNoRaioMaximo()
    {
        AssertV(new Vector2(-1.5f, 0), Regras.LimitarPuxada(Vector2.zero, new Vector2(-3, 0), 1.5f));
        AssertV(new Vector2(-0.5f, 0.2f), Regras.LimitarPuxada(Vector2.zero, new Vector2(-0.5f, 0.2f), 1.5f));
    }

    [Test]
    public void PuxadaValida_CliqueSemPuxarNaoLanca()
    {
        Assert.IsFalse(Regras.PuxadaValida(Vector2.zero, new Vector2(-0.1f, 0)));
        Assert.IsTrue(Regras.PuxadaValida(Vector2.zero, new Vector2(-0.5f, 0)));
    }

    [Test]
    public void PosicaoNaTrajetoria_SegueAEquacaoDoMovimento()
    {
        // p = p0 + v·t + ½·g·t²
        AssertV(new Vector2(10, -5), Regras.PosicaoNaTrajetoria(Vector2.zero, new Vector2(10, 0), new Vector2(0, -10), 1f));
    }

    [Test]
    public void FatorExplosao_CaiLinearmenteAteOZero()
    {
        Assert.AreEqual(1f, Regras.FatorExplosao(0, 2), 1e-4f);
        Assert.AreEqual(0.5f, Regras.FatorExplosao(1, 2), 1e-4f);
        Assert.AreEqual(0f, Regras.FatorExplosao(3, 2), 1e-4f);
    }

    [Test]
    public void ForaDoMundo_LadosEChao()
    {
        Assert.IsTrue(Regras.ForaDoMundo(new Vector2(9.5f, 0)));
        Assert.IsTrue(Regras.ForaDoMundo(new Vector2(-9.5f, 0)));
        Assert.IsTrue(Regras.ForaDoMundo(new Vector2(0, -7.5f)));
        Assert.IsFalse(Regras.ForaDoMundo(new Vector2(3, -4)));
    }

    [Test]
    public void ForaDoMundo_VoarAltoNaoESairDoMundo()
    {
        Assert.IsFalse(Regras.ForaDoMundo(new Vector2(0, 20)));
    }

    [Test]
    public void EstadoAposOnda_PerderOUltimoTubaraoEDerrotaMesmoComRaposasPorVir()
    {
        Assert.AreEqual(EstadoFase.Derrota, Regras.EstadoAposOnda(0, 3, 2, false));
        Assert.AreEqual(EstadoFase.Derrota, Regras.EstadoAposOnda(0, 0, 0, true));
    }

    [Test]
    public void EstadoAposOnda_ContinuaEnquantoHaRaposasPorNascerOuEmCampo()
    {
        Assert.AreEqual(EstadoFase.Jogando, Regras.EstadoAposOnda(2, 1, 0, true));
        Assert.AreEqual(EstadoFase.Jogando, Regras.EstadoAposOnda(2, 0, 1, true));
    }

    [Test]
    public void EstadoAposOnda_SoVenceDepoisQueOMundoPara()
    {
        Assert.AreEqual(EstadoFase.Jogando, Regras.EstadoAposOnda(1, 0, 0, false));
        Assert.AreEqual(EstadoFase.Vitoria, Regras.EstadoAposOnda(1, 0, 0, true));
    }

    [Test]
    public void EstrelasPorSobreviventes_TodosVivosValeTres()
    {
        Assert.AreEqual(3, Regras.EstrelasPorSobreviventes(4, 4, true));
    }

    [Test]
    public void EstrelasPorSobreviventes_MetadeOuMaisValeDoisEMenosValeUma()
    {
        Assert.AreEqual(2, Regras.EstrelasPorSobreviventes(3, 4, true));
        Assert.AreEqual(2, Regras.EstrelasPorSobreviventes(2, 4, true));
        Assert.AreEqual(1, Regras.EstrelasPorSobreviventes(1, 4, true));
    }

    [Test]
    public void EstrelasPorSobreviventes_DerrotaOuNinguemVivoNaoDaEstrela()
    {
        Assert.AreEqual(0, Regras.EstrelasPorSobreviventes(2, 4, false));
        Assert.AreEqual(0, Regras.EstrelasPorSobreviventes(0, 4, true));
    }

    [Test]
    public void FormatarPontos_UsaPontoComoSeparadorDeMilhar()
    {
        Assert.AreEqual("0", Regras.FormatarPontos(0));
        Assert.AreEqual("500", Regras.FormatarPontos(500));
        Assert.AreEqual("12.500", Regras.FormatarPontos(12500));
        Assert.AreEqual("1.234.567", Regras.FormatarPontos(1234567));
    }
}
