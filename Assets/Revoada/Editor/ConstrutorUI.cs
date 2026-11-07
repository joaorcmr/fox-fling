using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static ConstrutorAssets;

// Monta a interface (uGUI) por código. Convenção (a mesma do Fox Fling): o
// pivô de cada elemento é igual à sua âncora, então "posicao" é a distância da
// borda ou canto ancorado (ex.: canto superior esquerdo + (10, −10)).
public static class ConstrutorUI
{
    public const float LarguraBarra = 152f; // a barra lateral cobre x > 14 no mundo (14 × 32 px = 448 px)

    public static readonly Color Creme = new Color(1f, 0.965f, 0.88f);
    public static readonly Color Marrom = new Color(0.17f, 0.11f, 0.08f);

    public static readonly Vector2 TopoEsq = new Vector2(0f, 1f), Topo = new Vector2(0.5f, 1f), TopoDir = new Vector2(1f, 1f),
        Centro = new Vector2(0.5f, 0.5f), BaixoEsq = new Vector2(0f, 0f), Baixo = new Vector2(0.5f, 0f), BaixoDir = new Vector2(1f, 0f),
        Esq = new Vector2(0f, 0.5f), Dir = new Vector2(1f, 0.5f);

    static Font Fonte => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    public static Canvas Canvas(Transform pai, string nome, int ordem)
    {
        var go = new GameObject(nome, typeof(RectTransform));
        go.transform.SetParent(pai, false);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = ordem;
        // Desenhado para 600×480 (a resolução da disciplina); em outra resolução tudo escala junto.
        var escala = go.AddComponent<CanvasScaler>();
        escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escala.referenceResolution = new Vector2(ConstrutorRevoada.Largura, ConstrutorRevoada.Altura);
        escala.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    public static void SistemaDeEventos(Transform pai)
    {
        var go = new GameObject("EventSystem");
        go.transform.SetParent(pai, false);
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();
    }

    public static RectTransform Retangulo(Transform pai, string nome, Vector2 ancora, Vector2 posicao, Vector2 tamanho)
    {
        var go = new GameObject(nome, typeof(RectTransform));
        go.transform.SetParent(pai, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = ancora;
        rt.anchoredPosition = posicao;
        rt.sizeDelta = tamanho;
        return rt;
    }

    public static RectTransform Esticar(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return rt;
    }

    public static Text Texto(Transform pai, string nome, string conteudo, int tamanho, TextAnchor alinhamento,
        Vector2 ancora, Vector2 posicao, Vector2 caixa, Color cor, bool contorno = false, bool negrito = true)
    {
        var t = Retangulo(pai, nome, ancora, posicao, caixa).gameObject.AddComponent<Text>();
        t.font = Fonte;
        t.text = conteudo;
        t.fontSize = Mathf.Max(12, tamanho); // nada menor que 12 px (legível em 600×480)
        t.fontStyle = negrito ? FontStyle.Bold : FontStyle.Normal;
        t.alignment = alinhamento;
        t.color = cor;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        if (contorno)
        {
            var o = t.gameObject.AddComponent<Outline>();
            o.effectColor = Marrom;
            o.effectDistance = new Vector2(1.2f, -1.2f);
        }
        return t;
    }

    public static Image Imagem(Transform pai, string nome, Sprite sprite, Vector2 ancora, Vector2 posicao, Vector2 tamanho, bool fatiada = false)
    {
        var img = Retangulo(pai, nome, ancora, posicao, tamanho).gameObject.AddComponent<Image>();
        img.sprite = sprite;
        if (fatiada) img.type = Image.Type.Sliced;
        return img;
    }

    public static Button Botao(Transform pai, string nome, Sprite icone, Vector2 ancora, Vector2 posicao, Vector2 tamanho)
    {
        var fundo = Imagem(pai, nome, Sprite("botao"), ancora, posicao, tamanho, true);
        var botao = fundo.gameObject.AddComponent<Button>();
        botao.targetGraphic = fundo;
        var cores = botao.colors;
        cores.highlightedColor = new Color(1f, 1f, 0.85f);
        cores.pressedColor = new Color(0.75f, 0.85f, 0.7f);
        cores.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.75f);
        botao.colors = cores;
        if (icone != null)
        {
            var img = Imagem(fundo.transform, "Icone", icone, Centro, new Vector2(0f, 1f), new Vector2(tamanho.y, tamanho.y) * 0.66f);
            img.raycastTarget = false;
        }
        return botao;
    }

    // ------------------------------------------------------------------
    // HUD do jogo: a barra lateral
    // ------------------------------------------------------------------
    public static Hud CriarHud(Transform pai, Jogo jogo, Controles controles, TipoPorco[] porcos)
    {
        var canvas = Canvas(pai, "HUD", 10);
        var hud = canvas.gameObject.AddComponent<Hud>();
        hud.jogo = jogo;
        hud.controles = controles;

        // O fundo da barra bloqueia os cliques: o que cai nele não chega ao mapa.
        var barra = Imagem(canvas.transform, "BarraLateral", Sprite("painel"), TopoDir, Vector2.zero,
            new Vector2(LarguraBarra, ConstrutorRevoada.Altura), true).transform;

        Imagem(barra, "IconeVida", Sprite("icone_vida"), TopoEsq, new Vector2(12f, -10f), new Vector2(26f, 26f)).raycastTarget = false;
        hud.textoVida = Texto(barra, "Vida", "100", 22, TextAnchor.MiddleLeft, TopoEsq, new Vector2(44f, -10f), new Vector2(96f, 26f), Marrom);
        Imagem(barra, "IconePena", Sprite("icone_pena"), TopoEsq, new Vector2(12f, -40f), new Vector2(26f, 26f)).raycastTarget = false;
        hud.textoPenas = Texto(barra, "Penas", "300", 22, TextAnchor.MiddleLeft, TopoEsq, new Vector2(44f, -40f), new Vector2(96f, 26f), Marrom);
        hud.textoOnda = Texto(barra, "Onda", "Onda 1/30", 14, TextAnchor.MiddleLeft, TopoEsq, new Vector2(12f, -70f), new Vector2(128f, 20f), Marrom);

        Texto(barra, "TituloLoja", "Porcos", 14, TextAnchor.MiddleLeft, TopoEsq, new Vector2(12f, -94f), new Vector2(128f, 18f), Marrom);
        hud.cartoes = new CartaoPorcoLoja[porcos.Length];
        for (int i = 0; i < porcos.Length; i++)
        {
            var posicao = new Vector2(10f + (i % 2) * 68f, -114f - (i / 2) * 72f);
            hud.cartoes[i] = CriarCartao(barra, porcos[i], posicao, new Vector2(64f, 68f));
        }

        hud.botaoSoltar = Botao(barra, "BotaoSoltar", null, BaixoEsq, new Vector2(8f, 10f), new Vector2(136f, 44f));
        Imagem(hud.botaoSoltar.transform, "Icone", Sprite("icone_play"), Esq, new Vector2(8f, 1f), new Vector2(26f, 26f)).raycastTarget = false;
        hud.textoSoltar = Texto(hud.botaoSoltar.transform, "Texto", "Soltar a revoada", 13, TextAnchor.MiddleCenter, Esq,
            new Vector2(36f, 1f), new Vector2(96f, 40f), Creme, contorno: true);

        SistemaDeEventos(pai);
        return hud;
    }

    static CartaoPorcoLoja CriarCartao(Transform pai, TipoPorco tipo, Vector2 posicao, Vector2 tamanho)
    {
        var fundo = Imagem(pai, "Cartao" + tipo.dados.especie, Sprite("botao"), TopoEsq, posicao, tamanho, true);
        var cartao = fundo.gameObject.AddComponent<CartaoPorcoLoja>();
        cartao.tipo = tipo;
        cartao.fundo = fundo;
        cartao.botao = fundo.gameObject.AddComponent<Button>();
        cartao.botao.targetGraphic = fundo;
        cartao.grupo = fundo.gameObject.AddComponent<CanvasGroup>();

        cartao.icone = Imagem(fundo.transform, "Icone", tipo.corpo, Topo, new Vector2(0f, -2f), new Vector2(46f, 46f));
        cartao.icone.raycastTarget = false;
        cartao.icone.preserveAspect = true;
        Imagem(fundo.transform, "IconePena", Sprite("icone_pena"), Baixo, new Vector2(-17f, 4f), new Vector2(16f, 16f)).raycastTarget = false;
        cartao.preco = Texto(fundo.transform, "Preco", tipo.dados.custo.ToString(), 13, TextAnchor.MiddleLeft, Baixo,
            new Vector2(8f, 3f), new Vector2(36f, 18f), Creme, contorno: true);
        return cartao;
    }
}
