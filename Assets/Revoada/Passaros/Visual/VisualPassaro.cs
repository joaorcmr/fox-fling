using UnityEngine;

// O desenho de um pássaro (prefab Passaro). Lê o Passaro da simulação a cada
// quadro: posição interpolada entre dois passos, balanço de voo, para que lado
// está virado, flash ao levar golpe e barra de vida dos mais fortes.
public class VisualPassaro : MonoBehaviour
{
    public const float LarguraBarra = 0.7f;
    const float TamanhoSpriteBarra = 0.125f; // o sprite "barra" tem 8 px a 64 px por unidade

    public SpriteRenderer corpo;
    public SpriteRenderer flash;
    public SpriteRenderer sombra;
    public Transform barra;
    public SpriteRenderer barraVida;

    [Header("Estado ao vivo (só leitura, para ver no Inspector durante o Play)")]
    public EspeciePassaro especie;
    public float vida;
    public float vidaMaxima;
    public float distancia;

    public Passaro Passaro { get; private set; }

    float fase, fimFlash;
    bool viradoEsquerda;

    public void Iniciar(Passaro p, TipoPassaro tipo)
    {
        Passaro = p;
        corpo.sprite = flash.sprite = tipo.sprite;
        flash.color = new Color(1f, 1f, 1f, 0f);
        fimFlash = 0f;
        fase = p.Id * 2.39996f; // cada um bate as asas num tempo diferente
        transform.localScale = Vector3.one * tipo.tamanho;

        var tamanho = tipo.sprite != null ? tipo.sprite.bounds.size : new Vector3(0.8f, 0.8f, 0f);
        sombra.transform.localPosition = new Vector3(0f, -tamanho.y * 0.42f, 0f);
        sombra.transform.localScale = Vector3.one * (tamanho.x * 0.8f);
        barra.localPosition = new Vector3(0f, tamanho.y * 0.5f + 0.22f, 0f);
        barra.gameObject.SetActive(p.VidaMaxima >= 8f);

        Atualizar(1f);
    }

    public void Atualizar(float alfa)
    {
        var p = Passaro;
        if (p == null) return;

        transform.position = Vector2.Lerp(p.PosicaoAnterior, p.Posicao, alfa);

        // Voo: o corpo sobe e desce e estica um pouco; a sombra fica no chão.
        float t = Time.time * 9f + fase;
        corpo.transform.localPosition = new Vector3(0f, 0.06f + Mathf.Abs(Mathf.Sin(t)) * 0.12f, 0f);
        float estica = 1f + Mathf.Sin(t * 2f) * 0.04f;
        corpo.transform.localScale = new Vector3(1f / estica, estica, 1f);

        float dx = p.Posicao.x - p.PosicaoAnterior.x;
        if (Mathf.Abs(dx) > 1e-4f) viradoEsquerda = dx < 0f;
        corpo.flipX = flash.flipX = viradoEsquerda;

        if (fimFlash > 0f)
        {
            float k = Mathf.Clamp01((fimFlash - Time.time) / 0.1f);
            flash.color = new Color(1f, 1f, 1f, k * 0.85f);
            if (k <= 0f) fimFlash = 0f;
        }

        if (barra.gameObject.activeSelf)
        {
            float f = Mathf.Clamp01(p.Vida / p.VidaMaxima);
            barraVida.transform.localScale = new Vector3(LarguraBarra * f / TamanhoSpriteBarra, 0.08f / TamanhoSpriteBarra, 1f);
            barraVida.transform.localPosition = new Vector3(-LarguraBarra / 2f + LarguraBarra * f / 2f, 0f, 0f);
        }

        especie = p.Especie;
        vida = p.Vida;
        vidaMaxima = p.VidaMaxima;
        distancia = p.Distancia;
    }

    public void Piscar() => fimFlash = Time.time + 0.1f;
}
