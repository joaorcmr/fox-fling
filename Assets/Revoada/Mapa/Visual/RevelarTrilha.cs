using System.Collections.Generic;
using UnityEngine;

// Mostra o caminho do mapa novo logo depois de uma troca de fase: uma faixa
// dourada que se desenha da entrada até a Torre, setas andando no sentido em
// que os pássaros vão voar e um anel pulsando na entrada e na Torre. É o "por
// onde eles vêm agora" que o jogador precisa ver antes de colocar os porcos de
// novo. Só desenho, em tempo sem escala (anda igual em 1×, 3× ou pausado);
// quem decide a hora é a TrocaDeFase.
public class RevelarTrilha : MonoBehaviour
{
    static readonly Color Cor = new Color(1f, 0.86f, 0.35f);

    public Sprite seta;
    public Sprite anel;
    [Tooltip("Material de sprite (Sprites/Default) para a faixa.")]
    public Material material;
    public float duracao = 5f;            // quanto tempo tudo fica na tela
    public float duracaoDesenho = 1.4f;   // a faixa indo da entrada até a Torre
    public float espacoSetas = 1.6f;
    public float velocidadeSetas = 2.6f;
    public float tamanhoSeta = 0.5f;      // em unidades do mundo
    public float tamanhoAnel = 1.3f;

    LineRenderer faixa;
    SpriteRenderer anelEntrada, anelTorre;
    SpriteRenderer[] aneis; // anelEntrada e anelTorre juntos, para o Update não alocar a cada quadro
    readonly List<SpriteRenderer> setas = new List<SpriteRenderer>();
    readonly List<Vector3> pontos = new List<Vector3>();
    Trilha trilha;
    float periodoSetas;
    int quantasSetas;
    float inicio = -100f;

    public bool Mostrando => Time.unscaledTime - inicio < duracao;

    public void Revelar(Trilha nova, DadosMapa mapa)
    {
        Garantir();
        trilha = nova;
        inicio = Time.unscaledTime;

        pontos.Clear();
        for (float d = 0f; d < nova.Comprimento; d += 0.2f) pontos.Add(nova.Posicao(d));
        pontos.Add(nova.Posicao(nova.Comprimento));

        anelEntrada.transform.position = Entrada(nova, mapa);
        anelTorre.transform.position = mapa.torre;

        quantasSetas = Mathf.Max(1, Mathf.FloorToInt(nova.Comprimento / espacoSetas));
        periodoSetas = quantasSetas * espacoSetas;
        while (setas.Count < quantasSetas) setas.Add(NovoSprite("Seta", seta, 30));
        float escalaSeta = seta != null ? tamanhoSeta / seta.bounds.size.x : 1f;
        for (int i = 0; i < setas.Count; i++)
        {
            setas[i].gameObject.SetActive(i < quantasSetas);
            setas[i].transform.localScale = Vector3.one * escalaSeta;
        }
        faixa.enabled = true;
        anelEntrada.gameObject.SetActive(true);
        anelTorre.gameObject.SetActive(true);
        Update();
    }

    // O primeiro ponto da trilha costuma ficar fora do mapa (os pássaros entram
    // pela borda): a entrada é onde ela passa a ser visível.
    static Vector2 Entrada(Trilha t, DadosMapa mapa)
    {
        const float margem = 0.5f;
        for (float d = 0f; d < t.Comprimento; d += 0.1f)
        {
            var p = t.Posicao(d);
            if (p.x >= margem && p.y >= margem && p.x <= mapa.tamanho.x - margem && p.y <= mapa.tamanho.y - margem) return p;
        }
        return t.Posicao(0f);
    }

    void Update()
    {
        if (faixa == null || trilha == null) return;
        float t = Time.unscaledTime - inicio;
        if (t > duracao)
        {
            Esconder();
            return;
        }
        float sumir = Mathf.Clamp01((duracao - t) / 0.6f);

        // A faixa cresce da entrada até a Torre ("ease out": rápida no começo).
        float k = Mathf.Clamp01(t / duracaoDesenho);
        k = 1f - (1f - k) * (1f - k);
        int quantos = Mathf.Clamp(Mathf.CeilToInt(k * (pontos.Count - 1)) + 1, 2, pontos.Count);
        faixa.positionCount = quantos;
        for (int i = 0; i < quantos; i++) faixa.SetPosition(i, pontos[i]);
        faixa.startColor = faixa.endColor = new Color(Cor.r, Cor.g, Cor.b, 0.8f * sumir);

        // As setas andam no sentido dos pássaros, só na parte já desenhada.
        float desenhado = k * trilha.Comprimento;
        float aparecer = Mathf.Clamp01((t - 0.3f) / 0.4f) * sumir;
        for (int i = 0; i < quantasSetas; i++)
        {
            var s = setas[i];
            float d = Mathf.Repeat(t * velocidadeSetas + i * espacoSetas, periodoSetas);
            float pontas = Mathf.Clamp01(d / 0.8f) * Mathf.Clamp01((trilha.Comprimento - d) / 0.8f);
            float alfa = d <= desenhado ? aparecer * pontas : 0f;
            s.color = new Color(1f, 1f, 1f, alfa);
            s.transform.position = trilha.Posicao(d);
            var dir = trilha.Direcao(d);
            s.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        }

        // Anéis pulsando (1 por segundo) na entrada e na Torre.
        float pulso = Mathf.Repeat(t, 1f);
        float escalaAnel = anel != null ? tamanhoAnel / anel.bounds.size.x : 1f;
        var corAnel = new Color(Cor.r, Cor.g, Cor.b, (1f - pulso) * sumir);
        foreach (var a in aneis)
        {
            a.transform.localScale = Vector3.one * escalaAnel * (1f + 0.7f * pulso);
            a.color = corAnel;
        }
    }

    void Esconder()
    {
        if (faixa != null) faixa.enabled = false;
        if (anelEntrada != null) anelEntrada.gameObject.SetActive(false);
        if (anelTorre != null) anelTorre.gameObject.SetActive(false);
        foreach (var s in setas) s.gameObject.SetActive(false);
        trilha = null;
    }

    void Garantir()
    {
        if (faixa != null) return;
        var go = new GameObject("Faixa");
        go.transform.SetParent(transform, false);
        faixa = go.AddComponent<LineRenderer>();
        faixa.useWorldSpace = true;
        faixa.sharedMaterial = material;
        faixa.widthMultiplier = 0.32f;
        faixa.numCapVertices = 4;
        faixa.numCornerVertices = 4;
        faixa.sortingOrder = -3; // por cima da terra da trilha, por baixo de árvores e bichos
        faixa.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        faixa.receiveShadows = false;
        anelEntrada = NovoSprite("AnelEntrada", anel, 29);
        anelTorre = NovoSprite("AnelTorre", anel, 29);
        aneis = new[] { anelEntrada, anelTorre };
    }

    SpriteRenderer NovoSprite(string nome, Sprite sprite, int ordem)
    {
        var go = new GameObject(nome);
        go.transform.SetParent(transform, false);
        var r = go.AddComponent<SpriteRenderer>();
        r.sprite = sprite;
        r.sortingOrder = ordem;
        go.SetActive(false);
        return r;
    }
}
