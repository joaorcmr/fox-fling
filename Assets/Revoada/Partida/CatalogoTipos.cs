// Acha o asset de cada espécie (sprite, cor das penas, sprites do porco). A
// simulação só conhece os números; o que é para desenhar fica nos assets
// TipoPassaro e TipoPorco, e quem desenha pergunta aqui.
public class CatalogoTipos
{
    readonly TipoPassaro[] passaros;
    readonly TipoPorco[] porcos;

    public CatalogoTipos(TipoPassaro[] passaros, TipoPorco[] porcos)
    {
        this.passaros = passaros ?? new TipoPassaro[0];
        this.porcos = porcos ?? new TipoPorco[0];
    }

    public TipoPassaro De(EspeciePassaro especie)
    {
        foreach (var t in passaros)
            if (t != null && t.dados.especie == especie) return t;
        return null;
    }

    public TipoPorco De(EspeciePorco especie)
    {
        foreach (var t in porcos)
            if (t != null && t.dados.especie == especie) return t;
        return null;
    }
}
