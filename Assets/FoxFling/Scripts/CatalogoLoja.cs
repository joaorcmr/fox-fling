using UnityEngine;

// A lista do que a loja vende, na ordem em que os botões aparecem na barra.
[CreateAssetMenu(menuName = "Fox Fling/Catálogo da Loja")]
public class CatalogoLoja : ScriptableObject
{
    public ItemLoja[] itens;
}
