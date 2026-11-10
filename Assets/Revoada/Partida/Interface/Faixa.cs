using UnityEngine;
using UnityEngine.UI;

// A faixa que desce no alto do mapa anunciando "Onda 7" ou o chefe. Usa o
// tempo sem escala (unscaledTime): anima igual com o jogo em 1×, 3× ou pausado.
public class Faixa : MonoBehaviour
{
    public RectTransform caixa;
    public Image fundo;
    public Text texto;
    public CanvasGroup grupo;

    Vector2 posicaoBase;
    float inicio = -100f, duracao;

    void Awake()
    {
        posicaoBase = caixa.anchoredPosition;
        grupo.alpha = 0f;
    }

    public void Mostrar(string conteudo, Color cor, float novaDuracao)
    {
        texto.text = conteudo;
        fundo.color = cor;
        duracao = novaDuracao;
        inicio = Time.unscaledTime;
    }

    void Update()
    {
        float t = Time.unscaledTime - inicio;
        if (t > duracao)
        {
            grupo.alpha = 0f;
            return;
        }
        float entrada = Mathf.Clamp01(t / 0.25f);
        float saida = Mathf.Clamp01((duracao - t) / 0.35f);
        grupo.alpha = Mathf.Min(entrada, saida);
        float desce = 1f - (1f - entrada) * (1f - entrada);
        caixa.anchoredPosition = posicaoBase + Vector2.up * (1f - desce) * 50f;
    }
}
