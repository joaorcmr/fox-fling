using NUnit.Framework;
using static CenarioTeste;

// Quem guarda o resultado no Progresso é a partida (FimDePartida), não a tela
// de fim. Usa um prefixo de teste para não mexer no recorde de quem joga.
public class FimDePartidaTests
{
    string prefixoOriginal;

    [SetUp]
    public void Isolar()
    {
        prefixoOriginal = Salvamento.Prefixo;
        Salvamento.Prefixo = "teste_foxtower_";
        Progresso.Zerar();
    }

    [TearDown]
    public void Restaurar()
    {
        Progresso.Zerar();
        Salvamento.Prefixo = prefixoOriginal;
    }

    // Uma partida que cai na primeira onda: sem porcos e com 1 de vida na Torre.
    static void PerderNaOnda1(out FimDePartida fim)
    {
        var d = Basico();
        d.economia.vidaInicial = 1;
        d.ondas.Add(Onda(Grupo(EspeciePassaro.Pardal, 3)));
        var s = new Simulacao(d);
        fim = new FimDePartida(s);
        s.SoltarOnda();
        for (int i = 0; i < 60 * 60 && s.Estado != EstadoJogo.Derrota; i++) s.Passo();
        Assert.AreEqual(EstadoJogo.Derrota, s.Estado);
    }

    [Test]
    public void Derrota_RegistraORecordeEAvisaQueENovo()
    {
        PerderNaOnda1(out var fim);

        Assert.AreEqual(1, Progresso.MelhorOnda);
        Assert.AreEqual(0, fim.RecordeAnterior);
        Assert.IsTrue(fim.NovoRecorde);
    }

    [Test]
    public void Derrota_NaMesmaOnda_NaoENovoRecorde()
    {
        PerderNaOnda1(out _);
        PerderNaOnda1(out var segunda);

        Assert.AreEqual(1, segunda.RecordeAnterior);
        Assert.IsFalse(segunda.NovoRecorde);
        Assert.AreEqual(0, Progresso.Vitorias);
    }
}
