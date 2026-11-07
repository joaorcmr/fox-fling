using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// A loja do tower defense: uma barra no pé da tela (em cima da faixa de terra), aberta só
// entre as levas. Tem duas páginas: COMPRAS (um botão por item do catálogo) e MELHORIAS (um
// botão por tipo de arma, que sobe o nível de todas as armas daquele tipo). Os botões
// "Melhorias/Compras" e "Iniciar leva" ficam sempre à direita. A interface é montada aqui,
// por código, na hora que a fase começa (o GerenciadorFase cria esta loja), reaproveitando o
// estilo dos botões do HUD.
public class Loja : MonoBehaviour
{
    static readonly Color Creme = new Color(1f, 0.965f, 0.88f);
    static readonly Color Marrom = new Color(0.17f, 0.11f, 0.08f);

    // Medidas dos botões (a barra tem 52 px de altura e 588 de largura).
    const float LarguraItem = 64f, Espaco = 3f, Altura = 44f;

    class Entrada
    {
        public ItemLoja item;
        public Button botao;
        public Text texto;
    }

    GerenciadorFase fase;
    CatalogoLoja catalogo;
    Colocador colocador;
    GameObject raiz, paginaCompras, paginaMelhorias;
    Text textoDaPagina;
    Font fonte;
    Sprite spriteBotao, spritePainel;
    bool naPaginaDeMelhorias;
    readonly List<Entrada> compras = new List<Entrada>();
    readonly List<Entrada> melhorias = new List<Entrada>();

    public bool Aberta => raiz != null && raiz.activeSelf;

    // O jogador está com um item "na mão", escolhendo onde colocar.
    public bool EmColocacao => colocador != null && colocador.Ativo;

    // Devolve true se havia uma colocação em andamento (o HUD usa isso para o Esc).
    public bool CancelarColocacao() => colocador != null && colocador.Cancelar();

    public void Configurar(GerenciadorFase gerenciador, CatalogoLoja itens)
    {
        fase = gerenciador;
        catalogo = itens;
        colocador = gameObject.AddComponent<Colocador>();
        colocador.Configurar(gerenciador);
        PegarEstiloDoHud();
        Construir();
        Fechar();
    }

    public void Abrir(string mensagem)
    {
        raiz.SetActive(true);
        MostrarPagina(false);
        if (!string.IsNullOrEmpty(mensagem)) fase.Mensagem(mensagem);
    }

    public void Fechar()
    {
        CancelarColocacao();
        raiz.SetActive(false);
    }

    // Chamado quando o saldo ou o nível de uma arma muda: refaz textos e liga/desliga botões.
    public void Atualizar()
    {
        foreach (var e in compras)
            e.botao.interactable = fase.Saldo >= e.item.preco;

        foreach (var e in melhorias)
        {
            bool pode = fase.PodeMelhorar(e.item);
            e.texto.text = pode
                ? $"Melhorar\n{e.item.nome}\nNv {fase.NivelDaArma(e.item) + 1} · {Regras.FormatarPontos(fase.CustoDaMelhoria(e.item))}"
                : $"{e.item.nome}\nNível máx.";
            e.botao.interactable = pode && fase.Saldo >= fase.CustoDaMelhoria(e.item);
        }
    }

    void MostrarPagina(bool deMelhorias)
    {
        naPaginaDeMelhorias = deMelhorias;
        CancelarColocacao();
        paginaCompras.SetActive(!deMelhorias);
        paginaMelhorias.SetActive(deMelhorias);
        if (textoDaPagina != null) textoDaPagina.text = deMelhorias ? "Compras" : "Melhorias";
        Atualizar();
    }

    void PegarEstiloDoHud()
    {
        var hud = fase.hud;
        fonte = hud != null && hud.textoFase != null ? hud.textoFase.font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (hud == null) return;
        if (hud.botaoMenu != null && hud.botaoMenu.image != null) spriteBotao = hud.botaoMenu.image.sprite;
        if (hud.painelResultado != null)
        {
            var painel = hud.painelResultado.transform.Find("Painel");
            if (painel != null) spritePainel = painel.GetComponent<Image>().sprite;
        }
    }

    void Construir()
    {
        raiz = new GameObject("Loja", typeof(RectTransform));
        raiz.transform.SetParent(transform, false);

        var canvas = raiz.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 15; // acima do HUD (10)
        var escala = raiz.AddComponent<CanvasScaler>();
        escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escala.referenceResolution = new Vector2(600f, 480f);
        escala.matchWidthOrHeight = 0.5f;
        raiz.AddComponent<GraphicRaycaster>();

        // A barra cabe na faixa de terra do chão (os 60 px de baixo), sem esconder a torre.
        var barra = Retangulo(raiz.transform, "Barra", new Vector2(0.5f, 0f), new Vector2(0f, 4f), new Vector2(588f, 52f));
        var fundo = barra.gameObject.AddComponent<Image>();
        if (spritePainel != null) { fundo.sprite = spritePainel; fundo.type = Image.Type.Sliced; }
        else fundo.color = new Color(Marrom.r, Marrom.g, Marrom.b, 0.9f);

        paginaCompras = Pagina(barra, "Compras");
        paginaMelhorias = Pagina(barra, "Melhorias");

        float xCompras = 8f, xMelhorias = 8f;
        bool temArmas = false;
        if (catalogo != null && catalogo.itens != null)
        {
            foreach (var item in catalogo.itens)
            {
                if (item == null) continue;
                var capturado = item;

                var compra = new Entrada { item = item };
                compra.botao = CriarBotao(paginaCompras.transform, item.nome.Replace(' ', '\n') + "\n" + Regras.FormatarPontos(item.preco),
                    new Vector2(0f, 0.5f), new Vector2(xCompras, 0f), new Vector2(LarguraItem, Altura), () => AoComprar(capturado), out _);
                compras.Add(compra);
                xCompras += LarguraItem + Espaco;

                if (item.tipo != TipoItem.Arma) continue;
                temArmas = true;
                var melhoria = new Entrada { item = item };
                melhoria.botao = CriarBotao(paginaMelhorias.transform, "Melhorar",
                    new Vector2(0f, 0.5f), new Vector2(xMelhorias, 0f), new Vector2(LarguraItem, Altura), () => AoMelhorar(capturado), out melhoria.texto);
                melhorias.Add(melhoria);
                xMelhorias += LarguraItem + Espaco;
            }
        }

        CriarBotao(barra, "Iniciar leva", new Vector2(1f, 0.5f), new Vector2(-8f, 0f), new Vector2(84f, Altura), AoIniciar, out _);

        // Só aparece se a loja vende alguma arma (senão não há o que melhorar).
        if (temArmas)
            CriarBotao(barra, "Melhorias", new Vector2(1f, 0.5f), new Vector2(-96f, 0f), new Vector2(68f, Altura), AoTrocarPagina, out textoDaPagina);
    }

    GameObject Pagina(RectTransform pai, string nome)
    {
        var rt = Retangulo(pai, nome, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return rt.gameObject;
    }

    void AoIniciar()
    {
        Clicou();
        fase.IniciarLeva();
    }

    void AoTrocarPagina()
    {
        Clicou();
        MostrarPagina(!naPaginaDeMelhorias);
        fase.Mensagem(naPaginaDeMelhorias ? "Melhorias: sobem o nível de todas as armas do tipo." : "Compras: escolha um item.");
    }

    void AoComprar(ItemLoja item)
    {
        Clicou();
        fase.Mensagem(Comprar(item));
        Atualizar();
    }

    void AoMelhorar(ItemLoja item)
    {
        Clicou();
        fase.Mensagem(fase.MelhorarArma(item)
            ? $"{item.nome} melhorado: nível {fase.NivelDaArma(item)}!"
            : "Saldo insuficiente.");
        Atualizar();
    }

    static void Clicou()
    {
        // Sem isto o botão fica "selecionado" e a barra de espaço (ou o Enter) o apertaria de novo.
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        Sons.Tocar(Sons.I != null ? Sons.I.clique : null);
    }

    // Faz a compra e devolve a mensagem para o jogador.
    string Comprar(ItemLoja item)
    {
        if (fase.Saldo < item.preco) return "Saldo insuficiente.";

        switch (item.tipo)
        {
            case TipoItem.Reparo:
                if (!fase.TorreDanificada()) return "A torre não tem danos para reparar.";
                fase.TentarGastar(item.preco);
                return $"Torre reparada ({fase.RepararTorre()} peças)!";

            case TipoItem.Muro:
            case TipoItem.Arma:
                // Clicar de novo no mesmo item larga ele; o pagamento é feito ao colocar.
                if (colocador.Ativo && colocador.ItemAtual == item)
                {
                    colocador.Cancelar();
                    return "Colocação cancelada.";
                }
                colocador.Comecar(item);
                return "Clique no chão para colocar (clique direito cancela).";
        }
        return "";
    }

    // ---- montagem da interface ----

    RectTransform Retangulo(Transform pai, string nome, Vector2 ancora, Vector2 posicao, Vector2 tamanho)
    {
        var go = new GameObject(nome, typeof(RectTransform));
        go.transform.SetParent(pai, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = ancora;
        rt.anchoredPosition = posicao;
        rt.sizeDelta = tamanho;
        return rt;
    }

    Button CriarBotao(Transform pai, string texto, Vector2 ancora, Vector2 posicao, Vector2 tamanho,
        UnityEngine.Events.UnityAction aoClicar, out Text textoDoBotao)
    {
        var rt = Retangulo(pai, "Botao " + texto.Replace("\n", " "), ancora, posicao, tamanho);
        var imagem = rt.gameObject.AddComponent<Image>();
        if (spriteBotao != null) { imagem.sprite = spriteBotao; imagem.type = Image.Type.Sliced; }
        else imagem.color = Creme;

        var botao = rt.gameObject.AddComponent<Button>();
        botao.targetGraphic = imagem;
        var cores = botao.colors;
        cores.highlightedColor = new Color(1f, 0.93f, 0.8f);
        cores.pressedColor = new Color(0.8f, 0.68f, 0.55f);
        cores.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.75f);
        botao.colors = cores;
        botao.onClick.AddListener(aoClicar);

        var t = Retangulo(rt, "Texto", new Vector2(0.5f, 0.5f), Vector2.zero, tamanho).gameObject.AddComponent<Text>();
        t.font = fonte;
        t.text = texto;
        t.fontSize = 10; // cabem até 3 linhas (nome em 2 palavras + preço) nos 44 px do botão
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Marrom;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        textoDoBotao = t;
        return botao;
    }
}
