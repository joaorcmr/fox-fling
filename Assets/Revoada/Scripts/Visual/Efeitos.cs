using System.Collections.Generic;
using UnityEngine;

// Efeitos de partícula e de tela. Cada efeito é UM ParticleSystem do Unity que
// emite onde for pedido (Emit com posição e cor), em vez de um objeto novo por
// estouro: uma revoada de 80 pardais custa um sistema só.
public class Efeitos : MonoBehaviour
{
    public ParticleSystem penas;
    public ParticleSystem poeira;
    public ParticleSystem brilho;
    public SpriteRenderer prefabLama;
    public TextoFlutuante prefabTexto;
    public float duracaoLama = 0.9f;

    struct Mancha { public SpriteRenderer sprite; public float inicio; public Color cor; }

    readonly List<Mancha> manchas = new List<Mancha>();
    readonly Stack<SpriteRenderer> manchasLivres = new Stack<SpriteRenderer>();
    Pool<TextoFlutuante> textos;

    void Awake() => textos = new Pool<TextoFlutuante>(prefabTexto, transform);

    public void Penas(Vector2 posicao, Color cor, int quantidade)
    {
        Emitir(penas, posicao, cor, quantidade);
    }

    public void Poeira(Vector2 posicao) => Emitir(poeira, posicao, new Color(0.93f, 0.88f, 0.78f, 0.85f), 8);

    public void Brilho(Vector2 posicao) => Emitir(brilho, posicao, new Color(1f, 0.97f, 0.75f, 1f), 7);

    // Mancha de lama no chão, do tamanho da explosão, que some aos poucos.
    public void Lama(Vector2 posicao, float raio)
    {
        var s = manchasLivres.Count > 0 ? manchasLivres.Pop() : Instantiate(prefabLama, transform);
        s.gameObject.SetActive(true);
        s.transform.position = posicao;
        float largura = s.sprite != null ? s.sprite.bounds.size.x : 1.5f;
        s.transform.localScale = Vector3.one * (raio * 2f / largura);
        s.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        manchas.Add(new Mancha { sprite = s, inicio = Time.time, cor = Color.white });
        Emitir(poeira, posicao, new Color(0.55f, 0.38f, 0.22f, 0.9f), 10);
    }

    public void Texto(Vector2 posicao, string conteudo, Color cor)
    {
        var t = textos.Pegar();
        t.Mostrar(posicao, conteudo, cor, textos.Devolver);
    }

    void Update()
    {
        for (int i = manchas.Count - 1; i >= 0; i--)
        {
            var m = manchas[i];
            float t = (Time.time - m.inicio) / duracaoLama;
            if (t >= 1f)
            {
                m.sprite.gameObject.SetActive(false);
                manchasLivres.Push(m.sprite);
                manchas.RemoveAt(i);
                continue;
            }
            m.sprite.color = new Color(1f, 1f, 1f, t < 0.5f ? 1f : 1f - (t - 0.5f) / 0.5f);
        }
    }

    static void Emitir(ParticleSystem sistema, Vector2 posicao, Color cor, int quantidade)
    {
        if (sistema == null || quantidade <= 0) return;
        var p = new ParticleSystem.EmitParams
        {
            position = posicao,
            startColor = cor,
            applyShapeToPosition = true, // espalha em volta do ponto, pelo Shape do sistema
        };
        sistema.Emit(p, quantidade);
    }
}
