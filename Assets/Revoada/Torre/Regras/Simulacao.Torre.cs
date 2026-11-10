using System;
using UnityEngine;

// A Torre dos Porcos: cada pássaro que chega nela tira um pouco de vida, e a
// partida acaba em derrota quando a vida chega a zero. O jogador pode comprar
// reparo (issue #14): cura uma parte do que faltava, sem passar da vida
// inicial, e cada reparo fica mais caro que o anterior.
public partial class Simulacao
{
    public int Vida { get; private set; }
    public int VidaMaxima { get; private set; }

    public event Action<Passaro> PassaroChegou;           // chegou na Torre
    public event Action<int> TorreAtingida;               // dano
    public event Action<int> TorreReparada;               // cura

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

    public Resultado ComprarReparo()
    {
        if (Acabou) return Resultado.Indisponivel;
        if (Estado == EstadoJogo.EmOnda) return Resultado.OndaEmAndamento;
        if (Vida >= VidaMaxima) return Resultado.Indisponivel;
        if (!Economia.ComprarReparo()) return Resultado.SemMoedas;

        int cura = Mathf.Min(Dados.economia.curaPorReparo, VidaMaxima - Vida);
        Vida += cura;
        TorreReparada?.Invoke(cura);
        return Resultado.Ok;
    }
}
