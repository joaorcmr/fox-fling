using NUnit.Framework;

// Usa um prefixo próprio para não apagar o progresso de verdade do jogo.
public class ProgressoTests
{
    string prefixoOriginal;

    [SetUp]
    public void Preparar()
    {
        prefixoOriginal = Progresso.Prefixo;
        Progresso.Prefixo = "teste_";
        Progresso.Zerar();
    }

    [TearDown]
    public void Limpar()
    {
        Progresso.Zerar();
        Progresso.Prefixo = prefixoOriginal;
    }

    [Test]
    public void ComecaSoComAPrimeiraFaseLiberada()
    {
        Assert.AreEqual(1, Progresso.FaseMaisAltaLiberada);
        Assert.AreEqual(0, Progresso.MelhoresEstrelas(1));
    }

    [Test]
    public void VencerLiberaAProximaEGuardaEstrelas()
    {
        Progresso.RegistrarVitoria(1, 2);
        Assert.AreEqual(2, Progresso.FaseMaisAltaLiberada);
        Assert.AreEqual(2, Progresso.MelhoresEstrelas(1));
    }

    [Test]
    public void ResultadoPiorNaoApagaOMelhor()
    {
        Progresso.RegistrarVitoria(1, 2);
        Progresso.RegistrarVitoria(1, 1);
        Assert.AreEqual(2, Progresso.MelhoresEstrelas(1));
    }

    [Test]
    public void UltimaFaseNaoLiberaFaseInexistente()
    {
        for (int f = 1; f <= Progresso.TotalFases; f++) Progresso.RegistrarVitoria(f, 3);
        Assert.AreEqual(Progresso.TotalFases, Progresso.FaseMaisAltaLiberada);
    }

    [Test]
    public void VencerFaseAntigaNaoBloqueiaAsSeguintes()
    {
        Progresso.RegistrarVitoria(1, 1);
        Progresso.RegistrarVitoria(2, 1);
        Progresso.RegistrarVitoria(1, 3);
        Assert.AreEqual(3, Progresso.FaseMaisAltaLiberada);
    }
}
