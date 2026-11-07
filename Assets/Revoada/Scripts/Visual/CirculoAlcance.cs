using UnityEngine;

// O círculo que mostra o alcance de um porco (ou a aura do Tamboreiro). É o
// sprite "anel" escalado: o anel tem 256 px a 64 px por unidade, ou seja, um
// raio de cerca de 1,97 unidade em tamanho 1.
public class CirculoAlcance : MonoBehaviour
{
    const float RaioDoSprite = 1.97f;

    public SpriteRenderer anel;

    public bool Visivel => anel != null && anel.enabled;
    public Color Cor => anel != null ? anel.color : Color.white;

    public void Mostrar(Vector2 centro, float raio, Color cor)
    {
        transform.position = centro;
        transform.localScale = Vector3.one * (raio / RaioDoSprite);
        anel.color = cor;
        anel.enabled = true;
    }

    public void Esconder()
    {
        if (anel != null) anel.enabled = false;
    }
}
