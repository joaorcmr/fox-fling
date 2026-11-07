using NUnit.Framework;

// Os pontos de cada leva (issue #4): base da leva + raposas derrotadas − penalidade por
// tubarão perdido, sem nunca ficar negativo.
public class PontosTests
{
    [Test]
    public void BaseDaLeva_SobeComONivel()
    {
        Assert.Greater(Regras.BaseDaLeva(5), Regras.BaseDaLeva(1));
        Assert.Greater(Regras.BaseDaLeva(15), Regras.BaseDaLeva(5));
    }

    [Test]
    public void PontosDaLeva_SemPerdasSomaABaseEAsRaposas()
    {
        Assert.AreEqual(Regras.BaseDaLeva(1) + 200, Regras.PontosDaLeva(200, 0, 1));
    }

    [Test]
    public void PontosDaLeva_CadaTubaraoPerdidoDesconta()
    {
        int semPerda = Regras.PontosDaLeva(500, 0, 3);
        Assert.AreEqual(semPerda - Regras.PenalidadePorTubarao, Regras.PontosDaLeva(500, 1, 3));
        Assert.AreEqual(semPerda - 2 * Regras.PenalidadePorTubarao, Regras.PontosDaLeva(500, 2, 3));
    }

    [Test]
    public void PontosDaLeva_NuncaFicaNegativo()
    {
        Assert.AreEqual(0, Regras.PontosDaLeva(0, 10, 1));
    }

    [Test]
    public void PontosDaLeva_PerdasNegativasNaoViramBonus()
    {
        Assert.AreEqual(Regras.PontosDaLeva(100, 0, 2), Regras.PontosDaLeva(100, -3, 2));
    }
}
