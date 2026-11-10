using System.Collections.Generic;
using UnityEngine;

// Pode colocar um porco (um círculo de "raio") neste ponto? Tudo é círculo
// contra círculo, ou círculo contra a trilha, então a conta é só distância.
public static class Colocacao
{
    public static bool Valida(Vector2 pos, float raio, DadosMapa mapa, Trilha trilha, IReadOnlyList<Porco> porcos)
    {
        // Dentro do mapa (a barra lateral, em x > 14, fica de fora).
        if (pos.x < raio || pos.y < raio || pos.x > mapa.tamanho.x - raio || pos.y > mapa.tamanho.y - raio) return false;

        if (trilha.DistanciaAte(pos) < mapa.meiaLargura + raio) return false;
        if (Vector2.Distance(pos, mapa.torre) < mapa.raioTorre + raio) return false;

        foreach (var o in mapa.obstaculos)
            if (Vector2.Distance(pos, o.centro) < o.raio + raio) return false;

        for (int i = 0; i < porcos.Count; i++)
            if (Vector2.Distance(pos, porcos[i].Posicao) < porcos[i].Dados.raio + raio) return false;

        return true;
    }
}
