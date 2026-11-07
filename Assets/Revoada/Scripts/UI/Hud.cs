using UnityEngine;
using UnityEngine.UI;

// A barra lateral: vida da Torre, penas, onda, a loja de porcos e o botão de
// soltar a revoada. Ela só LÊ a simulação a cada quadro (é barato e não deixa
// a tela ficar fora de sincronia com o jogo).
public class Hud : MonoBehaviour
{
    public Jogo jogo;
    public Controles controles;

    [Header("Topo")]
    public Text textoVida;
    public Text textoPenas;
    public Text textoOnda;

    [Header("Loja")]
    public CartaoPorcoLoja[] cartoes;

    [Header("Onda")]
    public Button botaoSoltar;
    public Text textoSoltar;

    void Start()
    {
        foreach (var c in cartoes) c.Configurar(jogo, controles);
        botaoSoltar.onClick.AddListener(() => jogo.Sim.SoltarOnda());
    }

    void Update()
    {
        var s = jogo.Sim;
        textoVida.text = s.Vida.ToString();
        textoPenas.text = s.Penas.ToString();
        int numero = s.Estado == EstadoJogo.EmOnda ? s.Onda : Mathf.Min(s.Onda + 1, s.TotalOndas);
        textoOnda.text = $"Onda {numero}/{s.TotalOndas}";

        bool podeSoltar = s.Estado == EstadoJogo.Preparando || s.Estado == EstadoJogo.Intervalo;
        botaoSoltar.interactable = podeSoltar;
        textoSoltar.text = podeSoltar ? "Soltar a revoada" : s.Estado == EstadoJogo.EmOnda ? "Onda em andamento" : "Fim de jogo";
    }
}
