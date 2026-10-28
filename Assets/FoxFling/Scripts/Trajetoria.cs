using UnityEngine;

// Linha pontilhada que mostra para onde a raposa vai. Não simula a física de
// verdade: usa a equação do movimento (Regras.PosicaoNaTrajetoria), que é exata
// enquanto a raposa só sofre a gravidade (sem atrito com o ar).
public class Trajetoria : MonoBehaviour
{
    public Sprite spritePonto;
    public int quantidade = 20;

    [Tooltip("Tempo (s) entre um ponto e o próximo.")]
    public float passoTempo = 0.07f;

    public float chaoY = -4.5f;

    SpriteRenderer[] pontos;

    void Awake()
    {
        pontos = new SpriteRenderer[quantidade];
        for (int i = 0; i < quantidade; i++)
        {
            var go = new GameObject("Ponto " + i);
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = spritePonto;
            sr.sortingOrder = 15;
            float k = 1f - (float)i / quantidade;
            go.transform.localScale = Vector3.one * Mathf.Lerp(0.45f, 1f, k);
            sr.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.25f, 0.95f, k));
            pontos[i] = sr;
        }
        Esconder();
    }

    public void Mostrar(Vector2 inicio, Vector2 velocidade)
    {
        Vector2 gravidade = Physics2D.gravity;
        bool abaixoDoChao = false;
        for (int i = 0; i < pontos.Length; i++)
        {
            Vector2 p = Regras.PosicaoNaTrajetoria(inicio, velocidade, gravidade, (i + 1) * passoTempo);
            abaixoDoChao |= p.y < chaoY;
            pontos[i].transform.position = p;
            pontos[i].enabled = !abaixoDoChao;
        }
    }

    public void Esconder()
    {
        foreach (var p in pontos) p.enabled = false;
    }
}
