using System;
using UnityEngine;

// "+3" que sobe e some onde um pássaro estourou. TextMesh com sombra, como no
// Fox Fling; quando termina, avisa o Efeitos para voltar ao pool.
public class TextoFlutuante : MonoBehaviour
{
    public TextMesh texto;
    public TextMesh sombra;
    public float duracao = 0.9f;
    public float subida = 0.7f;

    float inicio;
    Vector3 origem;
    Color cor;
    Action<TextoFlutuante> aoTerminar;

    public void Mostrar(Vector2 posicao, string conteudo, Color novaCor, Action<TextoFlutuante> fim)
    {
        texto.text = sombra.text = conteudo;
        cor = novaCor;
        origem = new Vector3(posicao.x, posicao.y + 0.3f, 0f);
        transform.position = origem;
        inicio = Time.time;
        aoTerminar = fim;
        Pintar(1f);
    }

    void Update()
    {
        float t = (Time.time - inicio) / duracao;
        if (t >= 1f)
        {
            aoTerminar?.Invoke(this);
            return;
        }
        float sobe = 1f - (1f - t) * (1f - t); // sobe rápido e desacelera ("ease out")
        transform.position = origem + Vector3.up * subida * sobe;
        Pintar(t < 0.6f ? 1f : 1f - (t - 0.6f) / 0.4f);
    }

    void Pintar(float alfa)
    {
        texto.color = new Color(cor.r, cor.g, cor.b, alfa);
        sombra.color = new Color(0.17f, 0.11f, 0.08f, alfa * 0.8f);
    }
}
