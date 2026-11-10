using UnityEngine;

// O mapa: pontos da trilha, Torre e obstáculos. O VisualMapa desenha a partir
// daqui, e os Gizmos mostram a trilha na Scene view.
[CreateAssetMenu(menuName = Identidade.NomeDoJogo + "/Mapa")]
public class Mapa : ScriptableObject
{
    public DadosMapa dados = new DadosMapa();
}
