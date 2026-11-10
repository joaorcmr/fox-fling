using UnityEngine;

// Um tipo de pássaro como asset (nesta pasta, Passaros/Dados). Os números
// ficam em "dados" (o que a simulação usa); o resto é só para desenhar.
[CreateAssetMenu(menuName = "Revoada/Tipo de pássaro")]
public class TipoPassaro : ScriptableObject
{
    public DadosPassaro dados = new DadosPassaro();
    public Sprite sprite;
    [Tooltip("Cor das penas que voam quando ele leva um golpe ou estoura.")]
    public Color corPenas = Color.white;
    public float tamanho = 1f;
}
