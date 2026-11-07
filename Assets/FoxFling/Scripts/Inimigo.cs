using UnityEngine;

// Tubarões. No tower defense eles são os personagens do JOGADOR (quem as raposas querem
// atingir): a fase só é perdida quando todos morrem. O nome da classe continua "Inimigo"
// por enquanto. Mesma regra de dano dos blocos.
[RequireComponent(typeof(SpriteRenderer))]
public class Inimigo : Destrutivel
{
    public Sprite spriteFerido;

    SpriteRenderer sr;
    Sprite spriteSaudavel;

    protected override void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        spriteSaudavel = sr.sprite;
        base.Awake();
    }

    protected override void AoRestaurar()
    {
        sr.sprite = spriteSaudavel;
    }

    void Start()
    {
        // Start roda depois de todos os Awake, então o GerenciadorFase já existe.
        if (GerenciadorFase.Instance != null)
            GerenciadorFase.Instance.RegistrarInimigo(this);
    }

    // Perder um tubarão não dá pontos (a penalidade por tubarão perdido vem na issue #4).
    protected override int PontosAoMorrer => 0;

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
