using NUnit.Framework;
using UnityEngine;
using static CenarioTeste;

// Em quem o porco mira e para onde ele atira (o ponto onde o pássaro VAI estar).
public class AlvosTests
{
    static readonly Trilha Reta = new Trilha(new[] { new Vector2(0f, 7.5f), new Vector2(14f, 7.5f) }, 1f);

    static Passaro Em(float distancia, float vida, int id, float velocidade = 2f)
    {
        var p = new Passaro(id, Passaro(EspeciePassaro.Pardal, vida: vida, velocidade: velocidade), 1f, distancia);
        p.Posicao = Reta.Posicao(distancia);
        return p;
    }

    [Test]
    public void Primeiro_Ultimo_MaisForte_MaisPerto()
    {
        var a = Em(2f, 3f, 1);
        var b = Em(5f, 1f, 2);
        var c = Em(8f, 2f, 3);
        var lista = new[] { a, b, c };
        var origem = new Vector2(5f, 6.5f); // logo abaixo do b

        Assert.AreSame(c, Alvos.Escolher(origem, 10f, ModoAlvo.Primeiro, lista));
        Assert.AreSame(a, Alvos.Escolher(origem, 10f, ModoAlvo.Ultimo, lista));
        Assert.AreSame(a, Alvos.Escolher(origem, 10f, ModoAlvo.MaisForte, lista));
        Assert.AreSame(b, Alvos.Escolher(origem, 10f, ModoAlvo.MaisPerto, lista));

        c.Vivo = false; // pássaro estourado não é alvo
        Assert.AreSame(b, Alvos.Escolher(origem, 10f, ModoAlvo.Primeiro, lista));
    }

    [Test]
    public void MaisForte_EmpateVaiNoPrimeiro()
    {
        var lista = new[] { Em(2f, 2f, 1), Em(5f, 2f, 2), Em(8f, 2f, 3) };

        Assert.AreSame(lista[2], Alvos.Escolher(new Vector2(5f, 6.5f), 10f, ModoAlvo.MaisForte, lista));
    }

    [Test]
    public void ForaDoAlcance_Nulo()
    {
        var lista = new[] { Em(2f, 1f, 1), Em(5f, 1f, 2) };

        Assert.IsNull(Alvos.Escolher(new Vector2(5f, 1f), 1f, ModoAlvo.Primeiro, lista));
    }

    [Test]
    public void Antecipar_RetaMiraNaFrente()
    {
        // Pássaro em (5; 7,5) indo para +x a 2 u/s; tiro a 10 u/s saindo de (5; 4,5).
        // O encontro é onde x − 5 = 0,2·√((x − 5)² + 9): x ≈ 5,612.
        var alvo = Em(5f, 1f, 1, velocidade: 2f);

        var ponto = Alvos.Antecipar(new Vector2(5f, 4.5f), 10f, alvo, Reta);

        Assert.That(Vector2.Distance(ponto, new Vector2(5.612f, 7.5f)), Is.LessThan(0.02f));
    }

    [Test]
    public void Antecipar_NoFimDaTrilha_Limita()
    {
        var alvo = Em(Reta.Comprimento - 0.1f, 1f, 1, velocidade: 2f);

        var ponto = Alvos.Antecipar(new Vector2(7f, 4.5f), 1f, alvo, Reta); // tiro bem lento

        Assert.That(float.IsNaN(ponto.x) || float.IsNaN(ponto.y), Is.False);
        Assert.That(Vector2.Distance(ponto, Reta.Posicao(Reta.Comprimento)), Is.LessThan(1e-3f));
    }
}
