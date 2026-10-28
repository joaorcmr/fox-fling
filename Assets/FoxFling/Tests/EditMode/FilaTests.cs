using NUnit.Framework;
using UnityEngine;

// A fila de raposas precisa caber na tela: o jogador conta quantas ainda tem.
public class FilaTests
{
    const float RaioRaposa = 0.33f;
    const float BordaEsquerda = -7.5f;
    const float XEstilingue = -5.5f;

    [Test]
    public void TresRaposasEsperandoCabemNaTela()
    {
        var go = new GameObject("teste fila");
        try
        {
            var fase = go.AddComponent<GerenciadorFase>();
            for (int i = 0; i < 3; i++)
            {
                float x = fase.inicioFila.x - fase.espacoFila * i;
                Assert.GreaterOrEqual(x, BordaEsquerda + 0.15f, $"raposa {i} da fila sai da tela (x={x})");
            }
            // A primeira não pode ficar encostada no estilingue.
            Assert.LessOrEqual(fase.inicioFila.x + RaioRaposa, XEstilingue - 0.2f);
        }
        finally
        {
            Object.DestroyImmediate(go);
        }
    }
}
