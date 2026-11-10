using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Obstaculo
{
    public string tipo;
    public Vector2 centro;
    public float raio;
}

// O mapa: a trilha, a Torre e os obstáculos (editados no Inspector, no asset Mapa).
[Serializable]
public class DadosMapa
{
    public string nome;
    public Vector2 tamanho = new Vector2(14f, 15f);
    [Tooltip("Pontos da trilha, do lugar onde os pássaros entram até a Torre.")]
    public List<Vector2> pontos = new List<Vector2>();
    public float raioCanto = 1f;
    [Tooltip("Metade da largura da trilha: porco não pode ficar em cima dela.")]
    public float meiaLargura = 0.6f;
    public Vector2 torre;
    public float raioTorre = 1.1f;
    public List<Obstaculo> obstaculos = new List<Obstaculo>();
}
