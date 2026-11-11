using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// A barra lateral e os avisos por cima do mapa. Ela só LÊ a simulação a cada
// quadro (é barato e a tela nunca fica fora de sincronia com o jogo) e escuta
// alguns eventos para anunciar coisas (onda começou, chefe chegou, pássaro
// novo). O "Soltar a onda" pulsa sempre que dá para soltar a onda — não só
// depois de uma troca de fase —, pra ficar óbvio que é a vez do jogador agir;
// o pulso fica mais forte logo depois de uma troca de fase, quando o mapa é
// novo e vale a pena olhar antes de soltar.
public class Hud : MonoBehaviour
{
    static readonly Color CorOnda = new Color(0.32f, 0.55f, 0.25f, 0.95f);
    static readonly Color CorChefe = new Color(0.72f, 0.2f, 0.16f, 0.95f);
    static readonly Color CorNovo = new Color(0.27f, 0.42f, 0.74f, 0.95f);

    public Jogo jogo;

    [Header("Topo")]
    public Image iconeVida;
    public Text textoVida;
    public Text textoMoedas;
    public Text textoOnda;
    public Text textoFase;
    public Button botaoVelocidade;
    public Text textoVelocidade;
    public Button botaoMenu;

    [Header("Loja (some quando uma raposa está selecionada)")]
    public GameObject grupoLoja;
    public CartaoPorcoLoja[] cartoes;
    public CartaoReparo cartaoReparo;
    public Button botaoReparo;
    public CartaoCofrinho cartaoCofrinho;
    public PreviaOnda previa;
    public PainelPorco painelPorco;

    [Header("Onda")]
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
    bool chamarSoltar;   // mapa novo: o "Soltar a onda" pulsa até a próxima onda
    Color corTexto;

    void Start()
    {
        var s = jogo.Sim;
        foreach (var c in cartoes) c.Configurar(jogo);
        botaoSoltar.onClick.AddListener(() => s.SoltarOnda());
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

        s.OndaComecou += n =>
        {
            faixa.Mostrar("Onda " + n, CorOnda, 1.6f);
            chamarSoltar = false;
            var novos = s.Estreias(n);
            if (novos.Count > 0 && n > 1) StartCoroutine(AnunciarNovo(novos[0])); // na onda 1 tudo é novo
        };
        s.FaseComecou += fase => chamarSoltar = true;
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

    // Espera a faixa "Onda N" sumir (1,6 s + a saída) antes de anunciar o
    // pássaro novo, senão uma troca por cima da outra e ninguém lê nenhuma.
    IEnumerator AnunciarNovo(EspeciePassaro especie)
    {
        yield return new WaitForSecondsRealtime(1.9f);
        faixa.Mostrar("Novo: " + jogo.Sim.Dados.passaros[especie].nome, CorNovo, 2.2f);
    }

    public void Avisar(string texto, float duracao = 1.8f)
    {
        textoAviso.text = texto;
        fimAviso = Time.unscaledTime + duracao;
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
        textoFase.text = s.TotalFases > 1 ? $"Fase {s.Fase}/{s.TotalFases}" : "";

        textoVelocidade.text = jogo.Relogio.Velocidade + "×";

        bool podeSoltar = s.Estado == EstadoJogo.Preparando || s.Estado == EstadoJogo.Intervalo;
        botaoSoltar.interactable = podeSoltar;
        textoSoltar.text = podeSoltar ? "Soltar a onda" : s.Estado == EstadoJogo.EmOnda ? "Onda em andamento" : "Fim de jogo";
        // Só o botão crescendo um pouco (0,035) era sutil demais pra quem não
        // está de olho nele. Agora também BRILHA (o tint vai de branco a um
        // dourado bem mais claro que o verde do botão) — cor chama mais
        // atenção no canto do olho do que só o tamanho mudando.
        float pulso01 = podeSoltar ? Mathf.Abs(Mathf.Sin(Time.unscaledTime * (chamarSoltar ? 4.5f : 2.8f))) : 0f;
        float amplitudeEscala = podeSoltar ? (chamarSoltar ? 0.11f : 0.07f) : 0f;
        botaoSoltar.transform.localScale = Vector3.one * (1f + amplitudeEscala * pulso01);
        if (botaoSoltar.image != null)
            botaoSoltar.image.color = podeSoltar ? Color.Lerp(Color.white, new Color(1.5f, 1.3f, 0.55f), pulso01) : Color.white;

        if (textoAviso.text.Length > 0 && Time.unscaledTime > fimAviso) textoAviso.text = "";
    }
}
