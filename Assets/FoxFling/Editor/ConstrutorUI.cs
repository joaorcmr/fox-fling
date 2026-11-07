using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static ConstrutorAssets;

// Monta a interface (uGUI) por código: HUD da fase e tela do menu.
// Convenção: o pivô de cada elemento é igual à sua âncora, então "posicao" é a
// distância da borda/canto ancorado (ex.: canto superior esquerdo + (10, -10)).
public static class ConstrutorUI
{
    public static readonly Color Creme = new Color(1f, 0.965f, 0.88f);
    public static readonly Color Marrom = new Color(0.17f, 0.11f, 0.08f);

    public static readonly Vector2 TopoEsq = new Vector2(0f, 1f), Topo = new Vector2(0.5f, 1f), TopoDir = new Vector2(1f, 1f),
        Centro = new Vector2(0.5f, 0.5f), BaixoEsq = new Vector2(0f, 0f), Baixo = new Vector2(0.5f, 0f), BaixoDir = new Vector2(1f, 0f);

    static Font Fonte => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    public static Canvas Canvas(Transform pai, string nome, int ordem)
    {
        var go = new GameObject(nome, typeof(RectTransform));
        go.transform.SetParent(pai, false);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = ordem;
        // Desenhado para 600×480; em outras resoluções tudo escala junto.
        var escala = go.AddComponent<CanvasScaler>();
        escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escala.referenceResolution = new Vector2(600f, 480f);
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
        Vector2 ancora, Vector2 posicao, Vector2 caixa, Color cor, bool contorno = true, bool negrito = true)
    {
        var t = Retangulo(pai, nome, ancora, posicao, caixa).gameObject.AddComponent<Text>();
        t.font = Fonte;
        t.text = conteudo;
        t.fontSize = tamanho;
        t.fontStyle = negrito ? FontStyle.Bold : FontStyle.Normal;
        t.alignment = alinhamento;
        t.color = cor;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        if (contorno)
        {
            var o = t.gameObject.AddComponent<Outline>();
            o.effectColor = Marrom;
            o.effectDistance = new Vector2(1.6f, -1.6f);
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
        cores.highlightedColor = new Color(1f, 0.93f, 0.8f);
        cores.pressedColor = new Color(0.8f, 0.68f, 0.55f);
        cores.disabledColor = new Color(0.75f, 0.75f, 0.75f, 0.7f);
        botao.colors = cores;
        if (icone != null)
        {
            var img = Imagem(fundo.transform, "Icone", icone, Centro, new Vector2(0f, 2f), tamanho * 0.62f);
            img.raycastTarget = false;
        }
        return botao;
    }

    public static Hud CriarHud(Transform pai)
    {
        var canvas = Canvas(pai, "HUD", 10);
        var t = canvas.transform;
        var hud = canvas.gameObject.AddComponent<Hud>();

        hud.botaoMenu = Botao(t, "BotaoMenu", Sprite("icone_menu"), TopoEsq, new Vector2(10f, -10f), new Vector2(40f, 40f));
        hud.botaoReiniciar = Botao(t, "BotaoReiniciar", Sprite("icone_reiniciar"), TopoEsq, new Vector2(56f, -10f), new Vector2(40f, 40f));
        hud.textoFase = Texto(t, "TextoFase", "Fase 1 · Fácil", 18, TextAnchor.MiddleLeft, TopoEsq, new Vector2(106f, -10f), new Vector2(260f, 40f), Creme);
        hud.textoPontos = Texto(t, "TextoPontos", "Pontos: 0", 20, TextAnchor.MiddleRight, TopoDir, new Vector2(-12f, -10f), new Vector2(240f, 40f), Creme);
        hud.textoDica = Texto(t, "TextoDica", "", 17, TextAnchor.MiddleCenter, Topo, new Vector2(0f, -60f), new Vector2(560f, 30f), Creme);

        // Tela de resultado: fundo escurecido (bloqueia cliques no jogo) + painel.
        var escuro = Imagem(t, "Resultado", null, Centro, Vector2.zero, Vector2.zero);
        Esticar(escuro.rectTransform);
        escuro.color = new Color(0f, 0f, 0f, 0.45f);
        hud.painelResultado = escuro.gameObject;

        var painel = Imagem(escuro.transform, "Painel", Sprite("painel"), Centro, Vector2.zero, new Vector2(360f, 270f), true).transform;
        hud.tituloResultado = Texto(painel, "Titulo", "Fase concluída!", 27, TextAnchor.MiddleCenter, Topo, new Vector2(0f, -22f), new Vector2(340f, 44f), Marrom, contorno: false);

        hud.estrelaCheia = Sprite("estrela");
        hud.estrelaVazia = Sprite("estrela_vazia");
        hud.estrelas = new[]
        {
            Imagem(painel, "Estrela1", hud.estrelaVazia, Centro, new Vector2(-84f, 26f), new Vector2(64f, 64f)),
            Imagem(painel, "Estrela2", hud.estrelaVazia, Centro, new Vector2(0f, 38f), new Vector2(80f, 80f)),
            Imagem(painel, "Estrela3", hud.estrelaVazia, Centro, new Vector2(84f, 26f), new Vector2(64f, 64f)),
        };
        hud.pontosResultado = Texto(painel, "Pontos", "0 pontos", 22, TextAnchor.MiddleCenter, Centro, new Vector2(0f, -36f), new Vector2(320f, 32f), Marrom, contorno: false);

        hud.botaoMenuResultado = Botao(painel, "BotaoMenu", Sprite("icone_menu"), Centro, new Vector2(-80f, -92f), new Vector2(60f, 60f));
        hud.botaoRepetir = Botao(painel, "BotaoRepetir", Sprite("icone_reiniciar"), Centro, new Vector2(0f, -92f), new Vector2(60f, 60f));
        hud.botaoProxima = Botao(painel, "BotaoProxima", Sprite("icone_proxima"), Centro, new Vector2(80f, -92f), new Vector2(60f, 60f));

        SistemaDeEventos(pai);
        return hud;
    }

    public static MenuPrincipal CriarMenu(Transform pai, DefinicaoFase[] fases)
    {
        var canvas = Canvas(pai, "Menu", 10);
        var t = canvas.transform;
        var menu = canvas.gameObject.AddComponent<MenuPrincipal>();

        Imagem(t, "Logo", Sprite("logo"), Topo, new Vector2(0f, -12f), new Vector2(432f, 104f)).raycastTarget = false;
        Texto(t, "Subtitulo", "Escolha uma fase", 20, TextAnchor.MiddleCenter, Topo, new Vector2(0f, -122f), new Vector2(400f, 30f), Creme);

        const float largura = 92f, espaco = 14f;
        float inicio = -(fases.Length * largura + (fases.Length - 1) * espaco) / 2f + largura / 2f;
        menu.cartoes = new CartaoFase[fases.Length];
        for (int i = 0; i < fases.Length; i++)
            menu.cartoes[i] = CriarCartao(t, fases[i], new Vector2(inicio + i * (largura + espaco), 12f), new Vector2(largura, 120f));

        Texto(t, "Instrucoes", "Defenda a torre das raposas  ·  R reinicia  ·  Esc volta ao menu", 13, TextAnchor.MiddleCenter,
            Baixo, new Vector2(0f, 70f), new Vector2(560f, 20f), Creme);
        Texto(t, "Creditos", "Joao Ricardo Ribeiro · Insper", 12, TextAnchor.MiddleLeft, BaixoEsq, new Vector2(12f, 12f), new Vector2(260f, 20f), Creme);

        menu.botaoZerar = Botao(t, "BotaoZerar", null, BaixoDir, new Vector2(-10f, 8f), new Vector2(130f, 28f));
        Texto(menu.botaoZerar.transform, "Texto", "Zerar progresso", 13, TextAnchor.MiddleCenter, Centro, new Vector2(0f, 1f), new Vector2(130f, 28f), Creme);

        SistemaDeEventos(pai);
        return menu;
    }

    static CartaoFase CriarCartao(Transform pai, DefinicaoFase fase, Vector2 posicao, Vector2 tamanho)
    {
        var fundo = Imagem(pai, "Cartao" + fase.numero, Sprite("painel"), Centro, posicao, tamanho, true);
        var cartao = fundo.gameObject.AddComponent<CartaoFase>();
        cartao.numero = fase.numero;
        cartao.botao = fundo.gameObject.AddComponent<Button>();
        cartao.botao.targetGraphic = fundo;

        var conteudo = Retangulo(fundo.transform, "Conteudo", Centro, Vector2.zero, tamanho);
        cartao.conteudo = conteudo.gameObject.AddComponent<CanvasGroup>();
        Texto(conteudo, "Numero", fase.numero.ToString(), 40, TextAnchor.MiddleCenter, Topo, new Vector2(0f, -10f), new Vector2(tamanho.x, 48f), Marrom, contorno: false);
        Texto(conteudo, "Dificuldade", fase.dificuldade, 13, TextAnchor.MiddleCenter, Topo, new Vector2(0f, -60f), new Vector2(tamanho.x, 20f), Marrom, contorno: false);

        cartao.estrelaCheia = Sprite("estrela");
        cartao.estrelaVazia = Sprite("estrela_vazia");
        cartao.estrelas = new Image[3];
        for (int i = 0; i < 3; i++)
            cartao.estrelas[i] = Imagem(conteudo, "Estrela" + (i + 1), cartao.estrelaVazia, Baixo, new Vector2((i - 1) * 25f, 12f), new Vector2(22f, 22f));

        cartao.cadeado = Imagem(fundo.transform, "Cadeado", Sprite("cadeado"), Centro, new Vector2(0f, -4f), new Vector2(44f, 44f)).gameObject;
        cartao.cadeado.GetComponent<Image>().raycastTarget = false;
        cartao.cadeado.SetActive(fase.numero > 1); // estado inicial (o jogo atualiza ao abrir o menu)
        return cartao;
    }
}
