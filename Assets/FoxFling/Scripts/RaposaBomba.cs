using UnityEngine;

// Raposa bomba: explode quando você clica (a qualquer momento depois do lançamento)
// ou sozinha, um pouco depois da primeira batida (o pavio acende).
public class RaposaBomba : Raposa
{
    [Tooltip("Segundos entre a primeira batida e a explosão automática.")]
    public float pavio = 1.2f;

    public float raio = 2.2f;
    public float impulso = 14f;
    public float danoMaximo = 120f;

    SpriteRenderer desenho;
    bool acesa;
    float explodeEm;

    protected override void Awake()
    {
        base.Awake();
        desenho = GetComponent<SpriteRenderer>();
    }

    // Diferente da ninja, a bomba pode ser ativada mesmo depois de bater.
    protected override bool PodeUsarHabilidade => true;

    protected override void UsarHabilidade() => Explodir();

    protected override void AoBater(Collision2D colisao)
    {
        if (acesa) return;
        acesa = true;
        explodeEm = Time.time + pavio;
    }

    void LateUpdate()
    {
        if (!acesa || Estado != EstadoRaposa.Voando) return;

        // Pisca em vermelho cada vez mais rápido enquanto o pavio queima.
        float falta = explodeEm - Time.time;
        float piscada = Mathf.PingPong(Time.time * Mathf.Lerp(16f, 4f, falta / pavio), 1f);
        desenho.color = Color.Lerp(Color.white, new Color(1f, 0.35f, 0.3f), piscada);

        if (falta <= 0f) Explodir();
    }

    void Explodir()
    {
        if (Estado != EstadoRaposa.Voando) return;
        Explosao.Explodir(transform.position, raio, impulso, danoMaximo, gameObject);
        Terminar();
    }
}
