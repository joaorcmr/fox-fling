using System;
using UnityEngine;

public enum EspeciePorco { Estilingueiro, Arpoador, Bombardeiro, Tamboreiro }

// Em quem o porco mira quando há vários pássaros no alcance.
public enum ModoAlvo { Primeiro, Ultimo, MaisForte, MaisPerto }

// O bloco de números de um porco num nível. Uma melhoria guarda o bloco
// COMPLETO do nível seguinte (não a diferença), então o Inspector mostra
// exatamente como o porco fica depois de comprá-la.
[Serializable]
public class Atributos
{
    public TipoTiro tiro = TipoTiro.Pedrinha;
    public float dano = 1f;
    public float alcance = 3f;
    [Tooltip("Segundos entre um disparo e outro.")]
    public float recarga = 0.8f;
    public float velocidadeProjetil = 12f;
    public float raioProjetil = 0.15f;
    [Tooltip("Quantos pássaros um mesmo tiro atravessa.")]
    public int perfura = 1;
    [Tooltip("O tiro voa até alcance × alcanceVoo antes de sumir.")]
    public float alcanceVoo = 1.3f;
    [Tooltip("Tiros por disparo (a Rajada solta 3 em leque).")]
    public int tiros = 1;
    [Tooltip("Graus entre os tiros do leque.")]
    public float abertura;
    [Tooltip("Raio da explosão (bomba).")]
    public float raioArea;
    [Tooltip("Fração da velocidade que o pássaro perde (0,4 = 40% mais lento).")]
    public float lentidao;
    public float duracaoLentidao;
    [Tooltip("Quanto o pássaro volta na trilha quando é atingido.")]
    public float empurrao;
    public bool ignoraCouraca;

    [Header("Tamboreiro (aura)")]
    public float raioAura;
    [Tooltip("0,2 = os vizinhos atiram 20% mais rápido.")]
    public float bonusCadencia;
    public float bonusAlcance;
    [Tooltip("0,5 = pássaros estourados dentro da aura rendem 50% mais moedas.")]
    public float bonusMoedas;

    public Atributos Copia() => (Atributos)MemberwiseClone();
}

[Serializable]
public class Melhoria
{
    public string nome;
    public string descricao;
    public int custo;
    public Atributos atributos = new Atributos();
}

// Os números de um tipo de porco (editados no Inspector, no asset TipoPorco).
[Serializable]
public class DadosPorco
{
    public EspeciePorco especie;
    public string nome;
    public string papel;
    public int custo;
    [Tooltip("Espaço que o porco ocupa no mapa.")]
    public float raio = 0.45f;
    public Atributos nivel1 = new Atributos();
    public Melhoria nivel2 = new Melhoria();
    public Melhoria nivel3 = new Melhoria();
    public Melhoria especializacaoA = new Melhoria();
    public Melhoria especializacaoB = new Melhoria();
}
