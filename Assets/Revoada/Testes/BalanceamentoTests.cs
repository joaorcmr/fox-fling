using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

// "Difícil na medida certa", medido. Robôs jogam partidas inteiras com os
// números de verdade (os .asset), sem desenhar nada, e cada um precisa terminar
// onde o design manda: quem não faz nada perde logo; quem só defende ou só
// investe em moedas perde no meio; quem mistura as duas coisas vence. Se alguém
// mexer num número e quebrar esse equilíbrio, o teste acusa.
public class BalanceamentoTests
{
    static DadosJogo dados;

    [OneTimeSetUp]
    public void Carregar()
    {
        ConstrutorDados.GarantirDados(false);
        dados = ConstrutorDados.Carregar();
    }

    static ResultadoPartida Jogar(Estrategia e) => Robos.Jogar(dados, e);

    // A meta do design era "até a onda 3", mas com pardais de 1 de dano as três
    // primeiras ondas somam 55 de dano: quem não faz nada cai na onda 5.
    [Test]
    public void Parado_PerdeAteAOnda5()
    {
        var r = Jogar(Estrategia.Parado);
        Assert.IsFalse(r.venceu);
        Assert.LessOrEqual(r.ondaAlcancada, 5, r.ToString());
    }

    // As 3 fases (#14) trocam de mapa a cada 10 ondas, e na troca TODOS os
    // porcos voltam para a carteira pelo valor inteiro: o robô remonta a
    // defesa do zero no mapa novo (EstadoRobo.RecomecarOrdem), antes de pensar
    // no Cofrinho. Como todo mundo começa cada fase com a defesa no lugar
    // certo, as ondas da fase 3 vêm com 25% a mais de vida
    // (DefinicoesPadrao.VidaExtraFase3); sem isso, até o Defesa pura vencia a
    // onda 30. Com os números de agora: Só Estilingue cai na 24, Defesa pura
    // na 22 e o Equilibrado vence com 40 de vida. Rode os robôs
    // (Fox Tower > Balanceamento) depois de mexer em qualquer número de onda,
    // mapa ou fase.
    [Test]
    public void SoEstilingue_PerdeEntre20e27()
    {
        var r = Jogar(Estrategia.SoEstilingue);
        Assert.IsFalse(r.venceu, r.ToString());
        Assert.That(r.ondaAlcancada, Is.InRange(20, 27), r.ToString());
    }

    [Test]
    public void DefesaPura_PerdeEntre17e23()
    {
        var r = Jogar(Estrategia.DefesaPura);
        Assert.IsFalse(r.venceu, r.ToString());
        Assert.That(r.ondaAlcancada, Is.InRange(17, 23), r.ToString());
    }

    [Test]
    public void Ganancioso_PerdeAntesDa12()
    {
        var r = Jogar(Estrategia.Ganancioso);
        Assert.IsFalse(r.venceu, r.ToString());
        Assert.Less(r.ondaAlcancada, 12, r.ToString());
    }

    [Test]
    public void Equilibrado_VenceComVida5()
    {
        var r = Jogar(Estrategia.Equilibrado);
        Assert.IsTrue(r.venceu, r.ToString());
        Assert.GreaterOrEqual(r.vidaFinal, 5, r.ToString());
    }

    [Test]
    public void Equilibrado_SemPicos()
    {
        var r = Jogar(Estrategia.Equilibrado);
        // O maior pico é a onda 19 (urubus de couraça, logo antes do chefe 2).
        Assert.LessOrEqual(r.danoPorOnda.Max(), 40, r.ToString());
    }

    [Test]
    public void Simulacao_Deterministica()
    {
        var a = Jogar(Estrategia.Equilibrado);
        var b = Jogar(Estrategia.Equilibrado);
        Assert.AreEqual(a.ToString(), b.ToString());
        CollectionAssert.AreEqual(a.danoPorOnda, b.danoPorOnda);
        CollectionAssert.AreEqual(a.moedasPorOnda, b.moedasPorOnda);
    }

    [Test]
    public void Estresse_RodaRapido()
    {
        // 200 pássaros e 30 porcos: 600 passos precisam caber folgados em 1,5 s.
        var d = DefinicoesPadrao.Jogo();
        d.economia.moedasIniciais = 100000;
        d.ondas.Clear();
        d.ondas.Add(new DadosOnda { grupos = { new GrupoOnda { especie = EspeciePassaro.Arara, quantidade = 200, intervalo = 0.01f, multiplicadorVida = 50f } } });
        var s = new Simulacao(d);
        var especies = new[] { EspeciePorco.Estilingueiro, EspeciePorco.Arpoador, EspeciePorco.Bombardeiro };
        int colocados = 0;
        foreach (var lugar in Robos.LugaresBons(s, EspeciePorco.Estilingueiro))
        {
            if (colocados >= 30) break;
            if (s.Comprar(especies[colocados % 3], lugar, out _) == Resultado.Ok) colocados++;
        }
        Assert.AreEqual(30, colocados);
        s.SoltarOnda();

        var relogio = Stopwatch.StartNew();
        for (int i = 0; i < 600; i++) s.Passo();
        relogio.Stop();

        Assert.Less(relogio.Elapsed.TotalSeconds, 1.5, $"{relogio.Elapsed.TotalSeconds:F2} s");
    }
}
