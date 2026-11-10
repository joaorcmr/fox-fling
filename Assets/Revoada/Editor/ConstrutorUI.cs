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

    // Muda o pivô para o centro sem mexer em onde o elemento aparece: para
    // animar a escala em volta do meio (pulsar, bater), e não de um canto.
    public static RectTransform CentralizarPivo(RectTransform rt)
    {
        var centro = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition += Vector2.Scale(centro - rt.pivot, rt.sizeDelta);
        rt.pivot = centro;
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
        fundo.gameObject.AddComponent<BotaoComSom>();
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
    // HUD do jogo: a barra lateral (152 × 480) e os avisos por cima do mapa
    // ------------------------------------------------------------------
    const float CentroMapa = 224f; // o mapa ocupa os 448 px da esquerda

    public static Hud CriarHud(Transform pai, Jogo jogo, Controles controles, TipoPorco[] porcos, Efeitos efeitos,
        VisualMapa mapa, RevelarTrilha revelar)
    {
        var canvas = Canvas(pai, "HUD", 10);
        var t = canvas.transform;
        var hud = canvas.gameObject.AddComponent<Hud>();
        hud.jogo = jogo;

        // O fundo da barra bloqueia os cliques: o que cai nele não chega ao mapa.
        var barra = Imagem(t, "BarraLateral", Sprite("painel"), TopoDir, Vector2.zero,
            new Vector2(LarguraBarra, ConstrutorRevoada.Altura), true).transform;

        // Topo: vida, moedas e onda à esquerda; velocidade e menu à direita. O
        // ☰ já pausa ao abrir o menu (Continuar/Reiniciar/Menu): um botão de
        // pausa avulso ao lado dele pausava do mesmo jeito, por um caminho
        // diferente — dois controles para a mesma coisa (ver #13). P continua
        // pausando pelo teclado (documentado em "Como jogar").
        hud.iconeVida = Imagem(barra, "IconeVida", Sprite("icone_vida"), TopoEsq, new Vector2(10f, -8f), new Vector2(26f, 26f));
        hud.iconeVida.raycastTarget = false;
        hud.textoVida = Texto(barra, "Vida", "100", 22, TextAnchor.MiddleLeft, TopoEsq, new Vector2(40f, -8f), new Vector2(72f, 26f), Marrom);
        Imagem(barra, "IconeMoeda", Sprite("icone_moeda"), TopoEsq, new Vector2(10f, -38f), new Vector2(26f, 26f)).raycastTarget = false;
        hud.textoMoedas = Texto(barra, "Moedas", "300", 22, TextAnchor.MiddleLeft, TopoEsq, new Vector2(40f, -38f), new Vector2(72f, 26f), Marrom);
        hud.textoOnda = Texto(barra, "Onda", "Onda 1/30", 14, TextAnchor.MiddleLeft, TopoEsq, new Vector2(10f, -68f), new Vector2(80f, 22f), Marrom);
        // A fase ao lado da onda: o jogador sempre sabe em que mapa está.
        hud.textoFase = Texto(barra, "Fase", "Fase 1/3", 12, TextAnchor.MiddleRight, TopoDir, new Vector2(-10f, -68f), new Vector2(56f, 22f),
            Marrom, negrito: false);

        hud.botaoVelocidade = Botao(barra, "BotaoVelocidade", null, TopoEsq, new Vector2(114f, -8f), new Vector2(28f, 28f));
        hud.textoVelocidade = Texto(hud.botaoVelocidade.transform, "Texto", "1×", 13, TextAnchor.MiddleCenter, Centro,
            new Vector2(0f, 1f), new Vector2(28f, 28f), Creme, contorno: true);
        hud.botaoMenu = Botao(barra, "BotaoMenu", Sprite("icone_menu"), TopoEsq, new Vector2(114f, -38f), new Vector2(28f, 28f));

        // A loja e, no mesmo lugar, o painel do porco selecionado. Tudo o que se
        // compra tem a mesma cara (consistência): os 4 porcos, o Reparo da Torre
        // e o Cofrinho são cartões iguais, 2 por linha; embaixo do Cofrinho, a
        // linha de "se paga em ~N ondas"; depois, a prévia da próxima onda.
        var loja = Retangulo(barra, "Loja", TopoEsq, new Vector2(0f, -96f), new Vector2(LarguraBarra, 314f));
        hud.grupoLoja = loja.gameObject;
        var balao = CriarBalao(t);
        hud.cartoes = new CartaoPorcoLoja[porcos.Length];
        for (int i = 0; i < porcos.Length; i++)
        {
            hud.cartoes[i] = CriarCartao(loja, porcos[i], PosicaoCartao(i));
            hud.cartoes[i].balao = balao;
        }
        hud.cartaoReparo = CriarReparo(loja, jogo, PosicaoCartao(porcos.Length));
        hud.cartaoReparo.balao = balao;
        hud.botaoReparo = hud.cartaoReparo.botao;
        hud.cartaoCofrinho = CriarCofrinho(loja, jogo, PosicaoCartao(porcos.Length + 1));
        hud.cartaoCofrinho.balao = balao;
        hud.previa = CriarPrevia(loja, jogo, new Vector2(8f, PosicaoCartao(porcos.Length + 2).y - 30f));
        hud.painelPorco = CriarPainelPorco(barra, jogo, new Vector2(8f, -98f));

        // Sem Auto: o jogador decide toda vez quando a próxima onda começa.
        // Depois de uma troca de fase, ele pulsa (em volta do centro) até a próxima onda.
        hud.botaoSoltar = Botao(barra, "BotaoSoltar", null, BaixoEsq, new Vector2(8f, 10f), new Vector2(136f, 52f));
        CentralizarPivo((RectTransform)hud.botaoSoltar.transform);
        Imagem(hud.botaoSoltar.transform, "Icone", Sprite("icone_play"), Esq, new Vector2(8f, 1f), new Vector2(26f, 26f)).raycastTarget = false;
        hud.textoSoltar = Texto(hud.botaoSoltar.transform, "Texto", "Soltar a onda", 13, TextAnchor.MiddleCenter, Esq,
            new Vector2(36f, 1f), new Vector2(96f, 40f), Creme, contorno: true);

        // Por cima do mapa.
        hud.faixa = CriarFaixa(t);
        hud.barraChefe = CriarBarraChefe(t);
        hud.textoAviso = Texto(t, "Aviso", "", 15, TextAnchor.MiddleCenter, BaixoEsq, new Vector2(CentroMapa - 200f, 14f),
            new Vector2(400f, 22f), Creme, contorno: true);
        CriarDicas(t, jogo);
        int totalFases = jogo.fases != null ? jogo.fases.mapas.Count : 0;
        controles.trocaDeFase = CriarTrocaDeFase(t, jogo, hud, mapa, revelar, totalFases);
        CriarTelaFim(t, jogo, efeitos);
        hud.menuPausa = CriarMenuPausa(t, jogo);
        controles.menuPausa = hud.menuPausa;

        SistemaDeEventos(pai);
        return hud;
    }

    // Os cartões da loja, em 2 colunas: 0–3 os porcos, 4 o Reparo, 5 o Cofrinho.
    static readonly Vector2 TamanhoCartao = new Vector2(64f, 64f);

    public static Vector2 PosicaoCartao(int i) => new Vector2(10f + (i % 2) * 68f, -(i / 2) * 68f);

    // A troca de fase (TrocaDeFase): a cortina sobre o mapa, o véu sobre a barra
    // (os dois bloqueiam os cliques enquanto a troca acontece) e o quadro que
    // anuncia a fase nova, no meio do mapa.
    static TrocaDeFase CriarTrocaDeFase(Transform pai, Jogo jogo, Hud hud, VisualMapa mapa, RevelarTrilha revelar, int totalFases)
    {
        var raiz = Esticar(Retangulo(pai, "TrocaDeFase", Centro, Vector2.zero, Vector2.zero));
        var f = raiz.gameObject.AddComponent<TrocaDeFase>();
        f.jogo = jogo;
        f.hud = hud;
        f.mapa = mapa;
        f.revelar = revelar;
        f.grupo = raiz.gameObject.AddComponent<CanvasGroup>();
        f.grupo.blocksRaycasts = false;

        var escuro = new Color(0.17f, 0.11f, 0.08f, 0f); // o marrom mais escuro da paleta, não preto
        f.cortina = Imagem(raiz, "Cortina", null, Centro, Vector2.zero, Vector2.zero);
        Esticar(f.cortina.rectTransform).offsetMax = new Vector2(-LarguraBarra, 0f); // só o mapa
        f.cortina.color = escuro;
        f.veuBarra = Imagem(raiz, "VeuBarra", null, Centro, Vector2.zero, Vector2.zero);
        var veu = Esticar(f.veuBarra.rectTransform);
        veu.anchorMin = new Vector2(1f, 0f);
        veu.offsetMin = new Vector2(-LarguraBarra, 0f); // só a barra
        f.veuBarra.color = escuro;

        var quadro = Imagem(f.cortina.transform, "Quadro", Sprite("painel"), Centro, Vector2.zero, new Vector2(340f, 262f), true);
        f.quadro = quadro.rectTransform;
        f.grupoQuadro = quadro.gameObject.AddComponent<CanvasGroup>();
        var q = quadro.transform;
        f.titulo = Texto(q, "Titulo", "Fase 2 de 3", 24, TextAnchor.MiddleCenter, Topo, new Vector2(0f, -14f), new Vector2(310f, 30f), Marrom);
        f.pontoCheio = Sprite("ponto_nivel");
        f.pontoVazio = Sprite("ponto_nivel_vazio");
        int n = Mathf.Max(1, totalFases);
        f.pontosFase = new Image[n];
        for (int i = 0; i < n; i++)
        {
            f.pontosFase[i] = Imagem(q, "Ponto" + (i + 1), f.pontoVazio, Topo, new Vector2((i - (n - 1) / 2f) * 20f, -46f), new Vector2(14f, 14f));
            f.pontosFase[i].raycastTarget = false;
        }
        f.lugar = Texto(q, "Lugar", "Pomar, à tarde", 16, TextAnchor.MiddleCenter, Topo, new Vector2(0f, -64f), new Vector2(310f, 22f),
            new Color(0.31f, 0.5f, 0.21f));
        f.detalhes = Texto(q, "Detalhes", "", 13, TextAnchor.UpperLeft, Topo, new Vector2(0f, -94f), new Vector2(304f, 104f), Marrom, negrito: false);
        f.botaoContinuar = Botao(q, "Continuar", null, Baixo, new Vector2(0f, 14f), new Vector2(200f, 40f));
        CentralizarPivo((RectTransform)f.botaoContinuar.transform);
        Texto(f.botaoContinuar.transform, "Texto", "Ver o mapa novo", 16, TextAnchor.MiddleCenter, Centro, new Vector2(0f, 1f),
            new Vector2(200f, 40f), Creme, contorno: true);
        quadro.gameObject.SetActive(false);
        return f;
    }

    static Dicas CriarDicas(Transform pai, Jogo jogo)
    {
        var fundo = Imagem(pai, "Dica", Sprite("painel"), BaixoEsq, new Vector2(16f, 42f), new Vector2(330f, 50f), true);
        fundo.raycastTarget = false;
        var d = fundo.gameObject.AddComponent<Dicas>();
        d.jogo = jogo;
        d.grupo = fundo.gameObject.AddComponent<CanvasGroup>();
        d.grupo.blocksRaycasts = false;
        Imagem(fundo.transform, "Icone", Sprite("icone_moeda"), Esq, new Vector2(8f, 0f), new Vector2(22f, 22f)).raycastTarget = false;
        d.texto = Texto(fundo.transform, "Texto", "", 12, TextAnchor.MiddleLeft, Esq, new Vector2(36f, 0f), new Vector2(286f, 44f), Marrom, negrito: false);
        return d;
    }

    static TelaFim CriarTelaFim(Transform pai, Jogo jogo, Efeitos efeitos)
    {
        var escuro = Imagem(pai, "TelaFim", null, Centro, Vector2.zero, Vector2.zero);
        Esticar(escuro.rectTransform);
        escuro.color = new Color(0.17f, 0.11f, 0.08f, 0.6f); // o marrom mais escuro da paleta, não preto
        var f = escuro.gameObject.AddComponent<TelaFim>();
        f.jogo = jogo;
        f.efeitos = efeitos;
        f.grupo = escuro.gameObject.AddComponent<CanvasGroup>();
        f.clarao = Imagem(escuro.transform, "Clarao", null, Centro, Vector2.zero, Vector2.zero);
        Esticar(f.clarao.rectTransform);
        f.clarao.raycastTarget = false;

        var painel = Imagem(escuro.transform, "Painel", Sprite("painel"), Centro, new Vector2(-76f, 0f), new Vector2(320f, 260f), true);
        f.painel = painel.rectTransform;
        var p = painel.transform;
        f.titulo = Texto(p, "Titulo", "A Torre resistiu!", 22, TextAnchor.MiddleCenter, Topo, new Vector2(0f, -16f), new Vector2(300f, 32f), Marrom);
        f.medalha = Imagem(p, "Medalha", Sprite("medalha_ouro"), Topo, new Vector2(0f, -52f), new Vector2(52f, 65f));
        f.medalha.preserveAspect = true;
        f.medalhaBronze = Sprite("medalha_bronze");
        f.medalhaPrata = Sprite("medalha_prata");
        f.medalhaOuro = Sprite("medalha_ouro");
        f.detalhes = Texto(p, "Detalhes", "", 15, TextAnchor.MiddleCenter, Topo, new Vector2(0f, -124f), new Vector2(300f, 22f), Marrom);
        f.recorde = Texto(p, "Recorde", "", 13, TextAnchor.MiddleCenter, Topo, new Vector2(0f, -150f), new Vector2(300f, 20f), Marrom, negrito: false);
        f.botaoDeNovo = Botao(p, "DeNovo", null, Baixo, new Vector2(-76f, 18f), new Vector2(140f, 40f));
        Texto(f.botaoDeNovo.transform, "Texto", "Jogar de novo", 15, TextAnchor.MiddleCenter, Centro, new Vector2(0f, 1f), new Vector2(140f, 40f), Creme, contorno: true);
        f.botaoMenu = Botao(p, "Menu", null, Baixo, new Vector2(76f, 18f), new Vector2(140f, 40f));
        Texto(f.botaoMenu.transform, "Texto", "Menu", 15, TextAnchor.MiddleCenter, Centro, new Vector2(0f, 1f), new Vector2(140f, 40f), Creme, contorno: true);
        return f;
    }

    // Controle de volume com os sprites do jogo (o DefaultControls do Unity monta
    // a estrutura do Slider; os sprites e as cores são trocados pelos nossos).
    public static Slider ControleDeVolume(Transform pai, string rotulo, string canal, Vector2 ancora, Vector2 posicao, Color corTexto)
    {
        var raiz = Retangulo(pai, "Volume" + rotulo, ancora, posicao, new Vector2(180f, 22f));
        Texto(raiz, "Rotulo", rotulo, 13, TextAnchor.MiddleLeft, Esq, Vector2.zero, new Vector2(60f, 22f), corTexto, contorno: corTexto == Creme);
        var go = DefaultControls.CreateSlider(new DefaultControls.Resources
        {
            standard = Sprite("barra"),
            background = Sprite("barra"),
            knob = Sprite("ponto_nivel"),
        });
        go.transform.SetParent(raiz, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = Dir;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(112f, 16f);
        var slider = go.GetComponent<Slider>();
        foreach (var img in go.GetComponentsInChildren<Image>())
        {
            if (img.gameObject.name == "Background") img.color = new Color(0.42f, 0.29f, 0.18f, 0.85f);
            else if (img.gameObject.name == "Fill") img.color = new Color(0.55f, 0.8f, 0.37f);
            else if (img.gameObject.name == "Handle") img.color = Color.white;
        }
        var controle = raiz.gameObject.AddComponent<ControleVolume>();
        controle.slider = slider;
        controle.canal = canal;
        return slider;
    }

    // Um cartão da loja: o fundo verde de botão, o ícone grande em cima e a
    // moeda com o preço embaixo. Porcos, Reparo e Cofrinho usam este mesmo.
    static Image CartaoLoja(Transform pai, string nome, Sprite icone, Vector2 posicao, out Button botao, out CanvasGroup grupo,
        out Image imagemIcone, out Text preco)
    {
        var fundo = Imagem(pai, nome, Sprite("botao"), TopoEsq, posicao, TamanhoCartao, true);
        botao = fundo.gameObject.AddComponent<Button>();
        botao.targetGraphic = fundo;
        grupo = fundo.gameObject.AddComponent<CanvasGroup>();
        imagemIcone = Imagem(fundo.transform, "Icone", icone, Topo, new Vector2(0f, -3f), new Vector2(40f, 40f));
        imagemIcone.raycastTarget = false;
        imagemIcone.preserveAspect = true;
        Imagem(fundo.transform, "IconeMoeda", Sprite("icone_moeda"), Baixo, new Vector2(-17f, 4f), new Vector2(16f, 16f)).raycastTarget = false;
        preco = Texto(fundo.transform, "Preco", "", 13, TextAnchor.MiddleLeft, Baixo, new Vector2(8f, 3f), new Vector2(36f, 18f), Creme, contorno: true);
        return fundo;
    }

    // O selo no canto do cartão: o que a compra dá ("+20" de vida, "×1,5" moedas).
    static Text Selo(Transform cartao, string conteudo, Color cor)
    {
        var t = Texto(cartao, "Selo", conteudo, 12, TextAnchor.UpperRight, TopoDir, new Vector2(-4f, -2f), new Vector2(44f, 16f), cor, contorno: true);
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        return t;
    }

    static CartaoReparo CriarReparo(Transform pai, Jogo jogo, Vector2 posicao)
    {
        var fundo = CartaoLoja(pai, "CartaoReparo", Sprite("icone_reparo"), posicao, out var botao, out var grupo, out var icone, out var preco);
        var c = fundo.gameObject.AddComponent<CartaoReparo>();
        c.jogo = jogo;
        c.botao = botao;
        c.grupo = grupo;
        c.preco = preco;
        c.icone = CentralizarPivo(icone.rectTransform); // o coração bate em volta do centro
        c.cura = Selo(fundo.transform, "+20", new Color(1f, 0.84f, 0.84f));
        return c;
    }

    static CartaoCofrinho CriarCofrinho(Transform pai, Jogo jogo, Vector2 posicao)
    {
        var fundo = CartaoLoja(pai, "CartaoCofrinho", Sprite("icone_cofrinho"), posicao, out var botao, out var grupo, out _, out var preco);
        var c = fundo.gameObject.AddComponent<CartaoCofrinho>();
        c.jogo = jogo;
        c.botao = botao;
        c.grupo = grupo;
        c.custo = preco;
        c.multiplicador = Selo(fundo.transform, "×1,25", new Color(1f, 0.9f, 0.45f));
        // Logo embaixo do cartão, em quantas ondas o próximo nível se paga.
        // Um fundo próprio (do mesmo tom da prévia) faz o texto parecer parte
        // da interface, e não uma legenda solta flutuando sobre o mapa; a
        // caixa tem folga (32, não só as 2 linhas justas) e fica um pouco
        // afastada do cartão de baixo (a prévia da onda) para a descida de
        // uma letra como o "g" de "paga" não encostar nela.
        var retornoFundo = Imagem(pai, "Retorno", Sprite("painel"), TopoEsq,
            new Vector2(posicao.x - 4f, posicao.y - TamanhoCartao.y - 2f), new Vector2(TamanhoCartao.x + 8f, 32f), true);
        retornoFundo.color = new Color(0.93f, 0.86f, 0.7f);
        retornoFundo.raycastTarget = false;
        c.retorno = Texto(retornoFundo.transform, "Texto", "se paga em ~3 ondas", 12, TextAnchor.MiddleCenter, Centro,
            Vector2.zero, new Vector2(TamanhoCartao.x, 30f), Marrom, negrito: false);
        return c;
    }

    public static PreviaOnda CriarPrevia(Transform pai, Jogo jogo, Vector2 posicao)
    {
        var fundo = Imagem(pai, "Previa", Sprite("painel"), TopoEsq, posicao, new Vector2(136f, 80f), true);
        fundo.color = new Color(0.93f, 0.86f, 0.7f);
        var p = fundo.gameObject.AddComponent<PreviaOnda>();
        p.jogo = jogo;
        // Fonte menor e caixa mais larga: "Próxima: onda 20" não pode quebrar
        // linha (o texto some pra cima, cortado pelo cartão).
        p.titulo = Texto(fundo.transform, "Titulo", "Próxima: onda 1", 12, TextAnchor.MiddleLeft, TopoEsq,
            new Vector2(6f, -5f), new Vector2(128f, 16f), Marrom);
        p.icones = new Image[4];
        p.quantidades = new Text[4];
        for (int i = 0; i < 4; i++)
        {
            var x = 6f + (i % 2) * 64f;
            var y = -22f - (i / 2) * 18f;
            p.icones[i] = Imagem(fundo.transform, "Icone" + i, null, TopoEsq, new Vector2(x, y), new Vector2(22f, 22f));
            p.icones[i].preserveAspect = true;
            p.icones[i].raycastTarget = false;
            p.quantidades[i] = Texto(fundo.transform, "Quantidade" + i, "×0", 13, TextAnchor.MiddleLeft, TopoEsq,
                new Vector2(x + 24f, y - 2f), new Vector2(38f, 18f), Marrom);
        }
        // Centrado: longe dos cantos arredondados do painel. O texto do aviso
        // nunca é o nome puro de um pássaro (PreviaOnda.Mostrar troca por uma
        // versão mais curta, sem o detalhe entre parênteses, quando quebraria
        // linha), então 1 linha de sobra já basta: o cartão não tem espaço
        // pra 2, o botão de soltar a onda fica logo embaixo dele.
        p.aviso = Texto(fundo.transform, "Aviso", "", 12, TextAnchor.MiddleCenter, Topo, new Vector2(0f, -62f),
            new Vector2(128f, 16f), new Color(0.72f, 0.2f, 0.16f));
        return p;
    }

    static PainelPorco CriarPainelPorco(Transform pai, Jogo jogo, Vector2 posicao)
    {
        var raiz = Retangulo(pai, "PainelPorco", TopoEsq, posicao, new Vector2(136f, 300f));
        var p = raiz.gameObject.AddComponent<PainelPorco>();
        p.jogo = jogo;
        var r = raiz.transform;

        p.icone = Imagem(r, "Icone", null, TopoEsq, Vector2.zero, new Vector2(34f, 34f));
        p.icone.preserveAspect = true;
        p.icone.raycastTarget = false;
        // "Estilingueiro" é o nome mais comprido: a caixa vai até a borda e o × fica na linha das bolinhas.
        p.nome = Texto(r, "Nome", "Porco", 14, TextAnchor.MiddleLeft, TopoEsq, new Vector2(38f, 0f), new Vector2(98f, 18f), Marrom);
        p.nome.horizontalOverflow = HorizontalWrapMode.Overflow;
        p.pontoCheio = Sprite("ponto_nivel");
        p.pontoVazio = Sprite("ponto_nivel_vazio");
        p.pontos = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            p.pontos[i] = Imagem(r, "Nivel" + (i + 1), p.pontoVazio, TopoEsq, new Vector2(40f + i * 15f, -20f), new Vector2(12f, 12f));
            p.pontos[i].raycastTarget = false;
        }
        p.botaoFechar = Botao(r, "Fechar", null, TopoEsq, new Vector2(110f, -18f), new Vector2(26f, 20f));
        Texto(p.botaoFechar.transform, "X", "×", 15, TextAnchor.MiddleCenter, Centro, new Vector2(0f, 1f), new Vector2(26f, 20f), Creme, contorno: true);
        p.numeros = Texto(r, "Numeros", "", 12, TextAnchor.UpperLeft, TopoEsq, new Vector2(0f, -38f), new Vector2(136f, 44f), Marrom, negrito: false);

        // Próxima melhoria (níveis 2 e 3).
        p.botaoMelhoria = Botao(r, "Melhoria", null, TopoEsq, new Vector2(0f, -84f), new Vector2(136f, 72f));
        var m = p.botaoMelhoria.transform;
        p.nomeMelhoria = Texto(m, "Nome", "", 13, TextAnchor.UpperLeft, TopoEsq, new Vector2(6f, -4f), new Vector2(124f, 16f), Creme, contorno: true);
        p.descricaoMelhoria = Texto(m, "Descricao", "", 12, TextAnchor.UpperLeft, TopoEsq, new Vector2(6f, -21f), new Vector2(124f, 30f),
            Creme, contorno: true, negrito: false);
        Imagem(m, "IconeMoeda", Sprite("icone_moeda"), BaixoEsq, new Vector2(6f, 4f), new Vector2(14f, 14f)).raycastTarget = false;
        p.custoMelhoria = Texto(m, "Custo", "", 13, TextAnchor.MiddleLeft, BaixoEsq, new Vector2(22f, 3f), new Vector2(60f, 16f), Creme, contorno: true);

        // As duas especializações (no nível 3), uma em cima da outra.
        p.botoesEspecializacao = new Button[2];
        p.nomesEspecializacao = new Text[2];
        p.descricoesEspecializacao = new Text[2];
        p.custosEspecializacao = new Text[2];
        for (int i = 0; i < 2; i++)
        {
            var b = Botao(r, "Especializacao" + (i == 0 ? "A" : "B"), null, TopoEsq, new Vector2(0f, -84f - i * 64f), new Vector2(136f, 60f));
            p.botoesEspecializacao[i] = b;
            p.nomesEspecializacao[i] = Texto(b.transform, "Nome", "", 13, TextAnchor.UpperLeft, TopoEsq, new Vector2(6f, -4f), new Vector2(84f, 16f), Creme, contorno: true);
            Imagem(b.transform, "IconeMoeda", Sprite("icone_moeda"), TopoDir, new Vector2(-40f, -5f), new Vector2(13f, 13f)).raycastTarget = false;
            p.custosEspecializacao[i] = Texto(b.transform, "Custo", "", 13, TextAnchor.UpperLeft, TopoDir, new Vector2(-4f, -4f), new Vector2(36f, 16f), Creme, contorno: true);
            p.descricoesEspecializacao[i] = Texto(b.transform, "Descricao", "", 12, TextAnchor.UpperLeft, TopoEsq, new Vector2(6f, -21f),
                new Vector2(124f, 36f), Creme, contorno: true, negrito: false);
        }
        p.textoEspecializado = Texto(r, "Especializado", "", 12, TextAnchor.UpperLeft, TopoEsq, new Vector2(0f, -84f), new Vector2(136f, 60f), Marrom);

        p.botaoModo = Botao(r, "Modo", null, TopoEsq, new Vector2(0f, -216f), new Vector2(136f, 26f));
        p.textoModo = Texto(p.botaoModo.transform, "Texto", "Mira em: o primeiro", 12, TextAnchor.MiddleCenter, Centro,
            Vector2.zero, new Vector2(136f, 26f), Creme, contorno: true);
        p.botaoVender = Botao(r, "Vender", null, TopoEsq, new Vector2(0f, -248f), new Vector2(136f, 28f));
        ((Image)p.botaoVender.targetGraphic).color = new Color(1f, 0.72f, 0.62f);
        p.textoVender = Texto(p.botaoVender.transform, "Texto", "Vender", 13, TextAnchor.MiddleCenter, Centro,
            Vector2.zero, new Vector2(136f, 28f), Creme, contorno: true);
        return p;
    }

    static BalaoLoja CriarBalao(Transform pai)
    {
        var fundo = Imagem(pai, "BalaoLoja", Sprite("painel"), TopoDir, new Vector2(-LarguraBarra - 4f, -120f), new Vector2(176f, 62f), true);
        fundo.raycastTarget = false;
        var b = fundo.gameObject.AddComponent<BalaoLoja>();
        b.caixa = fundo.rectTransform;
        b.caixa.pivot = new Vector2(1f, 0.5f); // a altura acompanha o centro do cartão
        b.titulo = Texto(fundo.transform, "Titulo", "", 13, TextAnchor.UpperLeft, TopoEsq, new Vector2(8f, -6f), new Vector2(160f, 16f), Marrom);
        b.texto = Texto(fundo.transform, "Texto", "", 12, TextAnchor.UpperLeft, TopoEsq, new Vector2(8f, -23f), new Vector2(160f, 34f), Marrom, negrito: false);
        return b;
    }

    static Faixa CriarFaixa(Transform pai)
    {
        var caixa = Retangulo(pai, "Faixa", TopoEsq, new Vector2(CentroMapa - 140f, -40f), new Vector2(280f, 42f));
        var f = caixa.gameObject.AddComponent<Faixa>();
        f.caixa = caixa;
        f.grupo = caixa.gameObject.AddComponent<CanvasGroup>();
        f.grupo.blocksRaycasts = false;
        f.fundo = Imagem(caixa, "Fundo", Sprite("botao"), Centro, Vector2.zero, new Vector2(280f, 42f), true);
        f.fundo.raycastTarget = false;
        f.texto = Texto(caixa, "Texto", "Onda 1", 22, TextAnchor.MiddleCenter, Centro, new Vector2(0f, 1f), new Vector2(270f, 40f), Creme, contorno: true);
        return f;
    }

    static BarraChefe CriarBarraChefe(Transform pai)
    {
        var caixa = Retangulo(pai, "BarraChefe", TopoEsq, new Vector2(CentroMapa - 150f, -6f), new Vector2(300f, 30f));
        var b = caixa.gameObject.AddComponent<BarraChefe>();
        b.grupo = caixa.gameObject.AddComponent<CanvasGroup>();
        b.grupo.blocksRaycasts = false;
        b.nome = Texto(caixa, "Nome", "Tubarão-rei", 13, TextAnchor.MiddleLeft, TopoEsq, Vector2.zero, new Vector2(300f, 14f), Creme, contorno: true);
        var fundo = Imagem(caixa, "Fundo", Sprite("barra"), TopoEsq, new Vector2(0f, -16f), new Vector2(300f, 12f));
        fundo.color = new Color(0.17f, 0.11f, 0.08f, 0.9f);
        fundo.raycastTarget = false;
        b.preenchimento = Imagem(caixa, "Vida", Sprite("barra"), TopoEsq, new Vector2(2f, -18f), new Vector2(296f, 8f));
        b.preenchimento.type = Image.Type.Filled;
        b.preenchimento.fillMethod = Image.FillMethod.Horizontal;
        b.preenchimento.color = new Color(0.86f, 0.25f, 0.2f);
        b.preenchimento.raycastTarget = false;
        return b;
    }

    static MenuPausa CriarMenuPausa(Transform pai, Jogo jogo)
    {
        var escuro = Imagem(pai, "MenuPausa", null, Centro, Vector2.zero, Vector2.zero);
        Esticar(escuro.rectTransform);
        escuro.color = new Color(0f, 0f, 0f, 0.5f); // bloqueia os cliques no jogo
        var m = escuro.gameObject.AddComponent<MenuPausa>();
        m.jogo = jogo;
        var painel = Imagem(escuro.transform, "Painel", Sprite("painel"), Centro, Vector2.zero, new Vector2(240f, 276f), true).transform;
        Texto(painel, "Titulo", "Pausado", 24, TextAnchor.MiddleCenter, Topo, new Vector2(0f, -14f), new Vector2(220f, 32f), Marrom);
        m.botaoContinuar = BotaoTexto(painel, "Continuar", "Continuar", new Vector2(0f, 58f));
        m.botaoReiniciar = BotaoTexto(painel, "Reiniciar", "Reiniciar", new Vector2(0f, 12f));
        m.botaoMenu = BotaoTexto(painel, "Menu", "Menu", new Vector2(0f, -34f));
        ControleDeVolume(painel, "Música", "musica", Baixo, new Vector2(0f, 44f), Marrom);
        ControleDeVolume(painel, "Efeitos", "efeitos", Baixo, new Vector2(0f, 16f), Marrom);
        escuro.gameObject.SetActive(false);
        return m;
    }

    // ------------------------------------------------------------------
    // Tela de título (cena Menu): o mapa ao fundo, o logo a ~60% da altura,
    // Jogar, Como jogar, o recorde e os volumes.
    // ------------------------------------------------------------------
    public static MenuPrincipal CriarMenu(Transform pai)
    {
        var canvas = Canvas(pai, "Menu", 10);
        var t = canvas.transform;
        var m = canvas.gameObject.AddComponent<MenuPrincipal>();

        var veu = Imagem(t, "Veu", null, Centro, Vector2.zero, Vector2.zero);
        Esticar(veu.rectTransform);
        veu.color = new Color(0.17f, 0.11f, 0.08f, 0.35f);
        veu.raycastTarget = false;

        var logo = Imagem(t, "Logo", Sprite("logo"), Topo, new Vector2(0f, -108f), new Vector2(420f, 112f));
        logo.raycastTarget = false;
        m.logo = logo.rectTransform;

        // Issue #16: a tela de início ficava com tudo solto sobre o mapa de
        // enfeite (texto direto em cima da trilha e dos bichos andando), o que
        // deixava a leitura difícil. Dois cartões (o mesmo "painel" do resto da
        // interface) agrupam o conteúdo: um para jogar, outro para os ajustes.
        var quadroJogar = Imagem(t, "QuadroJogar", Sprite("painel"), Centro, new Vector2(0f, -73f), new Vector2(460f, 212f), true);
        quadroJogar.raycastTarget = false;
        m.cartaoJogar = quadroJogar.rectTransform;
        Texto(t, "Subtitulo", "Defenda a Torre dos Porcos", 16, TextAnchor.MiddleCenter, Topo, new Vector2(0f, -238f),
            new Vector2(400f, 22f), Marrom);

        m.botaoJogar = Botao(t, "Jogar", null, Centro, new Vector2(0f, -58f), new Vector2(200f, 54f));
        Texto(m.botaoJogar.transform, "Texto", "Jogar", 24, TextAnchor.MiddleCenter, Centro, new Vector2(0f, 1f), new Vector2(200f, 54f), Creme, contorno: true);
        m.botaoComoJogar = Botao(t, "ComoJogar", null, Centro, new Vector2(0f, -104f), new Vector2(160f, 34f));
        Texto(m.botaoComoJogar.transform, "Texto", "Como jogar", 15, TextAnchor.MiddleCenter, Centro, new Vector2(0f, 1f), new Vector2(160f, 34f), Creme, contorno: true);
        m.textoRecorde = Texto(t, "Recorde", "", 14, TextAnchor.MiddleCenter, Centro, new Vector2(0f, -142f), new Vector2(440f, 22f), Marrom);

        var quadroAjustes = Imagem(t, "QuadroAjustes", Sprite("painel"), Baixo, new Vector2(0f, 4f), new Vector2(580f, 72f), true);
        quadroAjustes.raycastTarget = false;
        m.cartaoAjustes = quadroAjustes.rectTransform;
        ControleDeVolume(t, "Música", "musica", Baixo, new Vector2(-100f, 44f), Marrom);
        ControleDeVolume(t, "Efeitos", "efeitos", Baixo, new Vector2(100f, 44f), Marrom);
        Texto(t, "Creditos", "Insper", 12, TextAnchor.MiddleLeft, BaixoEsq, new Vector2(18f, 12f), new Vector2(260f, 20f), Marrom);
        m.botaoZerar = Botao(t, "Zerar", null, BaixoDir, new Vector2(-18f, 12f), new Vector2(124f, 24f));
        Texto(m.botaoZerar.transform, "Texto", "Zerar progresso", 12, TextAnchor.MiddleCenter, Centro, Vector2.zero, new Vector2(124f, 24f), Creme, contorno: true);

        // "Como jogar": as regras em tópicos, com bullets, em duas colunas (tudo
        // à vista de uma vez, sem rolar nem trocar de página). Os números vêm
        // dos assets de dados, então o texto acompanha o balanceamento.
        var escuro = Imagem(t, "ComoJogar", null, Centro, Vector2.zero, Vector2.zero);
        Esticar(escuro.rectTransform);
        escuro.color = new Color(0.17f, 0.11f, 0.08f, 0.55f);
        m.painelComoJogar = escuro.gameObject;
        var quadro = Imagem(escuro.transform, "Quadro", Sprite("painel"), Centro, Vector2.zero, new Vector2(572f, 452f), true).transform;
        Texto(quadro, "Titulo", "Como jogar", 22, TextAnchor.MiddleCenter, Topo, new Vector2(0f, -12f), new Vector2(520f, 30f), Marrom);
        var dados = ConstrutorDados.Carregar();
        Texto(quadro, "RegrasEsquerda", RegrasEsquerda(dados), 12, TextAnchor.UpperLeft, TopoEsq, new Vector2(24f, -46f),
            new Vector2(254f, 340f), Marrom, negrito: false);
        Texto(quadro, "RegrasDireita", RegrasDireita(dados), 12, TextAnchor.UpperLeft, TopoDir, new Vector2(-24f, -46f),
            new Vector2(254f, 340f), Marrom, negrito: false);
        m.botaoFecharComoJogar = BotaoTexto(quadro, "Fechar", "Fechar", new Vector2(0f, -196f));

        m.painelConfirmarZerar = CriarConfirmarZerar(t, out m.botaoCancelarZerar, out m.botaoConfirmarZerar);
        return m;
    }

    // Os tópicos do "Como jogar". Só caracteres que a fonte padrão tem em
    // qualquer plataforma (no WebGL não há fonte do sistema para completar).
    static string Topico(string titulo) => $"<size=14><b><color=#4F7F35>{titulo}</color></b></size>\n";

    static string RegrasEsquerda(DadosJogo d)
    {
        var e = d.economia;
        return Topico("Objetivo") +
            $"• Proteja a Torre dos Porcos por {d.ondas.Count} ondas.\n" +
            "• Cada pássaro que chega na Torre tira vida. Com 0, ela cai.\n" +
            $"• Chefes (tubarão-rei) nas ondas {OndasDeChefe(d)}.\n\n" +
            Topico("Ondas") +
            "• Nenhuma onda começa sozinha: aperte Soltar a onda (ou Espaço).\n" +
            "• A prévia, na barra, mostra quem vem na próxima.\n\n" +
            Topico("Loja e moedas") +
            "• Pássaros estourados e ondas vencidas dão moedas.\n" +
            "• Só dá para comprar entre ondas.\n" +
            "• Porco: clique no cartão (ou 1 a 4) e depois no mapa.\n" +
            $"• Reparo (coração): +{e.curaPorReparo} de vida na Torre; fica mais caro a cada uso.\n" +
            "• Cofrinho: cada pássaro passa a render mais moedas.";
    }

    // "10, 20 e 30": as ondas que trazem um pássaro chefe.
    static string OndasDeChefe(DadosJogo d)
    {
        var ondas = new System.Collections.Generic.List<string>();
        for (int i = 0; i < d.ondas.Count; i++)
            if (d.ondas[i].grupos.Exists(g => d.passaros.TryGetValue(g.especie, out var p) && p.chefe)) ondas.Add((i + 1).ToString());
        if (ondas.Count <= 1) return string.Join("", ondas);
        return string.Join(", ", ondas.GetRange(0, ondas.Count - 1)) + " e " + ondas[ondas.Count - 1];
    }

    static string RegrasDireita(DadosJogo d)
    {
        var e = d.economia;
        int ondasPorFase = d.fases.Count > 1 ? d.ondas.Count / d.fases.Count : d.ondas.Count;
        string devolve = e.fracaoTrocaDeFase >= 0.999f ? "pelo valor inteiro" : $"com {Formato.Porcento(e.fracaoTrocaDeFase)} do que custaram";
        return Topico("Fases e mapas") +
            $"• A cada {ondasPorFase} ondas o mapa muda e os pássaros vêm por outro caminho.\n" +
            $"• Na troca, todos os porcos voltam para a carteira {devolve}: monte a defesa de novo.\n\n" +
            Topico("Porcos") +
            $"• Clique num porco para melhorar (U), mudar a mira ou vender (Delete, devolve {Formato.Porcento(e.fracaoVenda)}).\n" +
            "• O arpão atravessa a fila; a bomba pega bandos e a couraça do urubu.\n" +
            "• No círculo do Tamboreiro, os porcos atiram mais rápido.\n\n" +
            Topico("Atalhos") +
            "• P pausa · F acelera (1×, 2×, 3×)\n" +
            "• Esc abre o menu · clique direito cancela";
    }

    // "Zerar progresso" apaga para sempre (recorde, vitórias e dicas vistas); perguntar
    // antes dá uma saída para quem clicou sem querer (controle e liberdade do usuário).
    static GameObject CriarConfirmarZerar(Transform pai, out Button cancelar, out Button confirmar)
    {
        var escuro = Imagem(pai, "ConfirmarZerar", null, Centro, Vector2.zero, Vector2.zero);
        Esticar(escuro.rectTransform);
        escuro.color = new Color(0.17f, 0.11f, 0.08f, 0.55f);
        var quadro = Imagem(escuro.transform, "Quadro", Sprite("painel"), Centro, Vector2.zero, new Vector2(260f, 260f), true).transform;
        Texto(quadro, "Titulo", "Zerar progresso?", 19, TextAnchor.MiddleCenter, Topo, new Vector2(0f, -16f), new Vector2(230f, 28f), Marrom);
        Texto(quadro, "Aviso", "Apaga o recorde, as vitórias e as dicas vistas. Não dá para desfazer.",
            13, TextAnchor.MiddleCenter, Centro, new Vector2(0f, 24f), new Vector2(220f, 70f), Marrom, negrito: false);
        cancelar = BotaoTexto(quadro, "Cancelar", "Cancelar", new Vector2(0f, -60f));
        confirmar = BotaoTexto(quadro, "Confirmar", "Zerar mesmo assim", new Vector2(0f, -106f));
        ((Image)confirmar.targetGraphic).color = new Color(1f, 0.72f, 0.62f); // destrutivo: a mesma cor do "Vender"
        escuro.gameObject.SetActive(false);
        return escuro.gameObject;
    }

    public static Button BotaoTexto(Transform pai, string nome, string texto, Vector2 posicao)
    {
        var b = Botao(pai, nome, null, Centro, posicao, new Vector2(180f, 40f));
        Texto(b.transform, "Texto", texto, 16, TextAnchor.MiddleCenter, Centro, new Vector2(0f, 1f), new Vector2(180f, 40f), Creme, contorno: true);
        return b;
    }

    static CartaoPorcoLoja CriarCartao(Transform pai, TipoPorco tipo, Vector2 posicao)
    {
        var fundo = CartaoLoja(pai, "Cartao" + tipo.dados.especie, tipo.corpo, posicao, out var botao, out var grupo, out var icone, out var preco);
        var cartao = fundo.gameObject.AddComponent<CartaoPorcoLoja>();
        cartao.tipo = tipo;
        cartao.fundo = fundo;
        cartao.botao = botao;
        cartao.grupo = grupo;
        cartao.icone = icone;
        cartao.preco = preco;
        preco.text = tipo.dados.custo.ToString();
        return cartao;
    }
}
