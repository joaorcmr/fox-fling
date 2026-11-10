using UnityEngine;

// A Torre desenhada no mapa: quando um pássaro chega nela, ela treme para os
// lados e fica avermelhada por 0,3 s, e a câmera dá um tranco; quando é
// reparada, brilha verde por um instante. Tudo em tempo sem escala (a Torre
// que caiu ainda treme na tela de fim).
//
// A Torre é o sprite que o VisualMapa guarda em torreDesenhada. Numa fase nova
// o mapa é redesenhado e a Torre vira OUTRO objeto, em outro lugar: por isso
// ela é lida do mapa a cada quadro, e não guardada uma vez só.
public class VisualTorre : MonoBehaviour
{
    const float DuracaoGolpe = 0.3f;
    const float DuracaoReparo = 0.5f;
    static readonly Color CorGolpe = new Color(1f, 0.5f, 0.45f);
    static readonly Color CorReparo = new Color(0.62f, 1f, 0.55f);

    public Jogo jogo;
    [Tooltip("O mapa desenhado (a Torre é o torreDesenhada dele).")]
    public VisualMapa mapa;

    SpriteRenderer torre;
    Vector3 posicao;
    float fimGolpe, fimReparo;

    void Start()
    {
        jogo.Sim.TorreAtingida += dano =>
        {
            fimGolpe = Time.unscaledTime + DuracaoGolpe;
            TremorCamera.Tremer(Mathf.Min(0.25f, 0.06f + dano * 0.01f), 0.25f);
        };
        jogo.Sim.TorreReparada += cura => fimReparo = Time.unscaledTime + DuracaoReparo;
    }

    void Update()
    {
        if (mapa != null && mapa.torreDesenhada != torre)
        {
            torre = mapa.torreDesenhada;
            if (torre != null) posicao = torre.transform.position;
        }
        if (torre == null) return;

        float golpe = Mathf.Clamp01((fimGolpe - Time.unscaledTime) / DuracaoGolpe);
        float reparo = Mathf.Clamp01((fimReparo - Time.unscaledTime) / DuracaoReparo);
        torre.transform.position = posicao + Vector3.right * Mathf.Sin(Time.unscaledTime * 70f) * 0.07f * golpe;
        var cor = Color.Lerp(Color.white, CorReparo, reparo);
        torre.color = Color.Lerp(cor, CorGolpe, golpe);
    }
}
