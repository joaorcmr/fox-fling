using UnityEngine;

// Base de tudo que tem vida e quebra com pancada: blocos e inimigos.
//
// O dano vem do IMPULSO da colisão. Impulso é "força × tempo" (ou massa × variação
// de velocidade): o Box2D calcula quanto teve que empurrar cada ponto de contato
// para os corpos não se atravessarem. Batida rápida ou de coisa pesada → impulso alto.
[RequireComponent(typeof(Rigidbody2D))]
public abstract class Destrutivel : MonoBehaviour
{
    public float vidaMaxima = 50f;

    [Tooltip("Impulsos menores que isso não causam dano (encostar, rolar, ficar apoiado).")]
    public float impulsoMinimo = 1f;

    [Tooltip("Dano = impulso × multiplicador.")]
    public float multiplicadorDano = 10f;

    public int pontos = 500;

    public float Vida { get; private set; }
    public bool Morto { get; private set; }
    public bool Danificado => !Morto && Vida < vidaMaxima - 0.01f;

    protected Rigidbody2D rb;
    readonly ContactPoint2D[] contatos = new ContactPoint2D[8];

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Vida = vidaMaxima;
    }

    protected virtual void Update()
    {
        // Empurrado para fora da tela: conta como destruído (e a fase não trava).
        if (!Morto && Regras.ForaDoMundo(transform.position))
            Morrer();
    }

    void OnCollisionEnter2D(Collision2D colisao)
    {
        if (Morto) return;

        // Soma o impulso de todos os pontos de contato desta batida.
        int n = colisao.GetContacts(contatos);
        float impulso = 0f;
        for (int i = 0; i < n; i++)
            impulso += contatos[i].normalImpulse;

        // No começo da fase as peças ainda estão se acomodando: nada de dano.
        if (Time.timeSinceLevelLoad < Regras.TempoAcomodacao) return;

        AoBater(impulso);

        float dano = Regras.DanoDoImpulso(impulso, impulsoMinimo, multiplicadorDano);
        if (dano > 0f)
            ReceberDano(dano);
    }

    public void ReceberDano(float dano)
    {
        if (Morto) return;

        Vida -= dano;
        if (Vida <= 0f) Morrer();
        else AoMudarVida();
    }

    protected virtual void AoBater(float impulso) { }

    protected virtual void AoMudarVida() { }

    // Reparo (loja): volta à vida cheia, e quem tem sprite "ferido" volta ao sprite inteiro.
    public void RestaurarVida()
    {
        if (Morto) return;
        Vida = vidaMaxima;
        AoRestaurar();
    }

    protected virtual void AoRestaurar() { }

    // Quantos pontos o jogador ganha quando isto morre. Por padrão, o campo "pontos";
    // blocos e tubarões sobrescrevem para 0 (no tower defense só as raposas valem).
    protected virtual int PontosAoMorrer => pontos;

    protected virtual void Morrer()
    {
        if (Morto) return;
        Morto = true;

        if (GerenciadorFase.Instance != null)
            GerenciadorFase.Instance.AdicionarPontos(PontosAoMorrer, transform.position);

        Destroy(gameObject);
    }
}
