using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Um porco da loja na barra lateral: ícone e preço. Fica apagado quando faltam
// moedas; clicar começa a colocação (o fantasma aparece no mapa). Passar o
// mouse mostra o balão com o nome e o papel do porco.
public class CartaoPorcoLoja : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TipoPorco tipo;
    public Button botao;
    public Image fundo;
    public Image icone;
    public Text preco;
    public CanvasGroup grupo;
    public BalaoLoja balao;

    Jogo jogo;

    public void Configurar(Jogo novoJogo)
    {
        jogo = novoJogo;
        botao.onClick.AddListener(() => jogo.Selecao.IniciarColocacao(tipo.dados.especie));
    }

    void Update()
    {
        if (jogo == null) return;
        bool emOnda = jogo.Sim.Estado == EstadoJogo.EmOnda;
        bool temMoedas = jogo.Sim.Moedas >= tipo.dados.custo;
        grupo.alpha = emOnda ? 0.3f : temMoedas ? 1f : 0.45f;
        var naMao = jogo.Selecao.Colocando;
        bool escolhido = naMao.HasValue && naMao.Value == tipo.dados.especie;
        fundo.color = escolhido ? new Color(1f, 0.92f, 0.55f) : Color.white;
    }

    public void OnPointerEnter(PointerEventData e)
    {
        if (balao != null) balao.Mostrar(tipo, (RectTransform)transform);
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (balao != null) balao.Esconder();
    }

    void OnDisable()
    {
        if (balao != null) balao.Esconder();
    }
}
