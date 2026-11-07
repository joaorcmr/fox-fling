using UnityEngine;

// Reparo: ação imediata. Muro: bloco colocado no chão (fortifica a torre).
public enum TipoItem { Reparo, Muro }

// Um item da loja é um asset (Assets/FoxFling/Loja): nome, preço e o tipo, que diz o que a
// compra faz. Mudou o preço aqui → muda na loja na próxima vez que der Play.
[CreateAssetMenu(menuName = "Fox Fling/Item da Loja")]
public class ItemLoja : ScriptableObject
{
    [Tooltip("Nome no botão. Os espaços viram quebra de linha (cabe em 76 px de largura).")]
    public string nome = "Item";

    [TextArea] public string descricao;

    public int preco = 100;

    public TipoItem tipo;

    [Header("Só para o que é colocado no chão")]
    [Tooltip("O que aparece no chão. Para muros, o prefab Bloco.")]
    public GameObject prefab;

    [Tooltip("Só muros: o material do bloco (madeira, vidro ou rocha).")]
    public MaterialBloco material;

    [Tooltip("Largura × altura do que é colocado. Ele fica apoiado no chão.")]
    public Vector2 tamanho = new Vector2(0.4f, 1.2f);
}
