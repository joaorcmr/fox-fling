using UnityEngine;
using UnityEngine.UI;

// A tela de título: o próprio mapa ao fundo (com pássaros voando), o logo, o
// botão Jogar pulsando de leve (o movimento diz "aperte aqui"), Como jogar,
// o recorde e os volumes. "Zerar progresso" pergunta antes de apagar, porque
// não tem volta (controle e liberdade do usuário).
public class MenuPrincipal : MonoBehaviour
{
    public Button botaoJogar;
    public Button botaoComoJogar;
    public Button botaoFecharComoJogar;
    public Button botaoZerar;
    public Button botaoCancelarZerar;
    public Button botaoConfirmarZerar;
    public GameObject painelComoJogar;
    public GameObject painelConfirmarZerar;
    public Text textoRecorde;
    public RectTransform logo;

    Vector2 posicaoLogo;

    bool ModalAberto => painelComoJogar.activeSelf || painelConfirmarZerar.activeSelf;

    void Start()
    {
        Time.timeScale = 1f;
        posicaoLogo = logo.anchoredPosition;
        painelComoJogar.SetActive(false);
        painelConfirmarZerar.SetActive(false);
        botaoJogar.onClick.AddListener(Navegacao.Jogar);
        botaoComoJogar.onClick.AddListener(() => painelComoJogar.SetActive(true));
        botaoFecharComoJogar.onClick.AddListener(() => painelComoJogar.SetActive(false));
        botaoZerar.onClick.AddListener(() => painelConfirmarZerar.SetActive(true));
        botaoCancelarZerar.onClick.AddListener(() => painelConfirmarZerar.SetActive(false));
        botaoConfirmarZerar.onClick.AddListener(() =>
        {
            Progresso.Zerar();
            AtualizarRecorde();
            painelConfirmarZerar.SetActive(false);
        });
        AtualizarRecorde();
    }

    void AtualizarRecorde()
    {
        textoRecorde.text = Progresso.MelhorOnda > 0
            ? $"Recorde: onda {Progresso.MelhorOnda} de 30 · Vitórias: {Progresso.Vitorias}"
            : "Os pássaros estão chegando. A Torre conta com você!";
    }

    void Update()
    {
        float t = Time.unscaledTime;
        botaoJogar.transform.localScale = Vector3.one * (1f + 0.035f * Mathf.Sin(t * 3.9f)); // ~1,6 s por pulso
        logo.anchoredPosition = posicaoLogo + Vector2.up * Mathf.Sin(t * 1.4f) * 3f;
        if (!ModalAberto && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))) Navegacao.Jogar();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (painelConfirmarZerar.activeSelf) painelConfirmarZerar.SetActive(false);
            else painelComoJogar.SetActive(false);
        }
    }
}
