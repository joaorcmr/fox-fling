using System;
using System.Collections.Generic;
using UnityEngine;

// Os pássaros na simulação: nascer, andar pela trilha, levar golpes e estourar
// (deixando penas e, no caso da pata, os patinhos).
public partial class Simulacao
{
    readonly List<Passaro> passaros = new List<Passaro>();
    public IReadOnlyList<Passaro> Passaros => passaros;

    int proximoId = 1;

    public event Action<Passaro> PassaroNasceu;
    public event Action<Passaro, float> PassaroAtingido;  // dano causado
    public event Action<Passaro, int> PassaroEstourou;    // penas que entraram

    // Causa dano direto num pássaro. É por aqui que todo tiro machuca. A
    // couraça (urubu) corta pela metade, a não ser que o golpe a ignore (bomba,
    // arpão com Ponta de Ferro).
    public void Atingir(Passaro p, float dano, bool ignoraCouraca)
    {
        if (p == null || !p.Vivo || Acabou) return;
        float efetivo = p.Dados.couraca && !ignoraCouraca ? dano * 0.5f : dano;
        p.Vida -= efetivo;
        PassaroAtingido?.Invoke(p, efetivo);
        if (p.Vida <= 0f) Estourar(p);
    }

    Passaro Nascer(EspeciePassaro especie, float multiplicadorVida, float distancia)
    {
        var p = new Passaro(proximoId++, Dados.passaros[especie], multiplicadorVida, distancia);
        p.Posicao = p.PosicaoAnterior = Trilha.Posicao(distancia);
        passaros.Add(p);
        PassaroNasceu?.Invoke(p);
        return p;
    }

    void MoverPassaros()
    {
        for (int i = 0; i < passaros.Count; i++)
        {
            var p = passaros[i];
            if (!p.Vivo) continue;
            if (p.TempoLentidao > 0f)
            {
                p.TempoLentidao -= Dt;
                if (p.TempoLentidao <= 0f)
                {
                    p.TempoLentidao = 0f;
                    p.Lentidao = 0f; // a lama secou
                }
            }
            p.Distancia += p.Velocidade * Dt;
            if (p.Distancia >= Trilha.Comprimento)
            {
                p.Distancia = Trilha.Comprimento;
                p.Posicao = Trilha.Posicao(p.Distancia);
                ChegouNaTorre(p);
                if (Acabou) return;
                continue;
            }
            p.Posicao = Trilha.Posicao(p.Distancia);
        }
    }

    void Estourar(Passaro p)
    {
        p.Vivo = false;
        int ganhas = Economia.Creditar(p.Dados.penas * Economia.Multiplicador * BonusPenas(p.Posicao));
        PassaroEstourou?.Invoke(p, ganhas);

        // A pata solta os patinhos onde estourou, um pouco espalhados para trás
        // na trilha e com vida cheia. Eles contam para o fim da onda.
        for (int k = 0; k < p.Dados.quantidadeFilhos; k++)
            Nascer(p.Dados.filho, 1f, Mathf.Max(0f, p.Distancia - 0.3f * k));
    }
}
