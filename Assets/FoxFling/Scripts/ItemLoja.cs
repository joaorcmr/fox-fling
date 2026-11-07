using UnityEngine;

public enum TipoItem { Reparo }

// Um item da loja é um asset (Assets/FoxFling/Loja): nome, preço e o tipo, que diz o que a
// compra faz. Mudou o preço aqui → muda na loja na próxima vez que der Play.
[CreateAssetMenu(menuName = "Fox Fling/Item da Loja")]
public class ItemLoja : ScriptableObject
{
    public string nome = "Item";

    [TextArea] public string descricao;

    public int preco = 100;

    public TipoItem tipo;
}
