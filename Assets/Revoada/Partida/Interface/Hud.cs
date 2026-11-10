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

    [Header("Topo")]
    public Image iconeVida;
    public Text textoVida;
    public Text textoMoedas;
    public Text textoOnda;
    public Button botaoVelocidade;
    public Text textoVelocidade;
    public Button botaoMenu;
    public Button botaoReparo;

    [Header("Loja (some quando um porco está selecionado)")]
    public GameObject grupoLoja;
    public CartaoPorcoLoja[] cartoes;
    public CartaoCofrinho cartaoCofrinho;
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
    float moedasNaTela;   // conta até o valor de verdade em vez de pular (número que anima)
    float pulsoVida;     // 1 logo depois de a Torre apanhar, cai até 0
    Color corTexto;

    void Start()
    {
        var s = jogo.Sim;
        foreach (var c in cartoes) c.Configurar(jogo);
        botaoSoltar.onClick.AddListener(() => s.SoltarOnda());
        botaoAuto.onClick.AddListener(() => s.Auto = !s.Auto);
        botaoVelocidade.onClick.AddListener(() => jogo.Relogio.ProximaVelocidade());
        botaoMenu.onClick.AddListener(() => menuPausa.Abrir());

        jogo.Selecao.SelecaoMudou += p =>
        {
            if (p != null) painelPorco.Mostrar(p);
            else painelPorco.Esconder();
            grupoLoja.SetActive(p == null);
        };
        jogo.Selecao.Falhou += r =>
        {
            if (r == Resultado.SemMoedas) Avisar("Faltam moedas");
            else if (r == Resultado.LugarInvalido) Avisar("Não dá para colocar aqui");
            else if (r == Resultado.OndaEmAndamento) Avisar("Espere a onda terminar");
            if (Sons.Instancia != null) Sons.Tocar(Sons.Instancia.erro, 0.6f, 0f);
        };
        s.TorreAtingida += dano => pulsoVida = 1f;
        // Jogo acabado: pausar, acelerar e abrir o menu de pausa não fazem mais sentido.
        s.FimDeJogo += venceu => botaoVelocidade.interactable = botaoMenu.interactable = false;
        moedasNaTela = s.Moedas;
        corTexto = textoVida.color;

        s.OndaComecou += n => faixa.Mostrar("Onda " + n, CorOnda, 1.6f);
        s.OndaTerminou += (n, bonus) => Avisar($"Onda {n} vencida! +{bonus} moedas de bônus");
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
        float dt = Time.unscaledDeltaTime;

        // Vida: pulsa vermelha quando a Torre apanha; abaixo de 25, o coração pisca.
        textoVida.text = s.Vida.ToString();
        pulsoVida = Mathf.MoveTowards(pulsoVida, 0f, dt * 3f);
        textoVida.transform.localScale = Vector3.one * (1f + 0.35f * pulsoVida);
        bool perigo = s.Vida < 25 && s.Vida > 0;
        textoVida.color = Color.Lerp(corTexto, new Color(0.8f, 0.15f, 0.1f), Mathf.Max(pulsoVida, perigo ? 1f : 0f));
        iconeVida.transform.localScale = Vector3.one * (perigo ? 1f + 0.15f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * 6f)) : 1f);

        // Moedas: o contador corre até o valor novo.
        moedasNaTela = Mathf.Abs(s.Moedas - moedasNaTela) < 0.5f ? s.Moedas : Mathf.Lerp(moedasNaTela, s.Moedas, 1f - Mathf.Exp(-12f * dt));
        textoMoedas.text = Mathf.RoundToInt(moedasNaTela).ToString();
        // Entre ondas mostra a próxima; durante a onda e na derrota, a atual (a
        // mesma que a tela de fim diz que derrubou a Torre).
        bool ondaAtual = s.Estado == EstadoJogo.EmOnda || s.Estado == EstadoJogo.Derrota;
        int numero = ondaAtual ? s.Onda : Mathf.Min(s.Onda + 1, s.TotalOndas);
        textoOnda.text = $"Onda {numero}/{s.TotalOndas}";

        textoVelocidade.text = jogo.Relogio.Velocidade + "×";
        textoAuto.text = s.Auto ? "Auto: ligado" : "Auto: desligado";

        bool podeSoltar = s.Estado == EstadoJogo.Preparando || s.Estado == EstadoJogo.Intervalo;
        botaoSoltar.interactable = podeSoltar;
        textoSoltar.text = podeSoltar ? "Soltar a onda" : s.Estado == EstadoJogo.EmOnda ? "Onda em andamento" : "Fim de jogo";

        if (textoAviso.text.Length > 0 && Time.unscaledTime > fimAviso) textoAviso.text = "";
    }
}
