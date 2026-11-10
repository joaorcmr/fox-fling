using System.Collections.Generic;

// Junta os assets num DadosJogo, o pacote que a Simulacao lê.
public static class ConversorDados
{
    public static DadosJogo Montar(IEnumerable<TipoPassaro> passaros, IEnumerable<TipoPorco> porcos,
        ListaOndas ondas, ConfigEconomia economia, Mapa mapa, ListaFases fases = null)
    {
        var d = new DadosJogo();
        if (passaros != null)
            foreach (var p in passaros)
                if (p != null) d.passaros[p.dados.especie] = p.dados;
        if (porcos != null)
            foreach (var p in porcos)
                if (p != null) d.porcos[p.dados.especie] = p.dados;
        if (ondas != null) d.ondas.AddRange(ondas.ondas);
        if (economia != null) d.economia = economia.dados;
        if (mapa != null) d.mapa = mapa.dados;
        if (fases != null)
            foreach (var m in fases.mapas)
                if (m != null) d.fases.Add(m.dados);
        return d;
    }
}
