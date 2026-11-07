using System.Linq;
using NUnit.Framework;

// A composição das levas de raposas por nível (issue #5): mais raposas a cada nível
// e uma mistura de tipos que muda conforme a dificuldade sobe.
public class LevasTests
{
    [Test]
    public void NivelDaLeva_ContaAsLevasDeTodasAsFases()
    {
        Assert.AreEqual(1, Regras.NivelDaLeva(1, 1));
        Assert.AreEqual(3, Regras.NivelDaLeva(1, 3));
        Assert.AreEqual(4, Regras.NivelDaLeva(2, 1));
        Assert.AreEqual(15, Regras.NivelDaLeva(5, 3));
    }

    [Test]
    public void ComporLeva_TemOTamanhoDoNivel()
    {
        for (int fase = 1; fase <= 5; fase++)
            for (int leva = 1; leva <= Regras.LevasPorFase; leva++)
                Assert.AreEqual(Regras.QuantidadeDeRaposas(Regras.NivelDaLeva(fase, leva)), Regras.ComporLeva(fase, leva).Length);
    }

    [Test]
    public void ComporLeva_CadaNivelTemMaisRaposasQueOAnterior()
    {
        int anterior = 0;
        for (int fase = 1; fase <= 5; fase++)
            for (int leva = 1; leva <= Regras.LevasPorFase; leva++)
            {
                int agora = Regras.ComporLeva(fase, leva).Length;
                Assert.Greater(agora, anterior, $"fase {fase} leva {leva}");
                anterior = agora;
            }
    }

    [Test]
    public void ComporLeva_PrimeiraLevaSoTemRaposasNormais()
    {
        CollectionAssert.AreEqual(new[] { TipoRaposa.Normal, TipoRaposa.Normal }, Regras.ComporLeva(1, 1));
    }

    [Test]
    public void ComporLeva_TiposEspeciaisAparecemDepoisDeCertoNivel()
    {
        Assert.IsFalse(Regras.ComporLeva(1, 1).Contains(TipoRaposa.Ninja));
        Assert.IsTrue(Regras.ComporLeva(1, 2).Contains(TipoRaposa.Ninja));      // nível 2

        Assert.IsFalse(Regras.ComporLeva(1, 3).Contains(TipoRaposa.Bomba));     // nível 3
        Assert.IsTrue(Regras.ComporLeva(2, 1).Contains(TipoRaposa.Bomba));      // nível 4

        Assert.IsFalse(Regras.ComporLeva(2, 2).Contains(TipoRaposa.Grande));    // nível 5
        Assert.IsTrue(Regras.ComporLeva(2, 3).Contains(TipoRaposa.Grande));     // nível 6
    }

    [Test]
    public void ComporLeva_SempreTemPeloMenosUmaNormal()
    {
        for (int fase = 1; fase <= 5; fase++)
            for (int leva = 1; leva <= Regras.LevasPorFase; leva++)
                Assert.IsTrue(Regras.ComporLeva(fase, leva).Contains(TipoRaposa.Normal), $"fase {fase} leva {leva}");
    }

    [Test]
    public void ComporLeva_AProporcaoDeNormaisCaiComODificuldade()
    {
        float Fracao(int fase, int leva)
        {
            var l = Regras.ComporLeva(fase, leva);
            return l.Count(t => t == TipoRaposa.Normal) / (float)l.Length;
        }

        Assert.Greater(Fracao(1, 1), Fracao(2, 1));
        Assert.Greater(Fracao(2, 1), Fracao(5, 3));
        Assert.Less(Fracao(5, 3), 0.5f);
    }

    [Test]
    public void ComporLeva_ChegaEmOrdem_NormaisPrimeiroEBombasPorUltimo()
    {
        var leva = Regras.ComporLeva(5, 3);
        Assert.AreEqual(TipoRaposa.Normal, leva.First());
        Assert.AreEqual(TipoRaposa.Bomba, leva.Last());

        int ultimaNormal = System.Array.FindLastIndex(leva, t => t == TipoRaposa.Normal);
        int primeiraBomba = System.Array.FindIndex(leva, t => t == TipoRaposa.Bomba);
        Assert.Less(ultimaNormal, primeiraBomba);
    }

    [Test]
    public void IntervaloEntreRaposas_DiminuiMasTemMinimo()
    {
        Assert.Greater(Regras.IntervaloEntreRaposas(1), Regras.IntervaloEntreRaposas(10));
        Assert.GreaterOrEqual(Regras.IntervaloEntreRaposas(15), 0.7f);
        Assert.GreaterOrEqual(Regras.IntervaloEntreRaposas(100), 0.7f);
    }
}
