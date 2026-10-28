using UnityEngine;

// "+500" que sobe e some no lugar onde algo quebrou.
public class TextoFlutuante : MonoBehaviour
{
    public TextMesh texto, sombra;
    public float duracao = 1.1f;
    public float subida = 0.9f;

    float inicio;
    Vector3 origem;
    Color cor;

    public void Mostrar(string conteudo, Color novaCor, float escala)
    {
        texto.text = conteudo;
        sombra.text = conteudo;
        cor = novaCor;
        transform.localScale *= escala;
        origem = transform.position;
        inicio = Time.time;
    }

    void Update()
    {
        float t = (Time.time - inicio) / duracao;
        if (t >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        // Sobe rápido no começo e desacelera (curva "ease out").
        float sobe = 1f - (1f - t) * (1f - t);
        transform.position = origem + Vector3.up * subida * sobe;

        float alfa = t < 0.6f ? 1f : 1f - (t - 0.6f) / 0.4f;
        texto.color = new Color(cor.r, cor.g, cor.b, alfa);
        sombra.color = new Color(0.17f, 0.11f, 0.08f, alfa * 0.8f);
    }
}
