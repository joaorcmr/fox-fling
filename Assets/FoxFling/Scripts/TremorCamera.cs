using UnityEngine;

// Treme a câmera por um instante (explosões, pancadas fortes).
public class TremorCamera : MonoBehaviour
{
    static TremorCamera instancia;

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
        instancia.intensidade = novaIntensidade;
        instancia.duracao = novaDuracao;
        instancia.fim = Time.time + novaDuracao;
    }

    void LateUpdate()
    {
        if (Time.time < fim)
        {
            float k = (fim - Time.time) / duracao; // enfraquece até parar
            transform.localPosition = origem + (Vector3)(Random.insideUnitCircle * intensidade * k);
        }
        else
        {
            transform.localPosition = origem;
        }
    }
}
