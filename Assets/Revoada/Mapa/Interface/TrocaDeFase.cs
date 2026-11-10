using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// A troca de fase como um MOMENTO claro, para o jogador não se perder nem
// ficar confuso:
//   1. os porcos viram moedas, um por um, com o "+N" de cada (GerenteVisuais);
//   2. o mapa escurece e, por trás, o mapa novo é desenhado (VisualMapa);
//   3. um quadro anuncia a fase, o lugar e a hora do dia, o caminho novo e
//      quantas moedas voltaram para a carteira, e espera o jogador;
//   4. ao fechar, a tela clareia e o caminho novo se desenha com setas, da
//      entrada até a Torre (RevelarTrilha); o "Soltar a onda" fica pulsando
//      até a próxima onda (Hud).
// Do começo até a tela clarear, o mapa e a barra não recebem cliques e os
// Controles ficam parados (Ativa). Tudo em tempo sem escala.
public class TrocaDeFase : MonoBehaviour
{
    const float EsperaPorcos = 1.1f;   // deixa ver os porcos virando moedas
    const float DuracaoEscurecer = 0.45f;
    const float DuracaoClarear = 0.5f;
    const float OpacidadeCortina = 0.94f;
    const float OpacidadeVeuBarra = 0.45f;

    public Jogo jogo;
    public Hud hud;
    public VisualMapa mapa;
    public RevelarTrilha revelar;

    [Header("Tela")]
    [Tooltip("Bloqueia os cliques no mapa e na barra enquanto a troca acontece.")]
    public CanvasGroup grupo;
    public Image cortina;
    public Image veuBarra;
    public RectTransform quadro;
    public CanvasGroup grupoQuadro;
    public Text titulo;
    public Text lugar;
    public Text detalhes;
    public Image[] pontosFase;
    public Sprite pontoCheio, pontoVazio;
    public Button botaoContinuar;

    public bool Ativa { get; private set; }

    int porcosRecolhidos, moedasRecolhidas;
    bool esperandoJogador, fechar;

    void Start()
    {
        Esconder();
        var s = jogo.Sim;
        s.PorcosRecolhidos += (porcos, moedas) =>
        {
            porcosRecolhidos = porcos.Count;
            moedasRecolhidas = moedas;
        };
        s.FaseComecou += fase =>
        {
            StopAllCoroutines();
            StartCoroutine(Mostrar(fase));
        };
        botaoContinuar.onClick.AddListener(() => fechar = true);
    }

    void Update()
    {
        if (!esperandoJogador) return;
        // O teclado também fecha (os Controles estão parados durante a troca).
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) ||
            Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
            fechar = true;
        botaoContinuar.transform.localScale = Vector3.one * (1f + 0.04f * Mathf.Sin(Time.unscaledTime * 4f));
    }

    IEnumerator Mostrar(int fase)
    {
        Ativa = true;
        fechar = false;
        grupo.blocksRaycasts = true;
        quadro.gameObject.SetActive(false);

        yield return new WaitForSecondsRealtime(porcosRecolhidos > 0 ? EsperaPorcos : 0.4f);

        yield return Escurecer(0f, 1f, DuracaoEscurecer);
        mapa.MostrarFase(fase); // por trás da cortina: o jogador não vê o mapa trocando
        Preencher(fase);
        porcosRecolhidos = moedasRecolhidas = 0;

        if (Sons.Instancia != null) Sons.Tocar(Sons.Instancia.fase, 1f, 0f);
        quadro.gameObject.SetActive(true);
        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.35f)
        {
            grupoQuadro.alpha = Mathf.Clamp01(t * 2f);
            quadro.localScale = Vector3.one * Curvas.VoltaComQuique(t);
            yield return null;
        }
        grupoQuadro.alpha = 1f;
        quadro.localScale = Vector3.one;

        esperandoJogador = true;
        while (!fechar) yield return null;
        esperandoJogador = false;
        botaoContinuar.transform.localScale = Vector3.one;

        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.2f)
        {
            grupoQuadro.alpha = 1f - t;
            quadro.localScale = Vector3.one * (1f - 0.1f * t);
            yield return null;
        }
        quadro.gameObject.SetActive(false);

        // O caminho começa a se desenhar enquanto a tela clareia.
        var s = jogo.Sim;
        if (revelar != null) revelar.Revelar(s.Trilha, s.Dados.mapa);
        yield return Escurecer(1f, 0f, DuracaoClarear);
        Esconder();
        if (hud != null) hud.Avisar("Caminho novo! Monte a defesa e solte a onda.", 5f);
    }

    void Preencher(int fase)
    {
        var s = jogo.Sim;
        var d = s.Dados.mapa;
        titulo.text = $"Fase {fase} de {s.TotalFases}";
        lugar.text = $"{d.nome}, {Turno(d.turno)}";
        string porcos = porcosRecolhidos == 0
            ? "• Você não tinha porcos no mapa: as moedas ficam como estavam."
            : porcosRecolhidos == 1
                ? $"• Seu porco voltou para a carteira: <b>+{moedasRecolhidas} moedas</b>."
                : $"• Seus {porcosRecolhidos} porcos voltaram para a carteira: <b>+{moedasRecolhidas} moedas</b>.";
        detalhes.text =
            "• Os pássaros vêm por um <b>caminho novo</b>: siga as setas.\n" +
            porcos + "\n" +
            "• Monte a defesa de novo e solte a onda quando estiver pronto.";
        for (int i = 0; i < pontosFase.Length; i++)
        {
            pontosFase[i].enabled = i < s.TotalFases;
            pontosFase[i].sprite = i < fase ? pontoCheio : pontoVazio;
        }
    }

    static string Turno(TurnoDoDia t) => t == TurnoDoDia.Tarde ? "à tarde" : t == TurnoDoDia.Noite ? "à noite" : "de dia";

    IEnumerator Escurecer(float de, float para, float duracao)
    {
        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / duracao)
        {
            Pintar(Mathf.Lerp(de, para, t));
            yield return null;
        }
        Pintar(para);
    }

    void Pintar(float k)
    {
        var c = cortina.color;
        cortina.color = new Color(c.r, c.g, c.b, OpacidadeCortina * k);
        var v = veuBarra.color;
        veuBarra.color = new Color(v.r, v.g, v.b, OpacidadeVeuBarra * k);
    }

    void Esconder()
    {
        Ativa = false;
        esperandoJogador = false;
        grupo.blocksRaycasts = false;
        Pintar(0f);
        quadro.gameObject.SetActive(false);
    }
}
