using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// A loja do tower defense: uma barra no pé da tela (em cima da faixa de terra), aberta só
// entre as levas. Mostra um botão por item do catálogo e o "Iniciar leva". A interface é
// montada aqui, por código, na hora que a fase começa (o GerenciadorFase cria esta loja),
// reaproveitando o estilo dos botões do HUD.
public class Loja : MonoBehaviour
{
    static readonly Color Creme = new Color(1f, 0.965f, 0.88f);
    static readonly Color Marrom = new Color(0.17f, 0.11f, 0.08f);

    class Entrada
    {
        public ItemLoja item;
        public Button botao;
    }

    GerenciadorFase fase;
    CatalogoLoja catalogo;
    Colocador colocador;
    GameObject raiz;
    Font fonte;
    Sprite spriteBotao, spritePainel;
    readonly List<Entrada> entradas = new List<Entrada>();

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
        Atualizar();
        if (!string.IsNullOrEmpty(mensagem)) fase.Mensagem(mensagem);
    }

    public void Fechar()
    {
        CancelarColocacao();
        raiz.SetActive(false);
    }

    // Chamado quando o saldo muda: liga e desliga os botões conforme dá para pagar.
    public void Atualizar()
    {
        foreach (var e in entradas)
            e.botao.interactable = fase.Saldo >= e.item.preco;
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

        const float larguraItem = 76f, espaco = 4f, altura = 44f;
        float x = 8f;
        if (catalogo != null && catalogo.itens != null)
        {
            foreach (var item in catalogo.itens)
            {
                if (item == null) continue;
                var entrada = new Entrada { item = item };
                var capturado = item;
                entrada.botao = CriarBotao(barra, item.nome.Replace(' ', '\n') + "\n" + Regras.FormatarPontos(item.preco),
                    new Vector2(0f, 0.5f), new Vector2(x, 0f), new Vector2(larguraItem, altura), () => AoComprar(capturado));
                entradas.Add(entrada);
                x += larguraItem + espaco;
            }
        }

        CriarBotao(barra, "Iniciar leva", new Vector2(1f, 0.5f), new Vector2(-8f, 0f), new Vector2(104f, altura), AoIniciar);
    }

    void AoIniciar()
    {
        Clicou();
        fase.IniciarLeva();
    }

    void AoComprar(ItemLoja item)
    {
        Clicou();
        fase.Mensagem(Comprar(item));
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

    Button CriarBotao(Transform pai, string texto, Vector2 ancora, Vector2 posicao, Vector2 tamanho, UnityEngine.Events.UnityAction aoClicar)
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
        t.fontSize = 11; // cabem até 3 linhas (nome em 2 palavras + preço) nos 44 px do botão
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Marrom;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return botao;
    }
}
