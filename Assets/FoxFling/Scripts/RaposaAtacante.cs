using System;
using UnityEngine;

// Raposa "zumbi": entra pela esquerda, anda em direção à torre e, ao encostar de lado
// em algo que quebra (bloco ou tubarão), para e bate nele de tempos em tempos.
//
// Herda de Destrutivel: tem vida, pode morrer (pedra caindo, explosão, armas) e vale
// pontos para o jogador quando morre. O dano que ela causa NÃO vem da colisão (impulso)
// e sim de um golpe direto (ReceberDano), que é mais fácil de prever e de balancear.
[RequireComponent(typeof(CircleCollider2D))]
public class RaposaAtacante : Destrutivel
{
    public TipoRaposa tipo = TipoRaposa.Normal;

    [Tooltip("Velocidade andando, em unidades por segundo.")]
    public float velocidade = 1f;

    [Tooltip("Dano estrutural de cada golpe em um bloco ou tubarão.")]
    public float danoPorGolpe = 8f;

    [Tooltip("Segundos entre um golpe e o próximo.")]
    public float intervaloGolpe = 0.8f;

    // Quão rápido a velocidade volta ao normal depois de um empurrão (explosão, pedra).
    const float Aceleracao = 25f;

    // Se ficar esse tempo sem encostar no alvo (foi empurrada), volta a andar.
    const float TempoSemContato = 0.2f;

    // Só conta como "bater de frente" se o contato for mais de lado do que por cima.
    const float NormalLateralMinima = 0.5f;

    public event Action<RaposaAtacante> Morreu;

    Destrutivel alvo;
    float proximoGolpe;
    float ultimoContato;
    bool semRecompensa;

    protected override void Awake()
    {
        base.Awake();
        rb.freezeRotation = true; // anda em pé, não rola como a raposa lançada
    }

    void FixedUpdate()
    {
        if (alvo != null && (alvo.Morto || Time.time - ultimoContato > TempoSemContato))
            alvo = null;

        float alvoX = alvo != null ? 0f : velocidade;
        float x = Mathf.MoveTowards(rb.linearVelocity.x, alvoX, Aceleracao * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(x, rb.linearVelocity.y);
    }

    void OnCollisionStay2D(Collision2D colisao)
    {
        if (Morto) return;

        var outro = colisao.collider.GetComponent<Destrutivel>();
        if (outro == null || outro is RaposaAtacante || outro.Morto) return;

        // Pisar em cima de um bloco não é atacar: só vale contato de lado, à frente.
        if (Mathf.Abs(colisao.GetContact(0).normal.x) < NormalLateralMinima) return;
        if (outro.transform.position.x < transform.position.x) return;

        alvo = outro;
        ultimoContato = Time.time;

        if (Time.time < proximoGolpe) return;
        proximoGolpe = Time.time + intervaloGolpe;
        Golpear(outro);
    }

    // Normal: bate no alvo. A bomba sobrescreve para explodir.
    protected virtual void Golpear(Destrutivel outro)
    {
        outro.ReceberDano(danoPorGolpe);
        TremorCamera.Tremer(0.03f, 0.08f);

        var bloco = outro as Bloco;
        if (bloco != null && bloco.material != null)
            Sons.Tocar(bloco.material.somImpacto, 0.6f);
    }

    // Só dá pontos se foi derrotada. Quem se autodestrói (bomba) não rende nada.
    protected override int PontosAoMorrer => semRecompensa ? 0 : base.PontosAoMorrer;

    protected void Autodestruir()
    {
        semRecompensa = true;
        Morrer();
    }

    protected override void Morrer()
    {
        if (Morto) return;
        Efeitos.Fumaca(transform.position, 0.7f);
        Sons.Tocar(Sons.I != null ? Sons.I.inimigoMorre : null, 0.6f);
        base.Morrer();
        Morreu?.Invoke(this);
    }
}
