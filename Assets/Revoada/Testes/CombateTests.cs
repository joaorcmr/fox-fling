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
        d.economia.moedasIniciais = 10000;
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

    // --- Lama, empurrão, pata e leque (Task 7) ---

    static Simulacao UmPassaroAndando(bool chefe = false)
    {
        var d = Basico();
        d.passaros[EspeciePassaro.Pardal] = Passaro(EspeciePassaro.Pardal, vida: 50f, velocidade: 2f);
        d.passaros[EspeciePassaro.Pardal].chefe = chefe;
        d.ondas.Add(Onda(Grupo(EspeciePassaro.Pardal, 1)));
        d.ondas.Add(Onda(Grupo(EspeciePassaro.Pardal, 1)));
        var s = new Simulacao(d);
        s.SoltarOnda();
        s.Passo();
        return s;
    }

    [Test]
    public void Lentidao_MaisForteVale()
    {
        var s = UmPassaroAndando();
        var p = s.Passaros[0];

        s.AplicarLentidao(p, 0.4f, 1.5f);
        s.AplicarLentidao(p, 0.2f, 3f); // mais fraca, porém mais longa

        Assert.AreEqual(0.4f, p.Lentidao, 1e-5f);
        Assert.AreEqual(3f, p.TempoLentidao, 1e-5f);
        Assert.AreEqual(2f * 0.6f, p.Velocidade, 1e-5f);
    }

    [Test]
    public void Lentidao_AcabaNoTempo()
    {
        var s = UmPassaroAndando();
        var p = s.Passaros[0];

        s.AplicarLentidao(p, 0.4f, 1f);
        Avancar(s, 1.1f);

        Assert.AreEqual(0f, p.Lentidao, 1e-5f);
        Assert.AreEqual(2f, p.Velocidade, 1e-5f);
    }

    [Test]
    public void Chefe_LentidaoMaxima30()
    {
        var s = UmPassaroAndando(chefe: true);
        var p = s.Passaros[0];

        s.AplicarLentidao(p, 0.4f, 1f);

        Assert.AreEqual(0.3f, p.Lentidao, 1e-5f);
    }

    [Test]
    public void Empurrao_VoltaNaTrilha()
    {
        var s = UmPassaroAndando();
        var p = s.Passaros[0];

        p.Distancia = 5f;
        s.Empurrar(p, 1f);
        Assert.AreEqual(4f, p.Distancia, 1e-5f);
        Assert.That(Vector2.Distance(p.Posicao, s.Trilha.Posicao(4f)), Is.LessThan(1e-4f));

        p.Distancia = 0.5f;
        s.Empurrar(p, 1f);
        Assert.AreEqual(0f, p.Distancia, 1e-5f); // não volta para antes do começo
    }

    [Test]
    public void Chefe_NaoEmpurrado()
    {
        var s = UmPassaroAndando(chefe: true);
        var p = s.Passaros[0];
        p.Distancia = 5f;

        s.Empurrar(p, 1f);

        Assert.AreEqual(5f, p.Distancia, 1e-5f);
    }

    [Test]
    public void BombaComLama_DeixaLentoEEmpurra()
    {
        var d = ComPorco(EspeciePorco.Bombardeiro, new Atributos
        {
            tiro = TipoTiro.Bomba, dano = 1f, alcance = 5f, recarga = 10f, velocidadeProjetil = 8f, raioProjetil = 0.2f,
            raioArea = 1f, lentidao = 0.4f, duracaoLentidao = 1.5f, empurrao = 1f,
        }, vidaPardal: 50f);
        var s = ComPassarosParados(d, EspeciePassaro.Pardal, 7f);
        var p = s.Passaros[0];
        Comprar(s, EspeciePorco.Bombardeiro, 7f, 10f);

        Avancar(s, 0.6f); // a bomba voa 2,5 unidades a 8 u/s

        Assert.AreEqual(0.4f, p.Lentidao, 1e-5f);
        Assert.AreEqual(6f, p.Distancia, 1e-4f);
    }

    [Test]
    public void Pata_SoltaQuatroPatinhos()
    {
        var d = Basico();
        d.passaros[EspeciePassaro.Pata] = Passaro(EspeciePassaro.Pata, vida: 8f, moedas: 4f);
        d.passaros[EspeciePassaro.Pata].filho = EspeciePassaro.Patinho;
        d.passaros[EspeciePassaro.Pata].quantidadeFilhos = 4;
        d.passaros[EspeciePassaro.Patinho] = Passaro(EspeciePassaro.Patinho, velocidade: 3f);
        var s = ComPassarosParados(d, EspeciePassaro.Pata, 6f);
        int nascidos = 0;
        s.PassaroNasceu += p => nascidos++;

        s.Atingir(s.Passaros[0], 100f, false);

        var patinhos = s.Passaros.Where(p => p.Especie == EspeciePassaro.Patinho).Select(p => p.Distancia).ToArray();
        Assert.AreEqual(4, nascidos);
        CollectionAssert.AreEqual(new[] { 6f, 5.7f, 5.4f, 5.1f }, patinhos, new Aproximado(1e-4f));
        Avancar(s, 0.1f);
        Assert.AreEqual(EstadoJogo.EmOnda, s.Estado); // a onda continua enquanto houver patinho
    }

    [Test]
    public void Pata_NoComeco_PatinhosNaoFicamNegativos()
    {
        var d = Basico();
        d.passaros[EspeciePassaro.Pata] = Passaro(EspeciePassaro.Pata, vida: 8f);
        d.passaros[EspeciePassaro.Pata].filho = EspeciePassaro.Patinho;
        d.passaros[EspeciePassaro.Pata].quantidadeFilhos = 4;
        d.passaros[EspeciePassaro.Patinho] = Passaro(EspeciePassaro.Patinho);
        var s = ComPassarosParados(d, EspeciePassaro.Pata, 0.5f);

        s.Atingir(s.Passaros[0], 100f, false);

        foreach (var p in s.Passaros.Where(x => x.Especie == EspeciePassaro.Patinho))
            Assert.GreaterOrEqual(p.Distancia, 0f);
    }

    [Test]
    public void Rajada_TresTirosEmLeque()
    {
        var a = Tiro(TipoTiro.Pedrinha, recarga: 10f);
        a.tiros = 3;
        a.abertura = 15f;
        var s = ComPassarosParados(ComPorco(EspeciePorco.Estilingueiro, a), EspeciePassaro.Pardal, 7f);
        var porco = Comprar(s, EspeciePorco.Estilingueiro, 7f, 10f);

        s.Passo();

        Assert.AreEqual(3, s.Projeteis.Count);
        var angulos = s.Projeteis.Select(t => Vector2.SignedAngle(porco.Mira, t.Velocidade)).OrderBy(x => x).ToArray();
        Assert.AreEqual(-15f, angulos[0], 0.5f);
        Assert.AreEqual(0f, angulos[1], 0.5f);
        Assert.AreEqual(15f, angulos[2], 0.5f);
    }

    class Aproximado : System.Collections.IComparer
    {
        readonly float tolerancia;
        public Aproximado(float tolerancia) => this.tolerancia = tolerancia;
        public int Compare(object x, object y) => System.Math.Abs((float)x - (float)y) <= tolerancia ? 0 : ((float)x).CompareTo((float)y);
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
