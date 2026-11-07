using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using static CenarioTeste;

// O ciclo de uma partida: soltar a onda, os pássaros nascem e andam, chegam na
// Torre ou estouram, a onda termina com bônus, e no fim vem vitória ou derrota.
public class OndasTests
{
    static Simulacao Sim(params DadosOnda[] ondas)
    {
        var d = Basico();
        d.ondas.AddRange(ondas);
        return new Simulacao(d);
    }

    [Test]
    public void SoltarOnda_NascemNoTempoCerto()
    {
        var s = Sim(Onda(Grupo(EspeciePassaro.Pardal, 3, 1f)));

        Assert.IsTrue(s.SoltarOnda());
        Avancar(s, 0.01f);
        Assert.AreEqual(1, s.Passaros.Count);
        Avancar(s, 1f);
        Assert.AreEqual(2, s.Passaros.Count);
        Avancar(s, 1f);
        Assert.AreEqual(3, s.Passaros.Count);
        Assert.AreEqual(EstadoJogo.EmOnda, s.Estado);
        Assert.AreEqual(1, s.Onda);
    }

    [Test]
    public void Passaro_AndaPelaTrilha()
    {
        var s = Sim(Onda(Grupo(EspeciePassaro.Pardal, 1)));

        s.SoltarOnda();
        Avancar(s, 1f);

        var p = s.Passaros[0];
        Assert.AreEqual(2f, p.Distancia, 0.05f); // velocidade 2 durante 1 s
        Assert.That(Vector2.Distance(p.Posicao, s.Trilha.Posicao(p.Distancia)), Is.LessThan(1e-4f));
    }

    [Test]
    public void Passaro_ChegaNaTorre_TiraVida()
    {
        var d = Basico();
        d.passaros[EspeciePassaro.Pardal] = Passaro(EspeciePassaro.Pardal, velocidade: 7f, dano: 3);
        d.ondas.Add(Onda(Grupo(EspeciePassaro.Pardal, 1)));
        var s = new Simulacao(d);
        int golpes = 0, dano = 0;
        s.TorreAtingida += x => { golpes++; dano = x; };

        s.SoltarOnda();
        Avancar(s, 2.1f); // 14 unidades a 7 u/s = 2 s

        Assert.AreEqual(97, s.Vida);
        Assert.AreEqual(1, golpes);
        Assert.AreEqual(3, dano);
        Assert.AreEqual(0, s.Passaros.Count);
    }

    [Test]
    public void Onda_Termina_DaBonus()
    {
        var s = Sim(Onda(Grupo(EspeciePassaro.Pardal, 1)), Onda(Grupo(EspeciePassaro.Pardal, 1)));
        int ondaFim = -1, bonus = -1;
        s.OndaTerminou += (o, b) => { ondaFim = o; bonus = b; };

        s.SoltarOnda();
        Avancar(s, 0.01f);
        s.Atingir(s.Passaros[0], 5f, false);
        Avancar(s, 0.01f);

        Assert.AreEqual(1, ondaFim);
        Assert.AreEqual(30, bonus);
        Assert.AreEqual(300 + 1 + 30, s.Penas);
        Assert.AreEqual(EstadoJogo.Intervalo, s.Estado);
    }

    [Test]
    public void Auto_SoltaDepoisDaEspera()
    {
        var s = Sim(Onda(Grupo(EspeciePassaro.Pardal, 1)), Onda(Grupo(EspeciePassaro.Pardal, 1)));
        s.Auto = true;

        s.SoltarOnda();
        Avancar(s, 0.01f);
        s.Atingir(s.Passaros[0], 5f, false);
        Avancar(s, 0.01f);
        Assert.AreEqual(EstadoJogo.Intervalo, s.Estado);

        Avancar(s, 1.9f);
        Assert.AreEqual(EstadoJogo.Intervalo, s.Estado);
        Avancar(s, 0.2f);
        Assert.AreEqual(EstadoJogo.EmOnda, s.Estado);
        Assert.AreEqual(2, s.Onda);
    }

    [Test]
    public void Auto_NaoSoltaAPrimeira()
    {
        var s = Sim(Onda(Grupo(EspeciePassaro.Pardal, 1)));
        s.Auto = true;

        Avancar(s, 5f);

        Assert.AreEqual(EstadoJogo.Preparando, s.Estado);
        Assert.AreEqual(0, s.Onda);
    }

    [Test]
    public void UltimaOnda_DaVitoria()
    {
        var s = Sim(Onda(Grupo(EspeciePassaro.Pardal, 1)));
        var fins = new List<bool>();
        s.FimDeJogo += v => fins.Add(v);

        s.SoltarOnda();
        Avancar(s, 0.01f);
        s.Atingir(s.Passaros[0], 5f, false);
        Avancar(s, 0.05f);

        CollectionAssert.AreEqual(new[] { true }, fins);
        Assert.AreEqual(EstadoJogo.Vitoria, s.Estado);
        Assert.IsFalse(s.SoltarOnda());
    }

    [Test]
    public void VidaZero_DaDerrota()
    {
        var d = Basico();
        d.economia.vidaInicial = 2;
        d.passaros[EspeciePassaro.Pardal] = Passaro(EspeciePassaro.Pardal, velocidade: 7f, dano: 3);
        d.ondas.Add(Onda(Grupo(EspeciePassaro.Pardal, 2, 0.5f)));
        var s = new Simulacao(d);
        var fins = new List<bool>();
        s.FimDeJogo += v => fins.Add(v);

        s.SoltarOnda();
        Avancar(s, 2.1f);

        Assert.AreEqual(0, s.Vida);
        Assert.AreEqual(EstadoJogo.Derrota, s.Estado);
        CollectionAssert.AreEqual(new[] { false }, fins);

        // Depois da derrota o mundo para: o segundo pássaro não anda mais.
        var restante = s.Passaros.First(p => p.Vivo);
        float distancia = restante.Distancia;
        s.Passo();
        Assert.AreEqual(distancia, restante.Distancia);
    }

    [Test]
    public void MesmoPasso_DerrotaGanhaDaVitoria()
    {
        // Última onda: A zera a vida da Torre no mesmo passo em que B (o último
        // pássaro) estoura. Quem tem vida 0 perdeu, mesmo que a onda tenha acabado.
        var d = Basico();
        d.economia.vidaInicial = 1;
        d.passaros[EspeciePassaro.Pardal] = Passaro(EspeciePassaro.Pardal, velocidade: 2f, dano: 5);
        d.ondas.Add(Onda(Grupo(EspeciePassaro.Pardal, 2, 0.05f)));
        var s = new Simulacao(d);
        var fins = new List<bool>();
        s.FimDeJogo += v => fins.Add(v);

        s.SoltarOnda();
        Avancar(s, 0.1f);
        var a = s.Passaros[0];
        var b = s.Passaros[1];
        a.Distancia = s.Trilha.Comprimento - 0.01f;
        s.Atingir(b, 5f, false);
        s.Passo();

        Assert.AreEqual(EstadoJogo.Derrota, s.Estado);
        CollectionAssert.AreEqual(new[] { false }, fins);
    }

    [Test]
    public void Estouro_UsaMultiplicadorDoNinho()
    {
        var s = Sim(Onda(Grupo(EspeciePassaro.Pardal, 4, 0.01f)), Onda(Grupo(EspeciePassaro.Pardal, 1)));
        Assert.AreEqual(Resultado.Ok, s.ComprarNinho()); // ×1,25

        s.SoltarOnda();
        Avancar(s, 0.1f);
        int antes = s.Penas;
        foreach (var p in s.Passaros.ToArray()) s.Atingir(p, 5f, false);

        Assert.AreEqual(antes + 5, s.Penas); // 4 × 1 × 1,25
    }

    [Test]
    public void PenasBasePorOnda_ContaFilhos()
    {
        var d = Basico();
        d.passaros[EspeciePassaro.Pata] = Passaro(EspeciePassaro.Pata, penas: 4f);
        d.passaros[EspeciePassaro.Pata].filho = EspeciePassaro.Patinho;
        d.passaros[EspeciePassaro.Pata].quantidadeFilhos = 4;
        d.passaros[EspeciePassaro.Patinho] = Passaro(EspeciePassaro.Patinho, penas: 1f);
        d.ondas.Add(Onda(Grupo(EspeciePassaro.Pata, 2)));

        var s = new Simulacao(d);

        Assert.AreEqual(16f, s.PenasBasePorOnda[0], 1e-4f); // 2 × (4 + 4 × 1)
    }

    [Test]
    public void ResumoEEstreias()
    {
        var d = Basico();
        d.passaros[EspeciePassaro.Sabia] = Passaro(EspeciePassaro.Sabia);
        d.ondas.Add(Onda(Grupo(EspeciePassaro.Pardal, 3)));
        d.ondas.Add(Onda(Grupo(EspeciePassaro.Pardal, 2), Grupo(EspeciePassaro.Sabia, 1)));
        var s = new Simulacao(d);

        var resumo = s.ResumoOnda(2);
        Assert.AreEqual(2, resumo.Count);
        Assert.AreEqual(EspeciePassaro.Pardal, resumo[0].Key);
        Assert.AreEqual(2, resumo[0].Value);
        Assert.AreEqual(EspeciePassaro.Sabia, resumo[1].Key);
        Assert.AreEqual(1, resumo[1].Value);
        CollectionAssert.AreEqual(new[] { EspeciePassaro.Sabia }, s.Estreias(2));
        CollectionAssert.AreEqual(new[] { EspeciePassaro.Pardal }, s.Estreias(1));
    }
}
