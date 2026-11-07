using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// O fim de jogo como um MOMENTO, não só um texto: clarão, som, o painel
// entrando com um "quique" (OutBack), chuva de penas na vitória, e os números
// da partida (onda, vida, recorde). Tudo em tempo sem escala.
public class TelaFim : MonoBehaviour
{
    public Jogo jogo;
    public Efeitos efeitos;
    public CanvasGroup grupo;
    public RectTransform painel;
    public Image clarao;
    public Text titulo;
    public Text detalhes;
    public Text recorde;
    public Image medalha;
    public Sprite medalhaBronze, medalhaPrata, medalhaOuro;
    public Button botaoDeNovo;
    public Button botaoMenu;

    static readonly Color[] CoresConfete =
    {
        new Color(0.95f, 0.76f, 0.31f), new Color(0.78f, 0.27f, 0.23f), new Color(0.24f, 0.56f, 0.88f), new Color(0.55f, 0.8f, 0.37f),
    };

    void Start()
    {
        grupo.alpha = 0f;
        grupo.blocksRaycasts = false;
        clarao.color = new Color(1f, 0.95f, 0.75f, 0f);
        botaoDeNovo.onClick.AddListener(() => jogo.Reiniciar());
        botaoMenu.onClick.AddListener(() => jogo.IrParaMenu());
        jogo.Sim.FimDeJogo += venceu => StartCoroutine(Mostrar(venceu));
    }

    IEnumerator Mostrar(bool venceu)
    {
        var s = jogo.Sim;
        // Desde já só esta tela recebe cliques (a loja e o mapa ficam de fora),
        // mesmo nos 0,8 s em que ela ainda está invisível.
        grupo.blocksRaycasts = true;
        int melhorAntes = Progresso.MelhorOnda;
        Progresso.RegistrarFim(s.Onda, venceu);

        yield return new WaitForSecondsRealtime(0.8f); // deixa ver o último estouro (ou o último pássaro chegando)

        titulo.text = venceu ? "A Torre resistiu!" : $"A Torre caiu na onda {s.Onda}";
        detalhes.text = venceu ? $"Vida que sobrou: {s.Vida} de {s.Dados.economia.vidaInicial}" : $"Você chegou à onda {s.Onda} de {s.TotalOndas}";
        recorde.text = s.Onda > melhorAntes && !venceu
            ? "Novo recorde!"
            : $"Recorde: onda {Progresso.MelhorOnda} · Vitórias: {Progresso.Vitorias}";
        medalha.enabled = venceu;
        if (venceu) medalha.sprite = s.Vida >= 80 ? medalhaOuro : s.Vida >= 40 ? medalhaPrata : medalhaBronze;

        if (Sons.Instancia != null) Sons.Tocar(venceu ? Sons.Instancia.vitoria : Sons.Instancia.derrota, 1f, 0f);
        if (venceu && efeitos != null)
            for (int i = 0; i < 12; i++) efeitos.Penas(new Vector2(1f + i * 1.1f, 14.5f), CoresConfete[i % CoresConfete.Length], 4);

        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.35f)
        {
            clarao.color = new Color(1f, 0.95f, 0.75f, 0.6f * (1f - Mathf.Clamp01(t * 3.5f))); // clarão de 0,1 s
            grupo.alpha = Mathf.Clamp01(t * 2f);
            painel.localScale = Vector3.one * VoltaComQuique(t);
            yield return null;
        }
        clarao.color = new Color(1f, 0.95f, 0.75f, 0f);
        grupo.alpha = 1f;
        painel.localScale = Vector3.one;
    }

    // "OutBack": passa um pouco de 1 e volta, como algo que cai e quica.
    static float VoltaComQuique(float t)
    {
        const float c1 = 1.70158f, c3 = c1 + 1f;
        float x = t - 1f;
        return 1f + c3 * x * x * x + c1 * x * x;
    }
}
