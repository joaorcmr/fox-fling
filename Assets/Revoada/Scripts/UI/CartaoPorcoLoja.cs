using UnityEngine;
using UnityEngine.UI;

// Um porco da loja na barra lateral: ícone e preço. Fica apagado quando faltam
// penas; clicar começa a colocação (o fantasma aparece no mapa).
public class CartaoPorcoLoja : MonoBehaviour
{
    public TipoPorco tipo;
    public Button botao;
    public Image fundo;
    public Image icone;
    public Text preco;
    public CanvasGroup grupo;

    Jogo jogo;
    Controles controles;

    public void Configurar(Jogo novoJogo, Controles novosControles)
    {
        jogo = novoJogo;
        controles = novosControles;
        botao.onClick.AddListener(() => controles.IniciarColocacao(tipo.dados.especie));
    }

    void Update()
    {
        if (jogo == null) return;
        bool temPenas = jogo.Sim.Penas >= tipo.dados.custo;
        grupo.alpha = temPenas ? 1f : 0.45f;
        bool escolhido = controles.Colocando.HasValue && controles.Colocando.Value == tipo.dados.especie;
        fundo.color = escolhido ? new Color(1f, 0.92f, 0.55f) : Color.white;
    }
}
