using UnityEngine;

// A Torre desenhada no mapa apanhando: quando um pássaro chega nela, ela treme
// para os lados e fica avermelhada por 0,3 s, e a câmera dá um tranco. Tudo em
// tempo sem escala (a Torre que caiu ainda treme na tela de fim).
public class VisualTorre : MonoBehaviour
{
    const float DuracaoGolpe = 0.3f;

    public Jogo jogo;
    [Tooltip("O sprite da Torre que o VisualMapa desenha.")]
    public SpriteRenderer torre;

    Vector3 posicao;
    float fimGolpe;

    void Start()
    {
        if (torre != null) posicao = torre.transform.position;
        jogo.Sim.TorreAtingida += dano =>
        {
            fimGolpe = Time.unscaledTime + DuracaoGolpe;
            TremorCamera.Tremer(Mathf.Min(0.25f, 0.06f + dano * 0.01f), 0.25f);
        };
    }

    void Update()
    {
        if (torre == null) return;
        float k = Mathf.Clamp01((fimGolpe - Time.unscaledTime) / DuracaoGolpe);
        torre.transform.position = posicao + Vector3.right * Mathf.Sin(Time.unscaledTime * 70f) * 0.07f * k;
        torre.color = Color.Lerp(Color.white, new Color(1f, 0.5f, 0.45f), k);
    }
}
