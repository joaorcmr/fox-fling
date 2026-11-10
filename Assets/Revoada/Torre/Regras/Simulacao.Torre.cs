using System;
using UnityEngine;

// A Torre dos Porcos: cada pássaro que chega nela tira um pouco de vida, e a
// partida acaba em derrota quando a vida chega a zero.
public partial class Simulacao
{
    public int Vida { get; private set; }

    public event Action<Passaro> PassaroChegou;           // chegou na Torre
    public event Action<int> TorreAtingida;               // dano

    void ChegouNaTorre(Passaro p)
    {
        p.Vivo = false;
        int dano = p.Dados.danoTorre;
        Vida = Mathf.Max(0, Vida - dano);
        PassaroChegou?.Invoke(p);
        TorreAtingida?.Invoke(dano);
        if (Vida <= 0)
        {
            Estado = EstadoJogo.Derrota;
            FimDeJogo?.Invoke(false);
        }
    }
}
