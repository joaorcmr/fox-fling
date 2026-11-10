using System.Collections.Generic;
using UnityEngine;

// As fases do jogo: cada uma é um Mapa (outro caminho, outros obstáculos) num
// turno do dia diferente. As ondas (Ondas.asset) se dividem em partes iguais
// entre elas (ver Simulacao.Fase). Sem este asset (ou com só 1 mapa), o jogo
// roda inteiro no primeiro mapa, como sempre rodou.
[CreateAssetMenu(menuName = Identidade.NomeDoJogo + "/Lista de fases")]
public class ListaFases : ScriptableObject
{
    public List<Mapa> mapas = new List<Mapa>();
}
