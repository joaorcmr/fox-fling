using System.Linq;
using NUnit.Framework;
using UnityEngine;

// Confere os DADOS de verdade do jogo (os .asset das pastas Dados de cada assunto): se
// alguém mexer num número no Inspector e criar uma onda impossível de ler ou um
// obstáculo em cima da trilha, estes testes avisam.
public class DadosTests
{
    static DadosJogo Carregar()
    {
        ConstrutorDados.GarantirDados(false);
        return ConstrutorDados.Carregar();
    }

    [Test]
    public void Ha30Ondas_ChefeSoNas10_20_30()
    {
        var d = Carregar();

        Assert.AreEqual(30, d.ondas.Count);
        for (int i = 0; i < d.ondas.Count; i++)
        {
            bool temChefe = d.ondas[i].grupos.Any(g => g.especie == EspeciePassaro.Gaviao);
            Assert.AreEqual((i + 1) % 10 == 0, temChefe, $"onda {i + 1}");
        }
    }

    [Test]
    public void OndasUsamPassarosQueExistem()
    {
        var d = Carregar();

        for (int i = 0; i < d.ondas.Count; i++)
            foreach (var g in d.ondas[i].grupos)
            {
                Assert.IsTrue(d.passaros.ContainsKey(g.especie), $"onda {i + 1}: {g.especie} não existe");
                Assert.Greater(g.quantidade, 0, $"onda {i + 1}");
                Assert.Greater(g.intervalo, 0f, $"onda {i + 1}");
            }
    }

    [Test]
    public void TrilhaDentroDoMapa()
    {
        var d = Carregar();
        var trilha = new Trilha(d.mapa.pontos, d.mapa.raioCanto);

        // Os pássaros entram de fora do mapa; depois de 1,5 unidade já estão dentro.
        for (float s = 1.5f; s <= trilha.Comprimento; s += 0.1f)
        {
            var p = trilha.Posicao(s);
            Assert.That(p.x >= 0f && p.x <= d.mapa.tamanho.x && p.y >= 0f && p.y <= d.mapa.tamanho.y,
                $"a trilha sai do mapa em d = {s:F1}: {p}");
        }
    }

    [Test]
    public void ObstaculosForaDaTrilha()
    {
        var d = Carregar();
        var trilha = new Trilha(d.mapa.pontos, d.mapa.raioCanto);

        foreach (var o in d.mapa.obstaculos)
            Assert.That(trilha.DistanciaAte(o.centro), Is.GreaterThanOrEqualTo(o.raio + d.mapa.meiaLargura),
                $"{o.tipo} em {o.centro} encosta na trilha");

        // A Torre fica no FIM da trilha: é lá que os pássaros chegam.
        var fim = d.mapa.pontos[d.mapa.pontos.Count - 1];
        Assert.That(Vector2.Distance(fim, d.mapa.torre), Is.LessThan(d.mapa.raioTorre + 0.2f));
    }

    [Test]
    public void PorcosTemNumerosValidos()
    {
        var d = Carregar();

        Assert.AreEqual(4, d.porcos.Count);
        foreach (var p in d.porcos.Values)
        {
            Assert.Greater(p.custo, 0, p.nome);
            Assert.IsNotNull(p.nivel1, p.nome);
            foreach (var m in new[] { p.nivel2, p.nivel3, p.especializacaoA, p.especializacaoB })
            {
                Assert.IsNotNull(m?.atributos, $"{p.nome}: melhoria sem números");
                Assert.Greater(m.custo, 0, $"{p.nome}: {m.nome}");
                Assert.IsFalse(string.IsNullOrEmpty(m.nome), p.nome);
            }
        }
    }

    [Test]
    public void HaLugarParaPorcos()
    {
        var d = Carregar();
        var trilha = new Trilha(d.mapa.pontos, d.mapa.raioCanto);
        float raio = d.porcos[EspeciePorco.Estilingueiro].raio;
        var nenhum = new System.Collections.Generic.List<Porco>();

        int validos = 0;
        for (float x = 0f; x <= d.mapa.tamanho.x; x += 0.5f)
            for (float y = 0f; y <= d.mapa.tamanho.y; y += 0.5f)
                if (Colocacao.Valida(new Vector2(x, y), raio, d.mapa, trilha, nenhum)) validos++;

        Assert.GreaterOrEqual(validos, 40);
    }

    [Test]
    public void NumerosPositivos()
    {
        var d = Carregar();

        Assert.AreEqual(8, d.passaros.Count);
        foreach (var p in d.passaros.Values)
        {
            Assert.Greater(p.vida, 0f, p.nome);
            Assert.Greater(p.velocidade, 0f, p.nome);
            Assert.Greater(p.moedas, 0f, p.nome);
            Assert.Greater(p.danoTorre, 0, p.nome);
        }
    }
}
