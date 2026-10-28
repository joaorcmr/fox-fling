using System.Linq;
using NUnit.Framework;

// Confere os mapas antes de virar cena: raposas certas, peças apoiadas
// (nada flutuando nem enfiado em outra peça) e tudo dentro da área de jogo.
public class DefinicoesFasesTests
{
    const float Tol = 0.01f;

    static readonly TipoRaposa N = TipoRaposa.Normal, Ni = TipoRaposa.Ninja, B = TipoRaposa.Bomba;

    [Test]
    public void RaposasBatemComOQuadro()
    {
        // Quadro do Estágio 2: 1 fácil, 2 médios, 1 difícil e 1 desafio.
        var fases = DefinicoesFases.Todas();
        Assert.AreEqual(5, fases.Length);
        CollectionAssert.AreEqual(new[] { N, N }, fases[0].raposas, "Fase 1");
        CollectionAssert.AreEqual(new[] { N, N, Ni }, fases[1].raposas, "Fase 2");
        CollectionAssert.AreEqual(new[] { N, N, Ni }, fases[2].raposas, "Fase 3");
        CollectionAssert.AreEqual(new[] { N, N, Ni, B }, fases[3].raposas, "Fase 4");
        CollectionAssert.AreEqual(new[] { N, Ni, B }, fases[4].raposas, "Fase 5");
    }

    [Test]
    public void DificuldadesBatemComOQuadro()
    {
        var dificuldades = DefinicoesFases.Todas().Select(f => f.dificuldade).ToArray();
        CollectionAssert.AreEqual(new[] { "Fácil", "Médio", "Médio", "Difícil", "Desafio" }, dificuldades);
    }

    [Test]
    public void FasesDificeisUsamTodosOsMateriais()
    {
        var fases = DefinicoesFases.Todas();
        foreach (var f in new[] { fases[3], fases[4] })
            foreach (TipoMaterial m in System.Enum.GetValues(typeof(TipoMaterial)))
                Assert.IsTrue(f.pecas.Any(p => p.tipo == TipoPeca.Bloco && p.material == m), $"{f.nome} sem {m}");
    }

    [Test]
    public void FasesNumeradasEmOrdem()
    {
        var fases = DefinicoesFases.Todas();
        for (int i = 0; i < fases.Length; i++)
            Assert.AreEqual(i + 1, fases[i].numero);
    }

    [Test]
    public void TodaFaseTemInimigo()
    {
        foreach (var f in DefinicoesFases.Todas())
            Assert.IsTrue(f.pecas.Any(p => p.tipo != TipoPeca.Bloco), f.nome);
    }

    [Test]
    public void NadaFlutuaNemSobrepoe()
    {
        foreach (var f in DefinicoesFases.Todas())
        {
            var pecas = f.pecas;
            for (int i = 0; i < pecas.Count; i++)
            {
                var a = pecas[i];

                for (int j = i + 1; j < pecas.Count; j++)
                {
                    var b = pecas[j];
                    float sx = System.Math.Min(a.Direita, b.Direita) - System.Math.Max(a.Esquerda, b.Esquerda);
                    float sy = System.Math.Min(a.Topo, b.Topo) - System.Math.Max(a.Base, b.Base);
                    Assert.IsFalse(sx > Tol && sy > Tol, $"{f.nome}: peças {i} e {j} se sobrepõem");
                }

                if (System.Math.Abs(a.Base - DefinicoesFases.Chao) < Tol) continue;

                // Apoios: blocos cujo topo encosta na base desta peça (triângulo não serve de apoio).
                var apoios = pecas.Where(b => b.tipo == TipoPeca.Bloco
                                              && System.Math.Abs(b.Topo - a.Base) < Tol
                                              && System.Math.Min(a.Direita, b.Direita) - System.Math.Max(a.Esquerda, b.Esquerda) > 0.05f).ToList();
                Assert.IsTrue(apoios.Count > 0, $"{f.nome}: peça {i} em {a.centro} está flutuando");

                // O centro precisa estar sobre os apoios, senão a peça tomba sozinha.
                float min = apoios.Min(b => b.Esquerda), max = apoios.Max(b => b.Direita);
                Assert.IsTrue(a.centro.x >= min - Tol && a.centro.x <= max + Tol, $"{f.nome}: peça {i} em {a.centro} não está equilibrada");
            }
        }
    }

    [Test]
    public void PecasDentroDaArea()
    {
        foreach (var f in DefinicoesFases.Todas())
            foreach (var p in f.pecas)
            {
                Assert.GreaterOrEqual(p.Esquerda, 0f, $"{f.nome}: {p.centro}");
                Assert.LessOrEqual(p.Direita, 7.4f, $"{f.nome}: {p.centro}");
                Assert.GreaterOrEqual(p.Base, DefinicoesFases.Chao - Tol, $"{f.nome}: {p.centro}");
                Assert.LessOrEqual(p.Topo, 5.5f, $"{f.nome}: {p.centro}");
            }
    }
}
