using NUnit.Framework;

// O que fica salvo entre partidas (PlayerPrefs): melhor onda, vitórias, dicas
// já vistas e volumes. Os testes usam um prefixo próprio para não apagar o
// progresso de verdade de quem joga no mesmo computador.
public class ProgressoTests
{
    string prefixoOriginal;

    [SetUp]
    public void Isolar()
    {
        prefixoOriginal = Progresso.Prefixo;
        Progresso.Prefixo = "teste_revoada_";
        Progresso.Zerar();
        ApagarVolumes();
    }

    [TearDown]
    public void Restaurar()
    {
        Progresso.Zerar();
        ApagarVolumes();
        Progresso.Prefixo = prefixoOriginal;
    }

    // O Zerar() de propósito não mexe nos volumes (são preferência, não
    // progresso), então o teste limpa os que ele mesmo grava.
    static void ApagarVolumes()
    {
        UnityEngine.PlayerPrefs.DeleteKey(Progresso.Prefixo + "volume_efeitos");
        UnityEngine.PlayerPrefs.DeleteKey(Progresso.Prefixo + "volume_musica");
    }

    [Test]
    public void RegistrarFim_GuardaAMelhor()
    {
        Progresso.RegistrarFim(7, false);
        Progresso.RegistrarFim(4, false);

        Assert.AreEqual(7, Progresso.MelhorOnda);
    }

    [Test]
    public void RegistrarFim_ContaVitorias()
    {
        Progresso.RegistrarFim(30, true);
        Progresso.RegistrarFim(12, false);
        Progresso.RegistrarFim(30, true);

        Assert.AreEqual(2, Progresso.Vitorias);
        Assert.AreEqual(30, Progresso.MelhorOnda);
    }

    [Test]
    public void Dica_MarcadaUmaVez()
    {
        Assert.IsFalse(Progresso.DicaVista("arpao"));
        Progresso.MarcarDica("arpao");

        Assert.IsTrue(Progresso.DicaVista("arpao"));
        Assert.IsFalse(Progresso.DicaVista("ninho"));
    }

    [Test]
    public void Volume_GuardaENaoSaiDoIntervalo()
    {
        Assert.AreEqual(0.8f, Progresso.Volume("efeitos", 0.8f), 1e-5f); // padrão
        Progresso.SalvarVolume("efeitos", 0.3f);
        Progresso.SalvarVolume("musica", 7f);

        Assert.AreEqual(0.3f, Progresso.Volume("efeitos", 0.8f), 1e-5f);
        Assert.AreEqual(1f, Progresso.Volume("musica", 0.5f), 1e-5f);
    }

    [Test]
    public void Zerar_LimpaTudo()
    {
        Progresso.RegistrarFim(9, true);
        Progresso.MarcarDica("tambor");

        Progresso.Zerar();

        Assert.AreEqual(0, Progresso.MelhorOnda);
        Assert.AreEqual(0, Progresso.Vitorias);
        Assert.IsFalse(Progresso.DicaVista("tambor"));
    }
}
