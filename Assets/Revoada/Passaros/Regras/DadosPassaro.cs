using System;
using UnityEngine;

public enum EspeciePassaro { Pardal, Sabia, BeijaFlor, Arara, Pata, Patinho, Urubu, Gaviao }

// Os números de um tipo de pássaro (editados no Inspector, no asset TipoPassaro).
[Serializable]
public class DadosPassaro
{
    public EspeciePassaro especie;
    public string nome;
    [Tooltip("Quantos pontos de dano aguenta antes de estourar.")]
    public float vida = 1f;
    [Tooltip("Unidades por segundo ao longo da trilha (1 unidade = 32 px).")]
    public float velocidade = 2.5f;
    [Tooltip("Raio do corpo, usado para saber se um tiro encostou.")]
    public float raio = 0.35f;
    [Tooltip("Quanto de vida tira da Torre dos Porcos se chegar até ela.")]
    public int danoTorre = 1;
    [Tooltip("Moedas que deixa ao estourar (antes do multiplicador do Cofrinho).")]
    public float moedas = 1f;
    [Tooltip("Couraça: pedrinha e arpão causam só metade do dano.")]
    public bool couraca;
    public bool chefe;
    [Tooltip("Pássaro que nasce quando este estoura (a pata solta patinhos).")]
    public EspeciePassaro filho;
    public int quantidadeFilhos;
}
