using UnityEngine;

// Triângulos (inimigos). Mesma regra de dano dos blocos, mas valem bem mais pontos
// e a fase só termina quando todos morrem.
[RequireComponent(typeof(SpriteRenderer))]
public class Inimigo : Destrutivel
{
    public Sprite spriteFerido;

    SpriteRenderer sr;

    protected override void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        base.Awake();
    }

    void Start()
    {
        // Start roda depois de todos os Awake, então o GerenciadorFase já existe.
        if (GerenciadorFase.Instance != null)
            GerenciadorFase.Instance.RegistrarInimigo(this);
    }

    protected override void AoMudarVida()
    {
        if (spriteFerido != null && Vida < vidaMaxima * 0.6f)
            sr.sprite = spriteFerido;
    }

    protected override void Morrer()
    {
        if (Morto) return;
        Efeitos.Fumaca(transform.position, transform.localScale.x);
        Sons.Tocar(Sons.I != null ? Sons.I.inimigoMorre : null);
        base.Morrer();

        if (GerenciadorFase.Instance != null)
            GerenciadorFase.Instance.InimigoMorreu(this);
    }
}
