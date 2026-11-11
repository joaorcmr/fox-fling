using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Formato;

// O Cofrinho: comprar faz cada pássaro estourado render mais. Fica na loja no
// mesmo formato dos porcos e do Reparo (ícone, preço embaixo, balão), com o
// selo do multiplicador que ele passa a dar ("×1,5") e, logo embaixo do
// cartão, EM QUANTAS ONDAS o investimento se paga. É a informação que
// transforma "income ou defesa?" numa decisão que dá para pensar, e não um chute.
public class CartaoCofrinho : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Jogo jogo;
    public Button botao;
    public Text multiplicador;   // o selo no canto: quanto cada pássaro passa a render
    public Text custo;           // o preço embaixo do ícone
    public CanvasGroup grupo;
    public BalaoLoja balao;

    bool mouseEmCima;

    void Awake() => botao.onClick.AddListener(() =>
    {
        var s = jogo.Sim;
        jogo.Selecao.Avisar(s.Estado == EstadoJogo.EmOnda ? Resultado.OndaEmAndamento : s.ComprarCofrinho());
        if (mouseEmCima) MostrarBalao();
    });

    void Update()
    {
        var s = jogo.Sim;
        var proximo = s.Economia.ProximoCofrinho;
        if (proximo == null)
        {
            multiplicador.text = $"×{Num(s.Economia.Multiplicador)}";
            custo.text = "máx.";
            botao.interactable = false;
            grupo.alpha = 1f;
            return;
        }

        bool emOnda = s.Estado == EstadoJogo.EmOnda;
        multiplicador.text = $"×{Num(proximo.multiplicador)}";
        custo.text = proximo.custo.ToString();
        // Clicável sempre, como os porcos: em onda o clique avisa "Espere a onda terminar".
        botao.interactable = true;
        grupo.alpha = emOnda ? 0.3f : s.Moedas >= proximo.custo ? 1f : 0.45f;
    }

    // "Em quantas ondas se paga" já foi um cartão à parte, embaixo deste: dois
    // cartões cor-de-creme colados um no outro, com só uma friesta fina entre
    // eles, pareciam cortados ou um só desenho errado. Juntar a informação no
    // balão (que já abre ao passar o mouse aqui) resolve isso removendo o
    // cartão solto, em vez de ajustar a distância de novo.
    void MostrarBalao()
    {
        if (balao == null) return;
        var s = jogo.Sim;
        var e = s.Economia;
        var proximo = e.ProximoCofrinho;
        if (proximo == null)
        {
            balao.Mostrar("Cofrinho (máximo)", $"Cada inimigo já rende ×{Num(e.Multiplicador)}.", (RectTransform)transform);
            return;
        }
        string retorno = s.Estado == EstadoJogo.EmOnda
            ? " Só dá para comprar entre ondas."
            : s.OndasParaPagarCofrinho() is int ondas
                ? $" Se paga em ~{ondas} onda{(ondas == 1 ? "" : "s")}."
                : " Não se paga mais a tempo.";
        balao.Mostrar($"Cofrinho · {proximo.custo}",
            $"Cada inimigo passa a render ×{Num(proximo.multiplicador)} (agora ×{Num(e.Multiplicador)}).{retorno}",
            (RectTransform)transform);
    }

    public void OnPointerEnter(PointerEventData e)
    {
        mouseEmCima = true;
        MostrarBalao();
    }

    public void OnPointerExit(PointerEventData e)
    {
        mouseEmCima = false;
        if (balao != null) balao.Esconder();
    }

    void OnDisable()
    {
        mouseEmCima = false;
        if (balao != null) balao.Esconder();
    }
}
