using UnityEngine;

// Treme a câmera por um instante (explosão de lama, pássaro chegando na Torre).
// Usa tempo sem escala: o tremor dura o mesmo em 1× ou 3×.
public class TremorCamera : MonoBehaviour
{
    static TremorCamera instancia;

    [Tooltip("Multiplica todos os tremores (0 desliga).")]
    public float forca = 1f;

    Vector3 origem;
    float intensidade, duracao, fim;

    void Awake()
    {
        instancia = this;
        origem = transform.localPosition;
    }

    void OnDestroy()
    {
        if (instancia == this) instancia = null;
    }

    public static void Tremer(float novaIntensidade, float novaDuracao)
    {
        if (instancia == null) return;
        // Um tremor fraco não apaga um forte que ainda está acontecendo.
        if (Time.unscaledTime < instancia.fim && novaIntensidade < instancia.intensidade) return;
        instancia.intensidade = novaIntensidade;
        instancia.duracao = novaDuracao;
        instancia.fim = Time.unscaledTime + novaDuracao;
    }

    void LateUpdate()
    {
        if (Time.unscaledTime < fim)
        {
            float k = (fim - Time.unscaledTime) / duracao; // enfraquece até parar
            transform.localPosition = origem + (Vector3)(Random.insideUnitCircle * intensidade * k * forca);
        }
        else
        {
            transform.localPosition = origem;
        }
    }
}
