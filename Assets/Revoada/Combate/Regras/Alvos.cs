using System.Collections.Generic;
using UnityEngine;

// Em quem mirar e para onde atirar.
public static class Alvos
{
    // O melhor alvo vivo dentro do alcance, segundo o modo do porco. Empates vão
    // para o mais adiantado na trilha (o mais perigoso).
    public static Passaro Escolher(Vector2 origem, float alcance, ModoAlvo modo, IReadOnlyList<Passaro> passaros)
    {
        Passaro melhor = null;
        float alcance2 = alcance * alcance;
        for (int i = 0; i < passaros.Count; i++)
        {
            var p = passaros[i];
            if (!p.Vivo || (p.Posicao - origem).sqrMagnitude > alcance2) continue;
            if (melhor == null || MelhorQue(p, melhor, origem, modo)) melhor = p;
        }
        return melhor;
    }

    static bool MelhorQue(Passaro a, Passaro b, Vector2 origem, ModoAlvo modo)
    {
        switch (modo)
        {
            case ModoAlvo.Ultimo:
                if (a.Distancia != b.Distancia) return a.Distancia < b.Distancia;
                break;
            case ModoAlvo.MaisForte:
                if (a.Vida != b.Vida) return a.Vida > b.Vida;
                break;
            case ModoAlvo.MaisPerto:
                float da = (a.Posicao - origem).sqrMagnitude, db = (b.Posicao - origem).sqrMagnitude;
                if (da != db) return da < db;
                break;
        }
        return a.Distancia > b.Distancia; // Primeiro (e o desempate dos outros modos)
    }

    // Mira com antecipação: o tiro leva um tempo t para chegar, e nesse tempo o
    // pássaro anda v·t pela TRILHA (não em linha reta). Cada iteração usa o
    // ponto previsto para recalcular t; três bastam para o erro sumir.
    public static Vector2 Antecipar(Vector2 origem, float velocidadeProjetil, Passaro alvo, Trilha trilha)
    {
        if (velocidadeProjetil <= 0f) return alvo.Posicao;
        Vector2 ponto = alvo.Posicao;
        for (int i = 0; i < 3; i++)
        {
            float t = Vector2.Distance(ponto, origem) / velocidadeProjetil;
            ponto = trilha.Posicao(Mathf.Min(alvo.Distancia + alvo.Velocidade * t, trilha.Comprimento));
        }
        return ponto;
    }
}
