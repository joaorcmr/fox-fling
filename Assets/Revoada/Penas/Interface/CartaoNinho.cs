using UnityEngine;
using UnityEngine.UI;
using static Formato;

// O Ninho de Penas: comprar faz cada pássaro estourado render mais. O cartão
// mostra o multiplicador atual → o próximo, o custo e EM QUANTAS ONDAS o
// investimento se paga. É a informação que transforma "income ou defesa?" numa
// decisão que dá para pensar, e não um chute.
public class CartaoNinho : MonoBehaviour
{
    public Jogo jogo;
    public Button botao;
    public Text multiplicador;
    public Text custo;
    public Text retorno;
    public CanvasGroup grupo;

    void Awake() => botao.onClick.AddListener(() => jogo.Selecao.Avisar(jogo.Sim.ComprarNinho()));

    void Update()
    {
        var s = jogo.Sim;
        var proximo = s.Economia.ProximoNinho;
        if (proximo == null)
        {
            multiplicador.text = $"×{Num(s.Economia.Multiplicador)} (máximo)";
            custo.text = "—";
            retorno.text = "Ninho completo!";
            botao.interactable = false;
            grupo.alpha = 1f;
            return;
        }

        multiplicador.text = $"×{Num(s.Economia.Multiplicador)} → ×{Num(proximo.multiplicador)}"; // o ícone do ninho já diz o que é
        custo.text = proximo.custo.ToString();
        int? ondas = s.OndasParaPagarNinho();
        retorno.text = ondas.HasValue
            ? $"se paga em ~{ondas} onda{(ondas == 1 ? "" : "s")}"
            : $"não se paga mais até a onda {s.TotalOndas}";
        botao.interactable = true;
        grupo.alpha = s.Penas >= proximo.custo ? 1f : 0.55f;
    }
}
