using System.Collections.Generic;
using UnityEngine;

// Um tiro em voo. Ele leva uma CÓPIA dos atributos do porco no momento do
// disparo: se o porco for vendido ou melhorado com o tiro no ar, o tiro segue
// igual (e não depende mais do porco existir).
public class Projetil
{
    public readonly int Id;
    public readonly Porco Dono;
    public readonly Atributos A;

    public Vector2 Posicao;
    public Vector2 PosicaoAnterior;
    public Vector2 Velocidade;
    public Vector2 Destino;          // a bomba explode aqui
    public float DistanciaRestante;
    public float DistanciaTotal;
    public int PerfuracaoRestante;
    public bool Ativo = true;
    public readonly HashSet<Passaro> Atingidos = new HashSet<Passaro>(); // cada pássaro leva só um golpe deste tiro

    public TipoTiro Tipo => A.tiro;

    public Projetil(int id, Porco dono, Atributos a, Vector2 origem, Vector2 direcao, float distancia)
    {
        Id = id;
        Dono = dono;
        A = a;
        var dir = direcao.normalized;
        Posicao = PosicaoAnterior = origem;
        Velocidade = dir * a.velocidadeProjetil;
        DistanciaRestante = DistanciaTotal = distancia;
        Destino = origem + dir * distancia;
        PerfuracaoRestante = Mathf.Max(1, a.perfura);
    }
}
