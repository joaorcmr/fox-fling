using System.Collections.Generic;

// Transforma a descrição de uma onda ("20 pardais a cada 0,7 s, depois 5
// sabiás") numa agenda de nascimentos e vai soltando os pássaros na hora certa.
public class GeradorOnda
{
    struct Nascimento
    {
        public float tempo;
        public int ordem; // desempate: mesma hora → a ordem em que aparecem nos dados
        public GrupoOnda grupo;
    }

    readonly List<Nascimento> agenda = new List<Nascimento>();
    int proximo;
    float tempo;

    public bool Terminou => proximo >= agenda.Count;

    public void Iniciar(DadosOnda onda)
    {
        agenda.Clear();
        proximo = 0;
        tempo = 0f;
        foreach (var g in onda.grupos)
            for (int i = 0; i < g.quantidade; i++)
                agenda.Add(new Nascimento { tempo = g.atraso + i * g.intervalo, ordem = agenda.Count, grupo = g });

        // List.Sort não é estável; com o desempate explícito a ordem é sempre a
        // mesma, e a simulação continua determinística.
        agenda.Sort((a, b) => a.tempo != b.tempo ? a.tempo.CompareTo(b.tempo) : a.ordem.CompareTo(b.ordem));
    }

    public void Atualizar(float dt, List<GrupoOnda> nascidos)
    {
        tempo += dt;
        while (proximo < agenda.Count && agenda[proximo].tempo <= tempo + 1e-5f)
        {
            nascidos.Add(agenda[proximo].grupo);
            proximo++;
        }
    }
}
