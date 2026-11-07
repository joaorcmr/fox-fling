using UnityEngine;
using UnityEngine.UI;

// A barra lateral e os avisos por cima do mapa. Ela só LÊ a simulação a cada
// quadro (é barato e a tela nunca fica fora de sincronia com o jogo) e escuta
// alguns eventos para anunciar coisas (onda começou, chefe chegou).
public class Hud : MonoBehaviour
{
    static readonly Color CorOnda = new Color(0.32f, 0.55f, 0.25f, 0.95f);
    static readonly Color CorChefe = new Color(0.72f, 0.2f, 0.16f, 0.95f);

    public Jogo jogo;
    public Controles controles;

    [Header("Topo")]
    public Text textoVida;
    public Text textoPenas;
    public Text textoOnda;
    public Button botaoPausa;
    public Image iconePausa;
    public Sprite spritePausa;
    public Sprite spritePlay;
    public Button botaoVelocidade;
    public Text textoVelocidade;
    public Button botaoMenu;

    [Header("Loja (some quando um porco está selecionado)")]
    public GameObject grupoLoja;
    public CartaoPorcoLoja[] cartoes;
    public CartaoNinho cartaoNinho;
    public PreviaOnda previa;
    public PainelPorco painelPorco;

    [Header("Onda")]
    public Button botaoAuto;
    public Text textoAuto;
    public Button botaoSoltar;
    public Text textoSoltar;

    [Header("Por cima do mapa")]
    public Faixa faixa;
    public BarraChefe barraChefe;
    public Text textoAviso;
    public MenuPausa menuPausa;

    float fimAviso;

    void Start()
    {
        var s = jogo.Sim;
        foreach (var c in cartoes) c.Configurar(jogo, controles);
        botaoSoltar.onClick.AddListener(() => s.SoltarOnda());
        botaoAuto.onClick.AddListener(() => s.Auto = !s.Auto);
        botaoPausa.onClick.AddListener(() => jogo.AlternarPausa());
        botaoVelocidade.onClick.AddListener(() => jogo.ProximaVelocidade());
        botaoMenu.onClick.AddListener(() => menuPausa.Abrir());

        controles.SelecaoMudou += p =>
        {
            if (p != null) painelPorco.Mostrar(p);
            else painelPorco.Esconder();
            grupoLoja.SetActive(p == null);
        };
        controles.Falhou += r =>
        {
            if (r == Resultado.SemPenas) Avisar("Faltam penas");
            else if (r == Resultado.LugarInvalido) Avisar("Não dá para colocar aqui");
        };

        s.OndaComecou += n => faixa.Mostrar("Onda " + n, CorOnda, 1.6f);
        s.OndaTerminou += (n, bonus) => Avisar($"Onda {n} vencida! +{bonus} penas de bônus");
        s.PassaroNasceu += p =>
        {
            if (!p.Dados.chefe) return;
            faixa.Mostrar("CHEFE: " + p.Dados.nome, CorChefe, 2.4f);
            barraChefe.Seguir(p);
        };

        painelPorco.Esconder();
        textoAviso.text = "";
    }

    public void Avisar(string texto)
    {
        textoAviso.text = texto;
        fimAviso = Time.unscaledTime + 1.8f;
    }

    void Update()
    {
        var s = jogo.Sim;
        textoVida.text = s.Vida.ToString();
        textoPenas.text = s.Penas.ToString();
        int numero = s.Estado == EstadoJogo.EmOnda ? s.Onda : Mathf.Min(s.Onda + 1, s.TotalOndas);
        textoOnda.text = $"Onda {numero}/{s.TotalOndas}";

        iconePausa.sprite = jogo.Pausado ? spritePlay : spritePausa;
        textoVelocidade.text = jogo.Velocidade + "×";
        textoAuto.text = s.Auto ? "Auto: ligado" : "Auto: desligado";

        bool podeSoltar = s.Estado == EstadoJogo.Preparando || s.Estado == EstadoJogo.Intervalo;
        botaoSoltar.interactable = podeSoltar;
        textoSoltar.text = podeSoltar ? "Soltar a revoada" : s.Estado == EstadoJogo.EmOnda ? "Onda em andamento" : "Fim de jogo";

        if (textoAviso.text.Length > 0 && Time.unscaledTime > fimAviso) textoAviso.text = "";
    }
}
