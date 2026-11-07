using System.Collections.Generic;
using UnityEngine;

// Escuta os eventos da simulação e mantém um GameObject para cada coisa viva:
// pássaro nasceu → pega um visual do pool; estourou ou chegou na Torre →
// devolve. No Update, cada visual se atualiza com a interpolação entre o
// último passo fixo e o atual (alfa), para o movimento ficar liso mesmo numa
// tela de 120 Hz.
public class GerenteVisuais : MonoBehaviour
{
    public Jogo jogo;
    public VisualPassaro prefabPassaro;

    readonly Dictionary<Passaro, VisualPassaro> passaros = new Dictionary<Passaro, VisualPassaro>();
    Pool<VisualPassaro> poolPassaros;

    void Start()
    {
        poolPassaros = new Pool<VisualPassaro>(prefabPassaro, transform);
        var sim = jogo.Sim;
        sim.PassaroNasceu += CriarPassaro;
        sim.PassaroAtingido += (p, dano) =>
        {
            if (passaros.TryGetValue(p, out var v)) v.Piscar();
        };
        sim.PassaroEstourou += (p, penas) => RemoverPassaro(p);
        sim.PassaroChegou += RemoverPassaro;
        foreach (var p in sim.Passaros) CriarPassaro(p);
    }

    void CriarPassaro(Passaro p)
    {
        var v = poolPassaros.Pegar();
        v.Iniciar(p, jogo.TipoDe(p.Especie));
        passaros[p] = v;
    }

    void RemoverPassaro(Passaro p)
    {
        if (passaros.Remove(p, out var v)) poolPassaros.Devolver(v);
    }

    void Update()
    {
        float alfa = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);
        foreach (var v in passaros.Values) v.Atualizar(alfa);
    }
}
