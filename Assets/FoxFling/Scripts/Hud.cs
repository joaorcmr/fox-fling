using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Interface da fase: nome, pontos, dicas, botões e a tela de resultado com estrelas.
// Teclas: R reinicia, Esc volta ao menu.
public class Hud : MonoBehaviour
{
    public Text textoFase, textoPontos, textoDica;
    public Button botaoMenu, botaoReiniciar;

    [Header("Resultado")]
    public GameObject painelResultado;
    public Text tituloResultado, pontosResultado;
    public Image[] estrelas;
    public Sprite estrelaCheia, estrelaVazia;
    public Button botaoMenuResultado, botaoRepetir, botaoProxima;

    void Start()
    {
        painelResultado.SetActive(false);
        Ligar(botaoMenu, f => f.IrParaMenu());
        Ligar(botaoReiniciar, f => f.Reiniciar());
        Ligar(botaoMenuResultado, f => f.IrParaMenu());
        Ligar(botaoRepetir, f => f.Reiniciar());
        Ligar(botaoProxima, f => f.ProximaFase());
    }

    static void Ligar(Button botao, System.Action<GerenciadorFase> acao)
    {
        botao.onClick.AddListener(() =>
        {
            Sons.Tocar(Sons.I != null ? Sons.I.clique : null);
            if (GerenciadorFase.Instance != null) acao(GerenciadorFase.Instance);
        });
    }

    void Update()
    {
        var fase = GerenciadorFase.Instance;
        if (fase == null) return;
        if (Input.GetKeyDown(KeyCode.R)) fase.Reiniciar();
        else if (Input.GetKeyDown(KeyCode.Escape)) fase.IrParaMenu();
    }

    public void Configurar(string nomeFase, string dificuldade)
    {
        textoFase.text = $"{nomeFase} · {dificuldade}";
    }

    public void AtualizarPontos(int pontos)
    {
        textoPontos.text = "Pontos: " + Regras.FormatarPontos(pontos);
    }

    public void MostrarDica(string texto)
    {
        textoDica.text = texto;
        textoDica.enabled = !string.IsNullOrEmpty(texto);
    }

    public void MostrarResultado(bool venceu, int pontos, int quantasEstrelas, bool temProxima)
    {
        MostrarDica("");
        StartCoroutine(AnimarResultado(venceu, pontos, quantasEstrelas, temProxima));
    }

    IEnumerator AnimarResultado(bool venceu, int pontos, int quantasEstrelas, bool temProxima)
    {
        painelResultado.SetActive(true);
        tituloResultado.text = venceu ? "A torre resistiu!" : "Os tubarões caíram!";
        botaoProxima.gameObject.SetActive(venceu && temProxima);
        foreach (var e in estrelas) e.sprite = estrelaVazia;

        // Pontos contando de 0 até o total.
        const float duracao = 0.8f;
        for (float t = 0f; t < duracao; t += Time.unscaledDeltaTime)
        {
            pontosResultado.text = Regras.FormatarPontos(Mathf.RoundToInt(pontos * t / duracao)) + " pontos";
            yield return null;
        }
        pontosResultado.text = Regras.FormatarPontos(pontos) + " pontos";

        for (int i = 0; i < quantasEstrelas && i < estrelas.Length; i++)
        {
            yield return new WaitForSecondsRealtime(0.3f);
            estrelas[i].sprite = estrelaCheia;
            Sons.Tocar(Sons.I != null ? Sons.I.estrela : null, 0.8f, 0f);
            StartCoroutine(Pulo(estrelas[i].transform));
        }
    }

    static IEnumerator Pulo(Transform alvo)
    {
        for (float t = 0f; t < 0.25f; t += Time.unscaledDeltaTime)
        {
            float k = t / 0.25f;
            alvo.localScale = Vector3.one * (1f + 0.35f * Mathf.Sin(k * Mathf.PI));
            yield return null;
        }
        alvo.localScale = Vector3.one;
    }
}
