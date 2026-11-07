using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

// A mira das armas (issue #7): sempre na raposa mais próxima, e só dentro do alcance.
public class ArmasTests
{
    static readonly Vector2 Origem = new Vector2(0f, 0f);

    [Test]
    public void EscolherAlvo_SemRaposasNaoHaAlvo()
    {
        Assert.AreEqual(-1, Regras.EscolherAlvo(Origem, new List<Vector2>(), 5f));
    }

    [Test]
    public void EscolherAlvo_EscolheAMaisProxima()
    {
        var posicoes = new List<Vector2> { new Vector2(4f, 0f), new Vector2(-2f, 0f), new Vector2(3f, 0f) };
        Assert.AreEqual(1, Regras.EscolherAlvo(Origem, posicoes, 5f));
    }

    [Test]
    public void EscolherAlvo_IgnoraQuemEstaForaDoAlcance()
    {
        var posicoes = new List<Vector2> { new Vector2(6f, 0f), new Vector2(0f, 7f) };
        Assert.AreEqual(-1, Regras.EscolherAlvo(Origem, posicoes, 5f));
    }

    [Test]
    public void EscolherAlvo_OAlcanceEMedidoEmLinhaReta()
    {
        // (3,3) está a 4,24: dentro de 5, mas fora de 4.
        var posicoes = new List<Vector2> { new Vector2(3f, 3f) };
        Assert.AreEqual(0, Regras.EscolherAlvo(Origem, posicoes, 5f));
        Assert.AreEqual(-1, Regras.EscolherAlvo(Origem, posicoes, 4f));
    }

    [Test]
    public void EscolherAlvo_UmaRaposaForaDoAlcanceNaoImpedeEscolherOutraDentro()
    {
        var posicoes = new List<Vector2> { new Vector2(9f, 0f), new Vector2(4.5f, 0f) };
        Assert.AreEqual(1, Regras.EscolherAlvo(Origem, posicoes, 5f));
    }
}
