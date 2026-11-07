using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using static CenarioTeste;

// Os tiros: pedrinha acerta um, arpão atravessa a fila, bomba explode no ponto
// mirado, couraça corta o dano, e a recarga marca o ritmo.
public class CombateTests
{
    static DadosJogo ComPorco(EspeciePorco e, Atributos a, float vidaPardal = 5f)
    {
        var d = Basico();
        d.economia.penasIniciais = 10000;
        d.passaros[EspeciePassaro.Pardal] = Passaro(EspeciePassaro.Pardal, vida: vidaPardal);
        d.porcos[e] = Porco(e, 100, a);
        return d;
    }

    static Porco Comprar(Simulacao s, EspeciePorco e, float x, float y)
    {
        Assert.AreEqual(Resultado.Ok, s.Comprar(e, new Vector2(x, y), out var porco));
        return porco;
    }

    [Test]
    public void Pedrinha_AcertaUmSo()
    {
        var s = ComPassarosParados(ComPorco(EspeciePorco.Estilingueiro, Tiro(TipoTiro.Pedrinha, perfura: 1)),
            EspeciePassaro.Pardal, 9f, 9.3f);
        Comprar(s, EspeciePorco.Estilingueiro, 11f, 9.6f);

        Avancar(s, 1f);

        Assert.AreEqual(1, s.Passaros.Count(p => p.Vida < 5f));
    }

    [Test]
    public void Arpao_AtravessaVarios()
    {
        // A trilha termina em x = 8 e o arpoador fica alinhado com ela, em (9,5; 7,5):
        // o arpão voa ao longo da fila de pardais.
        var d = ComPorco(EspeciePorco.Arpoador, new Atributos
        {
            tiro = TipoTiro.Arpao, dano = 2f, alcance = 5f, recarga = 10f, velocidadeProjetil = 14f,
            raioProjetil = 0.18f, perfura = 4, alcanceVoo = 1.6f,
        }, vidaPardal: 1f);
        d.mapa.pontos = new List<Vector2> { new Vector2(0f, 7.5f), new Vector2(8f, 7.5f) };
        var s = ComPassarosParados(d, EspeciePassaro.Pardal, 3f, 4f, 5f, 6f, 7f);
        Comprar(s, EspeciePorco.Arpoador, 9.5f, 7.5f);

        Avancar(s, 1.5f);

        Assert.AreEqual(1, s.Passaros.Count(p => p.Vivo)); // perfura 4: estoura 4 de 5
    }

    [Test]
    public void Arpao_ContinuaDepoisDoAlvo()
    {
        var d = ComPorco(EspeciePorco.Arpoador, new Atributos
        {
            tiro = TipoTiro.Arpao, dano = 2f, alcance = 5f, recarga = 10f, velocidadeProjetil = 14f,
            raioProjetil = 0.18f, perfura = 4, alcanceVoo = 1.6f,
        }, vidaPardal: 10f);
        d.mapa.pontos = new List<Vector2> { new Vector2(0f, 7.5f), new Vector2(8f, 7.5f) };
        var s = ComPassarosParados(d, EspeciePassaro.Pardal, 7f);
        Comprar(s, EspeciePorco.Arpoador, 9.5f, 7.5f);

        Avancar(s, 0.3f); // já acertou (2,5 unidades a 14 u/s) e segue voando
        Assert.AreEqual(8f, s.Passaros[0].Vida, 1e-4f);
        Assert.AreEqual(1, s.Projeteis.Count);
        Assert.IsTrue(s.Projeteis[0].Ativo);

        Avancar(s, 0.4f); // voou 5 × 1,6 = 8 unidades e sumiu
        Assert.AreEqual(0, s.Projeteis.Count);
    }

    [Test]
    public void Bomba_ExplodeNoDestino_MesmoSemAlvo()
    {
        var d = ComPorco(EspeciePorco.Bombardeiro, new Atributos
        {
            tiro = TipoTiro.Bomba, dano = 2f, alcance = 5f, recarga = 10f, velocidadeProjetil = 8f, raioProjetil = 0.2f,
            raioArea = 1f,
        });
        d.passaros[EspeciePassaro.Urubu] = Passaro(EspeciePassaro.Urubu, vida: 5f, velocidade: 0f);
        d.passaros[EspeciePassaro.Urubu].couraca = true; // a bomba ignora a couraça
        d.ondas.Add(Onda(Grupo(EspeciePassaro.Pardal, 1, 0.0001f), Grupo(EspeciePassaro.Urubu, 1, 0.0001f)));
        d.ondas.Add(Onda(Grupo(EspeciePassaro.Pardal, 1)));
        d.passaros[EspeciePassaro.Pardal].velocidade = 0f;
        var s = new Simulacao(d);
        s.SoltarOnda();
        s.Passo();
        var alvo = s.Passaros.First(p => p.Especie == EspeciePassaro.Pardal);
        var vizinho = s.Passaros.First(p => p.Especie == EspeciePassaro.Urubu);
        alvo.Distancia = 6.6f; alvo.Posicao = alvo.PosicaoAnterior = s.Trilha.Posicao(6.6f);
        vizinho.Distancia = 6f; vizinho.Posicao = vizinho.PosicaoAnterior = s.Trilha.Posicao(6f);
        var explosoes = new List<Vector2>();
        s.BombaExplodiu += (centro, raio) => explosoes.Add(centro);
        Comprar(s, EspeciePorco.Bombardeiro, 6f, 10f);

        s.Passo();                       // atira no primeiro (o pardal em 6,6)
        s.Atingir(alvo, 100f, true);     // o alvo estoura antes de a bomba chegar
        Avancar(s, 1f);

        Assert.AreEqual(1, explosoes.Count);
        Assert.That(Vector2.Distance(explosoes[0], new Vector2(6.6f, 7.5f)), Is.LessThan(0.05f));
        Assert.AreEqual(3f, vizinho.Vida, 1e-4f); // dano cheio, apesar da couraça
    }

    [Test]
    public void Couraca_MetadeDoDano()
    {
        var d = Basico();
        d.passaros[EspeciePassaro.Urubu] = Passaro(EspeciePassaro.Urubu, vida: 10f);
        d.passaros[EspeciePassaro.Urubu].couraca = true;
        var s = ComPassarosParados(d, EspeciePassaro.Urubu, 3f);
        var urubu = s.Passaros[0];

        s.Atingir(urubu, 1f, false);   // pedrinha
        Assert.AreEqual(9.5f, urubu.Vida, 1e-4f);
        s.Atingir(urubu, 2f, true);    // bomba
        Assert.AreEqual(7.5f, urubu.Vida, 1e-4f);
        s.Atingir(urubu, 2f, true);    // arpão com Ponta de Ferro
        Assert.AreEqual(5.5f, urubu.Vida, 1e-4f);
    }

    [Test]
    public void Porco_RespeitaRecarga()
    {
        var s = ComPassarosParados(ComPorco(EspeciePorco.Estilingueiro, Tiro(TipoTiro.Pedrinha, recarga: 1f), vidaPardal: 1000f),
            EspeciePassaro.Pardal, 7f);
        Comprar(s, EspeciePorco.Estilingueiro, 7f, 10f);
        int tiros = 0;
        s.TiroDisparado += (porco, tiro) => tiros++;

        Avancar(s, 2.05f); // atira em 0 s, 1 s e 2 s

        Assert.AreEqual(3, tiros);
    }
}
