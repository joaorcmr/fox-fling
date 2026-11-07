using NUnit.Framework;
using UnityEngine;

// A trilha é o caminho que os pássaros seguem. Tudo no jogo mede "onde o
// pássaro está" pela distância percorrida nela, então estas contas precisam
// estar certas antes de qualquer outra coisa.
public class TrilhaTests
{
    [Test]
    public void RetaSimples_ComprimentoEPosicao()
    {
        var t = new Trilha(new[] { new Vector2(0, 0), new Vector2(10, 0) }, 1f);

        Assert.AreEqual(10f, t.Comprimento, 1e-3f);
        Assert.That(Vector2.Distance(t.Posicao(2.5f), new Vector2(2.5f, 0)), Is.LessThan(1e-3f));
        Assert.AreEqual(Vector2.zero, t.Posicao(-1f));
        Assert.That(Vector2.Distance(t.Posicao(99f), new Vector2(10, 0)), Is.LessThan(1e-3f));
        Assert.That(Vector2.Distance(t.Direcao(5f), Vector2.right), Is.LessThan(1e-3f));
    }

    [Test]
    public void CantoArredondado_EncurtaOCaminho()
    {
        // Um "L" de 10 + 10 com o canto trocado por um quarto de círculo de raio 1:
        // perde 2 unidades de reta e ganha π/2 de arco → 20 − 2 + 1,5708.
        var t = new Trilha(new[] { new Vector2(0, 0), new Vector2(10, 0), new Vector2(10, 10) }, 1f);

        Assert.AreEqual(19.5708f, t.Comprimento, 0.02f);
        // A metade do caminho cai no meio do arco (centro do arco em (9, 1)).
        Assert.That(Vector2.Distance(t.Posicao(t.Comprimento / 2f), new Vector2(9.7071f, 0.2929f)), Is.LessThan(0.03f));
        // A quina original fica de fora do arco: a distância é r·(√2 − 1).
        Assert.AreEqual(0.4142f, t.DistanciaAte(new Vector2(10, 0)), 0.05f);
    }

    [Test]
    public void DistanciaAte_PontoAoLado()
    {
        var t = new Trilha(new[] { new Vector2(0, 0), new Vector2(10, 0) }, 1f);

        Assert.AreEqual(3f, t.DistanciaAte(new Vector2(5, 3)), 1e-3f);
        Assert.AreEqual(2f, t.DistanciaAte(new Vector2(-2, 0)), 1e-3f);
    }

    [Test]
    public void CantoCurto_RaioLimitadoPeloSegmento()
    {
        // O primeiro trecho tem só 1 unidade: o arco não pode ter raio 1, senão
        // "comeria" o trecho inteiro. O raio cai para a metade (0,5).
        var t = new Trilha(new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 10) }, 1f);

        Assert.That(float.IsNaN(t.Comprimento) || float.IsInfinity(t.Comprimento), Is.False);
        Assert.That(t.Comprimento, Is.LessThan(11f));
        for (float d = 0f; d <= t.Comprimento; d += 0.05f)
        {
            var p = t.Posicao(d);
            Assert.That(float.IsNaN(p.x) || float.IsNaN(p.y), Is.False, $"NaN em d = {d}");
        }
    }
}
