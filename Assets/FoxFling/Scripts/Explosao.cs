using System.Collections.Generic;
using UnityEngine;

// Explosão em 2D: o Unity não tem "AddExplosionForce" para Rigidbody2D, então
// fazemos na mão. Para cada corpo dentro do raio: empurrão (impulso) para longe
// do centro e dano, os dois mais fortes quanto mais perto (Regras.FatorExplosao).
public static class Explosao
{
    static readonly HashSet<Rigidbody2D> atingidos = new HashSet<Rigidbody2D>();

    // poupaAtacantes: a bomba das raposas do tower defense não machuca as próprias raposas.
    public static void Explodir(Vector2 centro, float raio = 2.2f, float impulso = 14f, float danoMaximo = 120f, GameObject ignorar = null, bool poupaAtacantes = false)
    {
        Efeitos.Explosao(centro, raio);
        Sons.Tocar(Sons.I != null ? Sons.I.explosao : null);
        TremorCamera.Tremer(0.25f, 0.4f);

        atingidos.Clear();
        foreach (var colisor in Physics2D.OverlapCircleAll(centro, raio))
        {
            var corpo = colisor.attachedRigidbody;
            if (corpo == null || corpo.gameObject == ignorar || corpo.bodyType != RigidbodyType2D.Dynamic) continue;
            if (poupaAtacantes && corpo.GetComponent<RaposaAtacante>() != null) continue;
            if (!atingidos.Add(corpo)) continue; // um corpo com vários colliders só leva um empurrão

            // Distância até a borda mais próxima (não até o centro): bloco grande encostado
            // na explosão sente o tranco inteiro.
            Vector2 ponto = colisor.ClosestPoint(centro);
            float fator = Regras.FatorExplosao(Vector2.Distance(ponto, centro), raio);
            if (fator <= 0f) continue;

            Vector2 direcao = corpo.worldCenterOfMass - centro;
            if (direcao.sqrMagnitude < 0.0001f) direcao = Vector2.up;

            corpo.WakeUp();
            corpo.AddForce(direcao.normalized * impulso * fator, ForceMode2D.Impulse);

            var alvo = corpo.GetComponent<Destrutivel>();
            if (alvo != null) alvo.ReceberDano(danoMaximo * fator);
        }
    }
}
