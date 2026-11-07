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
    // HUD do jogo: a barra lateral (152 × 480) e os avisos por cima do mapa
    // ------------------------------------------------------------------
    const float CentroMapa = 224f; // o mapa ocupa os 448 px da esquerda

    public static Hud CriarHud(Transform pai, Jogo jogo, Controles controles, TipoPorco[] porcos)
    {
        var canvas = Canvas(pai, "HUD", 10);
        var t = canvas.transform;
        var hud = canvas.gameObject.AddComponent<Hud>();
        hud.jogo = jogo;
        hud.controles = controles;

        // O fundo da barra bloqueia os cliques: o que cai nele não chega ao mapa.
        var barra = Imagem(t, "BarraLateral", Sprite("painel"), TopoDir, Vector2.zero,
            new Vector2(LarguraBarra, ConstrutorRevoada.Altura), true).transform;

        // Topo: vida, penas e onda à esquerda; pausa, velocidade e menu à direita.
        Imagem(barra, "IconeVida", Sprite("icone_vida"), TopoEsq, new Vector2(10f, -8f), new Vector2(26f, 26f)).raycastTarget = false;
        hud.textoVida = Texto(barra, "Vida", "100", 22, TextAnchor.MiddleLeft, TopoEsq, new Vector2(40f, -8f), new Vector2(72f, 26f), Marrom);
        Imagem(barra, "IconePena", Sprite("icone_pena"), TopoEsq, new Vector2(10f, -38f), new Vector2(26f, 26f)).raycastTarget = false;
        hud.textoPenas = Texto(barra, "Penas", "300", 22, TextAnchor.MiddleLeft, TopoEsq, new Vector2(40f, -38f), new Vector2(72f, 26f), Marrom);
        hud.textoOnda = Texto(barra, "Onda", "Onda 1/30", 14, TextAnchor.MiddleLeft, TopoEsq, new Vector2(10f, -68f), new Vector2(104f, 22f), Marrom);

        hud.spritePausa = Sprite("icone_pausa");
        hud.spritePlay = Sprite("icone_play");
        hud.botaoPausa = Botao(barra, "BotaoPausa", null, TopoEsq, new Vector2(114f, -8f), new Vector2(28f, 28f));
        hud.iconePausa = Imagem(hud.botaoPausa.transform, "Icone", hud.spritePausa, Centro, Vector2.zero, new Vector2(18f, 18f));
        hud.iconePausa.raycastTarget = false;
        hud.botaoVelocidade = Botao(barra, "BotaoVelocidade", null, TopoEsq, new Vector2(114f, -38f), new Vector2(28f, 28f));
        hud.textoVelocidade = Texto(hud.botaoVelocidade.transform, "Texto", "1×", 13, TextAnchor.MiddleCenter, Centro,
            new Vector2(0f, 1f), new Vector2(28f, 28f), Creme, contorno: true);
        hud.botaoMenu = Botao(barra, "BotaoMenu", Sprite("icone_menu"), TopoEsq, new Vector2(114f, -68f), new Vector2(28f, 28f));

        // A loja (porcos, Ninho e prévia) e, no mesmo lugar, o painel do porco selecionado.
        var loja = Retangulo(barra, "Loja", TopoEsq, new Vector2(0f, -98f), new Vector2(LarguraBarra, 304f));
        hud.grupoLoja = loja.gameObject;
        var balao = CriarBalao(t);
        hud.cartoes = new CartaoPorcoLoja[porcos.Length];
        for (int i = 0; i < porcos.Length; i++)
        {
            var posicao = new Vector2(10f + (i % 2) * 68f, -(i / 2) * 72f);
            hud.cartoes[i] = CriarCartao(loja, porcos[i], posicao, new Vector2(64f, 68f));
            hud.cartoes[i].balao = balao;
        }
        hud.cartaoNinho = CriarNinho(loja, jogo, controles, new Vector2(8f, -144f));
        hud.previa = CriarPrevia(loja, jogo, new Vector2(8f, -206f));
        hud.painelPorco = CriarPainelPorco(barra, jogo, controles, new Vector2(8f, -98f));

        hud.botaoAuto = Botao(barra, "BotaoAuto", null, BaixoEsq, new Vector2(8f, 58f), new Vector2(136f, 20f));
        hud.textoAuto = Texto(hud.botaoAuto.transform, "Texto", "Auto: desligado", 12, TextAnchor.MiddleCenter, Centro,
            Vector2.zero, new Vector2(136f, 20f), Creme, contorno: true);
        hud.botaoSoltar = Botao(barra, "BotaoSoltar", null, BaixoEsq, new Vector2(8f, 10f), new Vector2(136f, 44f));
        Imagem(hud.botaoSoltar.transform, "Icone", Sprite("icone_play"), Esq, new Vector2(8f, 1f), new Vector2(26f, 26f)).raycastTarget = false;
        hud.textoSoltar = Texto(hud.botaoSoltar.transform, "Texto", "Soltar a revoada", 13, TextAnchor.MiddleCenter, Esq,
            new Vector2(36f, 1f), new Vector2(96f, 40f), Creme, contorno: true);

        // Por cima do mapa.
        hud.faixa = CriarFaixa(t);
        hud.barraChefe = CriarBarraChefe(t);
        hud.textoAviso = Texto(t, "Aviso", "", 15, TextAnchor.MiddleCenter, BaixoEsq, new Vector2(CentroMapa - 200f, 14f),
            new Vector2(400f, 22f), Creme, contorno: true);
        hud.menuPausa = CriarMenuPausa(t, jogo);
        controles.menuPausa = hud.menuPausa;

        SistemaDeEventos(pai);
        return hud;
    }

    static CartaoNinho CriarNinho(Transform pai, Jogo jogo, Controles controles, Vector2 posicao)
    {
        var fundo = Imagem(pai, "Ninho", Sprite("botao"), TopoEsq, posicao, new Vector2(136f, 58f), true);
        var c = fundo.gameObject.AddComponent<CartaoNinho>();
        c.jogo = jogo;
        c.controles = controles;
        c.botao = fundo.gameObject.AddComponent<Button>();
        c.botao.targetGraphic = fundo;
        c.grupo = fundo.gameObject.AddComponent<CanvasGroup>();
        Imagem(fundo.transform, "Icone", Sprite("icone_ninho"), TopoEsq, new Vector2(4f, -4f), new Vector2(26f, 26f)).raycastTarget = false;
        c.multiplicador = Texto(fundo.transform, "Multiplicador", "×1 → ×1,25", 13, TextAnchor.MiddleLeft, TopoEsq,
            new Vector2(32f, -4f), new Vector2(102f, 16f), Creme, contorno: true);
        c.multiplicador.horizontalOverflow = HorizontalWrapMode.Overflow;
        Imagem(fundo.transform, "IconePena", Sprite("icone_pena"), TopoEsq, new Vector2(34f, -22f), new Vector2(14f, 14f)).raycastTarget = false;
        c.custo = Texto(fundo.transform, "Custo", "250", 13, TextAnchor.MiddleLeft, TopoEsq, new Vector2(50f, -21f), new Vector2(80f, 16f), Creme, contorno: true);
        c.retorno = Texto(fundo.transform, "Retorno", "se paga em ~3 ondas", 12, TextAnchor.MiddleLeft, TopoEsq,
            new Vector2(6f, -38f), new Vector2(128f, 16f), Creme, contorno: true);
        return c;
    }

    static PreviaOnda CriarPrevia(Transform pai, Jogo jogo, Vector2 posicao)
    {
        var fundo = Imagem(pai, "Previa", Sprite("painel"), TopoEsq, posicao, new Vector2(136f, 92f), true);
        fundo.color = new Color(0.93f, 0.86f, 0.7f);
        var p = fundo.gameObject.AddComponent<PreviaOnda>();
        p.jogo = jogo;
        p.titulo = Texto(fundo.transform, "Titulo", "Próxima: onda 1", 13, TextAnchor.MiddleLeft, TopoEsq,
            new Vector2(8f, -6f), new Vector2(122f, 16f), Marrom);
        p.icones = new Image[4];
        p.quantidades = new Text[4];
        for (int i = 0; i < 4; i++)
        {
            var x = 6f + (i % 2) * 64f;
            var y = -24f - (i / 2) * 24f;
            p.icones[i] = Imagem(fundo.transform, "Icone" + i, null, TopoEsq, new Vector2(x, y), new Vector2(22f, 22f));
            p.icones[i].preserveAspect = true;
            p.icones[i].raycastTarget = false;
            p.quantidades[i] = Texto(fundo.transform, "Quantidade" + i, "×0", 13, TextAnchor.MiddleLeft, TopoEsq,
                new Vector2(x + 24f, y - 2f), new Vector2(38f, 18f), Marrom);
        }
        p.aviso = Texto(fundo.transform, "Aviso", "", 12, TextAnchor.MiddleLeft, TopoEsq, new Vector2(8f, -72f),
            new Vector2(124f, 16f), new Color(0.72f, 0.2f, 0.16f));
        return p;
    }

    static PainelPorco CriarPainelPorco(Transform pai, Jogo jogo, Controles controles, Vector2 posicao)
    {
        var raiz = Retangulo(pai, "PainelPorco", TopoEsq, posicao, new Vector2(136f, 300f));
        var p = raiz.gameObject.AddComponent<PainelPorco>();
        p.jogo = jogo;
        p.controles = controles;
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
        Imagem(m, "IconePena", Sprite("icone_pena"), BaixoEsq, new Vector2(6f, 4f), new Vector2(14f, 14f)).raycastTarget = false;
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
            Imagem(b.transform, "IconePena", Sprite("icone_pena"), TopoDir, new Vector2(-40f, -5f), new Vector2(13f, 13f)).raycastTarget = false;
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
        b.nome = Texto(caixa, "Nome", "Gavião-real", 13, TextAnchor.MiddleLeft, TopoEsq, Vector2.zero, new Vector2(300f, 14f), Creme, contorno: true);
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
        var painel = Imagem(escuro.transform, "Painel", Sprite("painel"), Centro, Vector2.zero, new Vector2(240f, 210f), true).transform;
        Texto(painel, "Titulo", "Pausado", 24, TextAnchor.MiddleCenter, Topo, new Vector2(0f, -14f), new Vector2(220f, 32f), Marrom);
        m.botaoContinuar = BotaoTexto(painel, "Continuar", "Continuar", new Vector2(0f, 26f));
        m.botaoReiniciar = BotaoTexto(painel, "Reiniciar", "Reiniciar", new Vector2(0f, -22f));
        m.botaoMenu = BotaoTexto(painel, "Menu", "Menu", new Vector2(0f, -70f));
        escuro.gameObject.SetActive(false);
        return m;
    }

    public static Button BotaoTexto(Transform pai, string nome, string texto, Vector2 posicao)
    {
        var b = Botao(pai, nome, null, Centro, posicao, new Vector2(180f, 40f));
        Texto(b.transform, "Texto", texto, 16, TextAnchor.MiddleCenter, Centro, new Vector2(0f, 1f), new Vector2(180f, 40f), Creme, contorno: true);
        return b;
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
