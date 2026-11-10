using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using static CenarioTeste;

// O jogo se divide em 3 fases (10 ondas cada), um mapa (caminho) e um turno do
// dia diferentes por fase (issue #14). Os mapas de verdade (Fazenda, Pomar,
// Floresta) usam as mesmas regras de posição dos testes de DadosTests: a
// trilha dentro do mapa, os obstáculos longe dela, a Torre no fim dela.
public class FasesTests
{
    static IEnumerable<DadosMapa> MapasDeVerdade()
    {
        yield return DefinicoesPadrao.Fazenda();
        yield return DefinicoesPadrao.Pomar();
        yield return DefinicoesPadrao.Floresta();
    }

    [TestCaseSource(nameof(MapasDeVerdade))]
    public void MapaDaFase_TrilhaDentroDoMapaEObstaculosLongeDela(DadosMapa d)
    {
        var trilha = new Trilha(d.pontos, d.raioCanto);

        for (float s = 1.5f; s <= trilha.Comprimento; s += 0.1f)
        {
            var p = trilha.Posicao(s);
            Assert.That(p.x >= 0f && p.x <= d.tamanho.x && p.y >= 0f && p.y <= d.tamanho.y,
                $"{d.nome}: a trilha sai do mapa em d = {s:F1}: {p}");
        }

        foreach (var o in d.obstaculos)
            Assert.That(trilha.DistanciaAte(o.centro), Is.GreaterThanOrEqualTo(o.raio + d.meiaLargura),
                $"{d.nome}: {o.tipo} em {o.centro} encosta na trilha");

        var fim = d.pontos[d.pontos.Count - 1];
        Assert.That(Vector2.Distance(fim, d.torre), Is.LessThan(d.raioTorre + 0.2f), $"{d.nome}: a Torre não está no fim da trilha");
    }

    [Test]
    public void TresMapas_CaminhosDiferentesETurnosDiferentes()
    {
        var mapas = MapasDeVerdade().ToList();
        Assert.AreEqual(3, mapas.Select(m => m.turno).Distinct().Count(), "os 3 mapas deviam ter turnos diferentes");

        // "Caminho diferente": nenhum ponto da trilha de um mapa é igual ao do outro.
        for (int i = 0; i < mapas.Count; i++)
            for (int j = i + 1; j < mapas.Count; j++)
                CollectionAssert.AreNotEqual(mapas[i].pontos, mapas[j].pontos, $"{mapas[i].nome} e {mapas[j].nome} têm o mesmo caminho");
    }

    static DadosJogo ComTresFasesDeUmaOnda()
    {
        var d = Basico(); // trilha reta de teste; cada fase troca para um mapa diferente
        d.passaros[EspeciePassaro.Pardal] = Passaro(EspeciePassaro.Pardal);
        d.ondas.Clear();
        for (int i = 0; i < 6; i++) d.ondas.Add(Onda(Grupo(EspeciePassaro.Pardal, 1, 10f))); // 6 ondas, 3 fases de 2
        d.fases = new List<DadosMapa>
        {
            NovoMapa("Fase A", new Vector2(0f, 7.5f), new Vector2(14f, 7.5f)),
            NovoMapa("Fase B", new Vector2(0f, 3f), new Vector2(14f, 3f)),
            NovoMapa("Fase C", new Vector2(0f, 11f), new Vector2(14f, 11f)),
        };
        return d;
    }

    static DadosMapa NovoMapa(string nome, Vector2 inicio, Vector2 fim) => new DadosMapa
    {
        nome = nome,
        tamanho = new Vector2(14f, 15f),
        pontos = new List<Vector2> { inicio, fim },
        meiaLargura = 0.6f,
        raioCanto = 1f,
        torre = fim,
        raioTorre = 1.1f,
    };

    [Test]
    public void TerminarOnda_NaUltimaDeCadaFase_TrocaOMapaAntesDaProxima()
    {
        // A troca acontece ao TERMINAR a última onda da fase (não ao soltar a
        // primeira da próxima): o jogador vê o mapa novo e tem o Intervalo
        // inteiro para se preparar antes de a onda chegar nele (#14).
        var s = new Simulacao(ComTresFasesDeUmaOnda());
        var fases = new List<int>();
        s.FaseComecou += f => fases.Add(f);

        Assert.AreEqual(1, s.Fase);
        Assert.AreEqual("Fase A", s.Dados.mapa.nome);

        AcabarOnda(s, soltar: true); // onda 1, fase 1
        Assert.AreEqual(1, s.Fase, "ainda não é o fim da fase 1 (faltam 2 ondas)");

        AcabarOnda(s, soltar: true); // onda 2: última da fase 1
        Assert.AreEqual(2, s.Fase, "ao terminar a onda 2, já é hora de trocar para a fase 2");
        Assert.AreEqual("Fase B", s.Dados.mapa.nome);

        AcabarOnda(s, soltar: true); // onda 3, fase 2
        Assert.AreEqual(2, s.Fase);

        AcabarOnda(s, soltar: true); // onda 4: última da fase 2
        Assert.AreEqual(3, s.Fase);
        Assert.AreEqual("Fase C", s.Dados.mapa.nome);

        CollectionAssert.AreEqual(new[] { 2, 3 }, fases, "o evento só deveria disparar quando a fase muda de verdade");
    }

    [Test]
    public void TrocaDeFase_ADistanciaDaTrilhaMudaParaOMapaNovo()
    {
        var s = new Simulacao(ComTresFasesDeUmaOnda());
        var antesDaTroca = s.Trilha;

        AcabarOnda(s, soltar: true); // onda 1, fase 1
        Assert.AreSame(antesDaTroca, s.Trilha, "ainda na fase 1: a trilha não devia ter mudado");

        AcabarOnda(s, soltar: true); // onda 2: última da fase 1 — troca ao terminar
        Assert.AreNotSame(antesDaTroca, s.Trilha, "a trilha precisa ser a do mapa novo");
        // A trilha da Fase B vai de (0,3) a (14,3): bem longe do y=7,5 da Fase A.
        Assert.Less(s.Trilha.DistanciaAte(new Vector2(7f, 3f)), 0.1f);
    }

    [Test]
    public void TrocaDeFase_SoRemoveQuemFicaEmCimaDaTrilhaNova_EDevolve70PorCento()
    {
        var d = ComTresFasesDeUmaOnda();
        d.economia.moedasIniciais = 200;
        d.economia.bonusOndaBase = 0;
        d.economia.bonusOndaPorOnda = 0; // isola o teste: só o gasto e o reembolso da venda devem mexer nas moedas
        d.porcos[EspeciePorco.Estilingueiro] = Porco(EspeciePorco.Estilingueiro, 100, Tiro(TipoTiro.Pedrinha, alcance: 0.01f));
        var s = new Simulacao(d);

        // A trilha da Fase A vai de (0,7.5) a (14,7.5); a da Fase B, de (0,3) a (14,3).
        Assert.AreEqual(Resultado.Ok, s.Comprar(EspeciePorco.Estilingueiro, new Vector2(7f, 10f), out var longe));   // longe das duas trilhas
        Assert.AreEqual(Resultado.Ok, s.Comprar(EspeciePorco.Estilingueiro, new Vector2(2f, 3.3f), out var naFaseB)); // em cima da trilha da Fase B
        Porco removido = null;
        s.PorcoVendido += p => removido = p;

        AcabarOnda(s, soltar: true); // onda 1
        Assert.AreEqual(2, s.Porcos.Count, "ainda na mesma fase: os dois continuam no mapa");

        AcabarOnda(s, soltar: true); // onda 2: última da fase 1 — troca ao terminar

        CollectionAssert.Contains(s.Porcos, longe, "longe da trilha nova: continua no mesmo lugar, nível e tudo");
        CollectionAssert.DoesNotContain(s.Porcos, naFaseB, "em cima da trilha nova: não tem mais onde ficar");
        Assert.AreSame(naFaseB, removido);
        Assert.AreEqual(70, s.Moedas, "devolve 70% do gasto (100) só de quem foi removido");
    }

    // Solta a próxima onda (se soltar) e avança até ela terminar.
    static void AcabarOnda(Simulacao s, bool soltar = false)
    {
        if (soltar) s.SoltarOnda();
        for (int i = 0; i < 60 * 30 && s.Estado == EstadoJogo.EmOnda; i++) s.Passo();
        Assert.AreNotEqual(EstadoJogo.EmOnda, s.Estado, "a onda não terminou a tempo");
    }
}
