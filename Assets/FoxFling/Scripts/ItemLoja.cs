using UnityEngine;

// Reparo: ação imediata. Muro: bloco colocado no chão (fortifica a torre).
// Arma: defesa que atira nas raposas (tem níveis, que a loja vende como melhorias).
public enum TipoItem { Reparo, Muro, Arma }

// Os números de uma arma em um nível.
[System.Serializable]
public struct NivelArma
{
    [Tooltip("Dano de cada tiro.")]
    public float dano;

    [Tooltip("Segundos entre um tiro e o próximo.")]
    public float intervalo;

    [Tooltip("Até que distância ela enxerga as raposas.")]
    public float alcance;

    [Tooltip("0 = acerta só uma raposa; maior que 0 = dano em área, com este raio.")]
    public float raioArea;

    [Tooltip("Preço para chegar a este nível (o do nível 1 não conta: é o preço de compra do item).")]
    public int custoMelhoria;
}

// Um item da loja é um asset (Assets/FoxFling/Loja): nome, preço e o tipo, que diz o que a
// compra faz. Mudou o preço aqui → muda na loja na próxima vez que der Play.
[CreateAssetMenu(menuName = "Fox Fling/Item da Loja")]
public class ItemLoja : ScriptableObject
{
    [Tooltip("Nome no botão. Os espaços viram quebra de linha (cabe em 64 px de largura).")]
    public string nome = "Item";

    [TextArea] public string descricao;

    public int preco = 100;

    public TipoItem tipo;

    [Header("Só para o que é colocado no chão (muros e armas)")]
    [Tooltip("O que aparece no chão. Para muros, o prefab Bloco; para armas, o prefab da arma.")]
    public GameObject prefab;

    [Tooltip("Só muros: o material do bloco (madeira, vidro ou rocha).")]
    public MaterialBloco material;

    [Tooltip("Largura × altura do que é colocado. Ele fica apoiado no chão.")]
    public Vector2 tamanho = new Vector2(0.4f, 1.2f);

    [Header("Só para armas")]
    [Tooltip("Os níveis da arma: o 1º é o que se compra, os seguintes são as melhorias.")]
    public NivelArma[] niveis;

    public int NivelMaximo => niveis != null ? niveis.Length : 0;

    // Os números do nível pedido (1 = primeiro); fora da faixa usa o mais próximo.
    public NivelArma Nivel(int nivel) => niveis[Mathf.Clamp(nivel - 1, 0, niveis.Length - 1)];
}
