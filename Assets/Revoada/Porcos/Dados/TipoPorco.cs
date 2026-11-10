using UnityEngine;

// Um tipo de porco como asset (nesta pasta, Porcos/Dados): custo, números de
// cada nível e das duas especializações, e os sprites.
[CreateAssetMenu(menuName = "Revoada/Tipo de porco")]
public class TipoPorco : ScriptableObject
{
    public DadosPorco dados = new DadosPorco();
    public Sprite corpo;
    public Sprite arma;
    public Sprite projetil;
    public Sprite acessorioA;
    public Sprite acessorioB;
}
