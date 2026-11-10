using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// O botão de reparar a Torre, na barra: cura uma parte da vida perdida por um
// custo que sobe a cada compra (issue #14). Só funciona entre ondas (ver
// Selecao, que vale a mesma regra para os porcos) e quando a vida não está
// cheia. O balão (o mesmo da loja) mostra o preço, sem precisar de um cartão
// cheio — não sobra espaço na barra para mais um.
public class CartaoReparo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Jogo jogo;
    public Button botao;
    public CanvasGroup grupo;
    public BalaoLoja balao;

    void Awake() => botao.onClick.AddListener(() =>
    {
        var s = jogo.Sim;
        jogo.Selecao.Avisar(s.Estado == EstadoJogo.EmOnda ? Resultado.OndaEmAndamento : s.ComprarReparo());
    });

    void Update()
    {
        var s = jogo.Sim;
        bool cheia = s.Vida >= s.VidaMaxima;
        bool emOnda = s.Estado == EstadoJogo.EmOnda;
        bool podeComprar = !cheia && !emOnda && s.Moedas >= s.Economia.CustoProximoReparo;
        botao.interactable = !cheia && !emOnda;
        grupo.alpha = cheia ? 0.4f : podeComprar ? 1f : 0.55f;
    }

    public void OnPointerEnter(PointerEventData e)
    {
        if (balao == null) return;
        var s = jogo.Sim;
        string texto = s.Vida >= s.VidaMaxima
            ? "A Torre já está com a vida cheia."
            : s.Estado == EstadoJogo.EmOnda
                ? "Espere a onda terminar."
                : $"+{s.Dados.economia.curaPorReparo} de vida. Custo: {s.Economia.CustoProximoReparo} moedas (sobe a cada reparo).";
        balao.Mostrar("Reparar a Torre", texto, (RectTransform)transform);
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
