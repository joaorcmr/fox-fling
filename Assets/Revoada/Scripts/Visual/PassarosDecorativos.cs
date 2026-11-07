using UnityEngine;

// Pássaros que voam pela trilha no fundo do menu, só de enfeite (não há
// simulação nem porcos aqui): dão vida à tela de título.
public class PassarosDecorativos : MonoBehaviour
{
    public Mapa mapa;
    public Sprite[] sprites;
    public Sprite sombra;
    public int quantidade = 8;
    public float velocidade = 1.6f;

    Trilha trilha;
    Transform[] corpos;
    SpriteRenderer[] renderizadores;
    Transform[] figuras;
    float[] distancias;

    void Start()
    {
        trilha = new Trilha(mapa.dados.pontos, mapa.dados.raioCanto);
        corpos = new Transform[quantidade];
        figuras = new Transform[quantidade];
        renderizadores = new SpriteRenderer[quantidade];
        distancias = new float[quantidade];
        for (int i = 0; i < quantidade; i++)
        {
            var raiz = new GameObject("Passaro" + i).transform;
            raiz.SetParent(transform, false);
            var s = new GameObject("Sombra").AddComponent<SpriteRenderer>();
            s.transform.SetParent(raiz, false);
            s.sprite = sombra;
            s.sortingOrder = -3;
            s.transform.localPosition = new Vector3(0f, -0.3f, 0f);
            s.transform.localScale = Vector3.one * 0.6f;
            var r = new GameObject("Corpo").AddComponent<SpriteRenderer>();
            r.transform.SetParent(raiz, false);
            r.sprite = sprites[i % sprites.Length];
            r.sortingOrder = 0;
            corpos[i] = raiz;
            figuras[i] = r.transform;
            renderizadores[i] = r;
            distancias[i] = trilha.Comprimento * i / quantidade;
        }
    }

    void Update()
    {
        for (int i = 0; i < quantidade; i++)
        {
            distancias[i] = (distancias[i] + velocidade * Time.unscaledDeltaTime) % trilha.Comprimento;
            corpos[i].position = trilha.Posicao(distancias[i]);
            figuras[i].localPosition = new Vector3(0f, 0.06f + Mathf.Abs(Mathf.Sin(Time.unscaledTime * 9f + i)) * 0.12f, 0f);
            renderizadores[i].flipX = trilha.Direcao(distancias[i]).x < -0.1f;
        }
    }
}
