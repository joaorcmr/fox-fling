using UnityEngine;

// Raposa bomba atacante: corre até a torre e explode no primeiro contato. A explosão
// (a mesma do estilingue: Explosao.Explodir) causa dano em área e empurra os blocos,
// então é a raposa mais perigosa para a estrutura.
public class AtacanteBomba : RaposaAtacante
{
    public float raio = 2.2f;
    public float impulso = 14f;
    public float danoMaximo = 120f;

    protected override void Golpear(Destrutivel outro)
    {
        Explosao.Explodir(transform.position, raio, impulso, danoMaximo, gameObject, poupaAtacantes: true);
        Autodestruir();
    }
}
