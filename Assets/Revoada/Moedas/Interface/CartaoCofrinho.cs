using UnityEngine;
using UnityEngine.UI;
using static Formato;

// O Cofrinho: comprar faz cada pássaro estourado render mais. O cartão
// mostra o multiplicador atual → o próximo, o custo e EM QUANTAS ONDAS o
// investimento se paga. É a informação que transforma "income ou defesa?" numa
// decisão que dá para pensar, e não um chute.
public class CartaoCofrinho : MonoBehaviour
{
    public Jogo jogo;
    public Button botao;
    public Text multiplicador;
    public Text custo;
    public Text retorno;
    public CanvasGroup grupo;

    void Awake() => botao.onClick.AddListener(() =>
    {
        var s = jogo.Sim;
        jogo.Selecao.Avisar(s.Estado == EstadoJogo.EmOnda ? Resultado.OndaEmAndamento : s.ComprarCofrinho());
    });

    void Update()
    {
        var s = jogo.Sim;
        var proximo = s.Economia.ProximoCofrinho;
        if (proximo == null)
        {
            multiplicador.text = $"×{Num(s.Economia.Multiplicador)} (máximo)";
            custo.text = "—";
            retorno.text = "Cofrinho completo!";
            botao.interactable = false;
            grupo.alpha = 1f;
            return;
        }

        bool emOnda = s.Estado == EstadoJogo.EmOnda;
        multiplicador.text = $"×{Num(s.Economia.Multiplicador)} → ×{Num(proximo.multiplicador)}"; // o ícone do cofrinho já diz o que é
        custo.text = proximo.custo.ToString();
        int? ondas = s.OndasParaPagarCofrinho();
        retorno.text = emOnda
            ? "espere a onda terminar"
            : ondas.HasValue
                ? $"se paga em ~{ondas} onda{(ondas == 1 ? "" : "s")}"
                : $"não se paga mais até a onda {s.TotalOndas}";
        botao.interactable = !emOnda;
        grupo.alpha = emOnda ? 0.4f : s.Moedas >= proximo.custo ? 1f : 0.55f;
    }
}
