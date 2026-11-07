using System;
using UnityEngine;
using UnityEngine.EventSystems;

// Grande: lenta e muito resistente, com golpe pesado contra a estrutura (só nas raposas atacantes).
public enum TipoRaposa { Normal, Ninja, Bomba, Grande }

public enum EstadoRaposa { Fila, IndoProEstilingue, NoEstilingue, Voando, Terminada }

// A "bola" do quadro: o jogador. Enquanto espera na fila ou no estilingue ela é
// Kinematic (a física não mexe nela, só o código). Ao ser lançada vira Dynamic e
// a partir daí quem manda é a gravidade e as colisões.
[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class Raposa : MonoBehaviour
{
    public TipoRaposa tipo = TipoRaposa.Normal;

    [Tooltip("Texto mostrado quando ela entra no estilingue.")]
    public string dica = "Arraste a raposa para trás e solte!";

    [Tooltip("Abaixo dessa velocidade por 1 segundo, a raposa conta como parada.")]
    public float velocidadeParada = 0.25f;

    public float tempoMaximoDeVoo = 12f;

    [Tooltip("Depois da primeira batida, quanto tempo ela ainda pode rolar.")]
    public float tempoDepoisDeBater = 5f;

    public EstadoRaposa Estado { get; private set; } = EstadoRaposa.Fila;
    public bool JaBateu { get; private set; }
    public bool HabilidadeUsada { get; private set; }
    public event Action<Raposa> Terminou;

    protected Rigidbody2D rb;
    CircleCollider2D colisor;
    TrailRenderer rastro;

    Vector2 alvoFila;
    Vector2 origemPulo, destinoPulo;
    float inicioPulo, duracaoPulo;
    float tempoLancamento, tempoBatida, tempoParada;
    float semente;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        colisor = GetComponent<CircleCollider2D>();
        rastro = GetComponent<TrailRenderer>();
        if (rastro != null) rastro.emitting = false;

        rb.bodyType = RigidbodyType2D.Kinematic;
        colisor.enabled = false;
        alvoFila = transform.position;
        semente = UnityEngine.Random.value * 10f;
    }

    public void IrParaFila(Vector2 posicao)
    {
        Estado = EstadoRaposa.Fila;
        alvoFila = posicao;
    }

    public void IrParaEstilingue(Vector2 posicao, float duracao)
    {
        Estado = EstadoRaposa.IndoProEstilingue;
        origemPulo = transform.position;
        destinoPulo = posicao;
        inicioPulo = Time.time;
        duracaoPulo = duracao;
        Sons.Tocar(Sons.I != null ? Sons.I.pulo : null, 0.6f);
    }

    public void Lancar(Vector2 velocidade)
    {
        Estado = EstadoRaposa.Voando;
        colisor.enabled = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = velocidade;
        rb.angularVelocity = -velocidade.x * 25f; // gira enquanto voa, como uma bola
        tempoLancamento = Time.time;

        if (rastro != null)
        {
            rastro.Clear();
            rastro.emitting = true;
        }
    }

    void Update()
    {
        switch (Estado)
        {
            case EstadoRaposa.Fila:
                // Anda até seu lugar na fila e fica dando pulinhos.
                Vector2 pos = Vector2.MoveTowards(transform.position, alvoFila, 3f * Time.deltaTime);
                if (pos == alvoFila)
                    pos.y += Mathf.Abs(Mathf.Sin(Time.time * 4f + semente)) * 0.1f;
                transform.position = pos;
                break;

            case EstadoRaposa.IndoProEstilingue:
                // Pulo em arco: interpolação linear + uma parábola para cima.
                float t = Mathf.Clamp01((Time.time - inicioPulo) / duracaoPulo);
                Vector2 p = Vector2.Lerp(origemPulo, destinoPulo, t);
                p.y += 4f * 0.8f * t * (1f - t);
                transform.position = p;
                transform.rotation = Quaternion.identity;
                if (t >= 1f) Estado = EstadoRaposa.NoEstilingue;
                break;

            case EstadoRaposa.Voando:
                AtualizarVoo();
                break;
        }
    }

    void AtualizarVoo()
    {
        float desdeLancamento = Time.time - tempoLancamento;

        if (desdeLancamento > 0.05f && Input.GetMouseButtonDown(0) && !PonteiroNaInterface())
        {
            AtivarHabilidade();
            if (Estado != EstadoRaposa.Voando) return; // a habilidade pode ter encerrado a raposa
        }

        if (Regras.ForaDoMundo(rb.position)
            || desdeLancamento > tempoMaximoDeVoo
            || (JaBateu && Time.time - tempoBatida > tempoDepoisDeBater))
        {
            Terminar();
            return;
        }

        if (desdeLancamento > 0.5f && rb.linearVelocity.magnitude < velocidadeParada)
        {
            tempoParada += Time.deltaTime;
            if (tempoParada > 1f) Terminar();
        }
        else
        {
            tempoParada = 0f;
        }
    }

    void OnCollisionEnter2D(Collision2D colisao)
    {
        if (Estado != EstadoRaposa.Voando) return;

        if (!JaBateu)
        {
            JaBateu = true;
            tempoBatida = Time.time;
            if (rastro != null) rastro.emitting = false;
        }
        AoBater(colisao);
    }

    // Chamado pelo clique no ar (e por testes). Cada raposa só usa a habilidade uma vez.
    public void AtivarHabilidade()
    {
        if (Estado != EstadoRaposa.Voando || HabilidadeUsada || !PodeUsarHabilidade) return;
        HabilidadeUsada = true;
        UsarHabilidade();
    }

    // Normal: sem habilidade. Ninja e Bomba sobrescrevem estes métodos.
    protected virtual bool PodeUsarHabilidade => !JaBateu;

    protected virtual void UsarHabilidade() { }

    protected virtual void AoBater(Collision2D colisao) { }

    protected void Terminar()
    {
        if (Estado == EstadoRaposa.Terminada) return;
        Estado = EstadoRaposa.Terminada;
        Efeitos.Fumaca(transform.position, 0.7f);
        Terminou?.Invoke(this);
        Destroy(gameObject);
    }

    public static bool PonteiroNaInterface()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
