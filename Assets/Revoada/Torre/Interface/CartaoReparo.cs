using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// O Reparo da Torre na loja, no MESMO formato dos porcos (CartaoPorcoLoja):
// ícone grande, a moeda com o preço embaixo, apagado quando não dá para
// comprar e o balão ao passar o mouse. O selo "+20" no canto diz quanta vida
// volta, e o preço sobe a cada reparo (issue #14). Quando a Torre está
// machucada e dá para pagar, o coração bate — é a hora em que o jogador
// precisa lembrar que ele existe.
public class CartaoReparo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Jogo jogo;
    public Button botao;
    public RectTransform icone;
    public Text preco;
    public Text cura;
    public CanvasGroup grupo;
    public BalaoLoja balao;

    bool mouseEmCima;

    void Awake() => botao.onClick.AddListener(() =>
    {
        // Igual aos porcos: o clique sempre responde, e quando não dá, avisa o porquê.
        var s = jogo.Sim;
        jogo.Selecao.Avisar(s.Estado == EstadoJogo.EmOnda ? Resultado.OndaEmAndamento : s.ComprarReparo());
        if (mouseEmCima) MostrarBalao(); // o preço do próximo já mudou
    });

    void Update()
    {
        var s = jogo.Sim;
        bool cheia = s.Vida >= s.VidaMaxima;
        bool emOnda = s.Estado == EstadoJogo.EmOnda;
        int custo = s.Economia.CustoProximoReparo;
        bool temMoedas = s.Moedas >= custo;

        preco.text = custo.ToString();
        cura.text = "+" + s.Dados.economia.curaPorReparo;
        // Os mesmos apagados da loja: em onda 0,3; sem moedas 0,45. Vida cheia
        // conta como "agora não".
        grupo.alpha = emOnda || cheia ? 0.3f : temMoedas ? 1f : 0.45f;

        // Coração batendo (duas batidas e uma pausa) com a Torre abaixo da metade.
        bool chamar = !emOnda && !cheia && temMoedas && s.Vida * 2 <= s.VidaMaxima;
        float t = Time.unscaledTime % 1.1f;
        float batida = chamar ? Mathf.Max(Batida(t, 0f), Batida(t, 0.22f)) : 0f;
        icone.localScale = Vector3.one * (1f + 0.16f * batida);
    }

    static float Batida(float t, float inicio)
    {
        float x = (t - inicio) / 0.18f;
        return x < 0f || x > 1f ? 0f : Mathf.Sin(x * Mathf.PI);
    }

    void MostrarBalao()
    {
        if (balao == null) return;
        var s = jogo.Sim;
        int custo = s.Economia.CustoProximoReparo;
        string texto = s.Vida >= s.VidaMaxima
            ? "A Torre já está com a vida cheia."
            : s.Estado == EstadoJogo.EmOnda
                ? "Espere a onda terminar."
                : $"+{s.Dados.economia.curaPorReparo} de vida para a Torre. O preço sobe a cada reparo.";
        balao.Mostrar($"Reparar a Torre · {custo}", texto, (RectTransform)transform);
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
