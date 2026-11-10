using System;
using System.Collections.Generic;
using UnityEngine;

// Um nível do Ninho de Penas: quanto multiplica as penas e quanto custa.
[Serializable]
public class NivelNinho
{
    public float multiplicador;
    public int custo;
}

// Penas iniciais, vida da Torre, bônus de onda, venda e os níveis do Ninho
// (editados no Inspector, no asset ConfigEconomia).
[Serializable]
public class DadosEconomia
{
    public int penasIniciais = 300;
    public int vidaInicial = 100;
    [Tooltip("Bônus de fim de onda = base + porOnda × número da onda.")]
    public int bonusOndaBase = 25;
    public int bonusOndaPorOnda = 5;
    [Tooltip("Fração do que foi gasto que volta ao vender um porco.")]
    public float fracaoVenda = 0.7f;
    [Tooltip("Segundos entre uma onda e a próxima com o Auto ligado.")]
    public float esperaAuto = 2f;
    public List<NivelNinho> ninho = new List<NivelNinho>();
}
