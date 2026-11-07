using UnityEngine;

// O desenho de um tiro (prefab Projetil). Pedrinha e arpão giram na direção do
// voo; a bomba sobe e desce num arco (só no desenho: na simulação ela anda em
// linha reta até o ponto mirado) e deixa uma sombra no chão.
public class VisualProjetil : MonoBehaviour
{
    const float AlturaBomba = 0.8f;

    public SpriteRenderer sprite;
    public SpriteRenderer sombra;

    public Projetil Projetil { get; private set; }

    public void Iniciar(Projetil p, Sprite s)
    {
        Projetil = p;
        sprite.sprite = s;
        sombra.enabled = p.Tipo == TipoTiro.Bomba;
        Atualizar(0f);
    }

    public void Atualizar(float alfa)
    {
        var p = Projetil;
        if (p == null) return;
        transform.position = Vector2.Lerp(p.PosicaoAnterior, p.Posicao, alfa);

        if (p.Tipo == TipoTiro.Bomba)
        {
            // Arco: altura 4·h·f·(1 − f), máxima no meio do caminho.
            float f = 1f - p.DistanciaRestante / Mathf.Max(p.DistanciaTotal, 1e-4f);
            float altura = 4f * AlturaBomba * f * (1f - f);
            sprite.transform.localPosition = new Vector3(0f, altura, 0f);
            sprite.transform.localRotation = Quaternion.Euler(0f, 0f, -f * 540f);
            sombra.transform.localScale = Vector3.one * (0.45f - altura * 0.2f);
        }
        else
        {
            float angulo = Mathf.Atan2(p.Velocidade.y, p.Velocidade.x) * Mathf.Rad2Deg;
            sprite.transform.localPosition = Vector3.zero;
            sprite.transform.localRotation = Quaternion.Euler(0f, 0f, angulo);
        }
    }
}
