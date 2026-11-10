using System;
using System.Collections.Generic;
using UnityEngine;

// Uma onda é uma lista de grupos: tipo de pássaro, quantos, de quanto em quanto
// tempo e a partir de quando (editados no Inspector, no asset ListaOndas).
[Serializable]
public class GrupoOnda
{
    public EspeciePassaro especie;
    public int quantidade = 1;
    [Tooltip("Segundos entre um pássaro e o próximo do grupo.")]
    public float intervalo = 1f;
    [Tooltip("Segundos depois do início da onda para o grupo começar.")]
    public float atraso;
    [Tooltip("Multiplica a vida (os chefes das ondas 20 e 30 são mais fortes).")]
    public float multiplicadorVida = 1f;
}

[Serializable]
public class DadosOnda
{
    public List<GrupoOnda> grupos = new List<GrupoOnda>();
}
