using UnityEngine;

// Um material de bloco é um asset (Assets/FoxFling/Materiais): Madeira, Vidro e Rocha
// são três arquivos deste tipo. Mudou um número aqui → muda em todos os blocos
// daquele material na próxima vez que der Play.
[CreateAssetMenu(menuName = "Fox Fling/Material de Bloco")]
public class MaterialBloco : ScriptableObject
{
    public string nome = "Madeira";

    [Tooltip("Densidade do collider. Com useAutoMass, massa = densidade × área do bloco.")]
    public float densidade = 1f;

    [Tooltip("Quanto dano o bloco aguenta antes de quebrar.")]
    public float vida = 50f;

    public int pontos = 500;

    [Tooltip("Atrito e quique (bounciness) usados pelo Box2D.")]
    public PhysicsMaterial2D fisica;

    public Sprite sprite, spriteRachado;

    [Tooltip("Opcional: versão com veio na vertical, usada quando o bloco é mais alto que largo.")]
    public Sprite spriteVertical, spriteVerticalRachado;

    public Color corDetritos = Color.white;
    public AudioClip somImpacto, somQuebra;
}
