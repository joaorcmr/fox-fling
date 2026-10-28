using UnityEngine;

// Raposa ninja: clicar com ela no ar dá uma arrancada na direção em que está indo.
// É um impulso instantâneo: troca a velocidade direto, sem esperar a física acelerar.
public class RaposaNinja : Raposa
{
    [Tooltip("Quanto a velocidade é multiplicada na arrancada.")]
    public float multiplicador = 1.8f;

    [Tooltip("Velocidade mínima depois da arrancada (u/s), para valer mesmo no topo do arco.")]
    public float velocidadeMinima = 16f;

    protected override void UsarHabilidade()
    {
        Vector2 v = rb.linearVelocity;
        float rapidez = Mathf.Max(v.magnitude * multiplicador, velocidadeMinima);
        rb.linearVelocity = (v.sqrMagnitude > 0.01f ? v.normalized : Vector2.right) * rapidez;

        var rastro = GetComponent<TrailRenderer>();
        if (rastro != null) rastro.widthMultiplier *= 1.6f;

        Efeitos.Fumaca(transform.position, 0.5f);
        TremorCamera.Tremer(0.05f, 0.15f);
        Sons.Tocar(Sons.I != null ? Sons.I.arrancada : null);
    }
}
