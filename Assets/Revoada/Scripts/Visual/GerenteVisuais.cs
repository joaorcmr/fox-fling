using System.Collections.Generic;
using UnityEngine;

// Escuta os eventos da simulação e mantém um GameObject para cada coisa viva
// (pássaros, porcos, tiros), pegando e devolvendo do pool. Também dispara os
// efeitos: peninhas a cada golpe, nuvem de penas e "+N" no estouro, poeira
// quando um porco cai no mapa, lama na explosão. No Update, cada visual se
// atualiza com a interpolação entre o último passo fixo e o atual (alfa), para
// o movimento ficar liso mesmo numa tela de 120 Hz.
public class GerenteVisuais : MonoBehaviour
{
    static readonly Color CorTextoPenas = new Color(1f, 0.86f, 0.35f);

    public Jogo jogo;
    public VisualPassaro prefabPassaro;
    public VisualPorco prefabPorco;
    public VisualProjetil prefabProjetil;
    public Efeitos efeitos;
    public Controles controles;
    [Tooltip("A Torre desenhada no mapa: treme e fica vermelha quando apanha.")]
    public SpriteRenderer torre;

    VisualPorco selecionado;
    Vector3 posicaoTorre;
    float fimGolpeTorre;

    readonly Dictionary<Passaro, VisualPassaro> passaros = new Dictionary<Passaro, VisualPassaro>();
    readonly Dictionary<Porco, VisualPorco> porcos = new Dictionary<Porco, VisualPorco>();
    readonly Dictionary<Projetil, VisualProjetil> tiros = new Dictionary<Projetil, VisualProjetil>();
    Pool<VisualPassaro> poolPassaros;
    Pool<VisualPorco> poolPorcos;
    Pool<VisualProjetil> poolTiros;

    public VisualPorco VisualDe(Porco p) => p != null && porcos.TryGetValue(p, out var v) ? v : null;

    void Start()
    {
        poolPassaros = new Pool<VisualPassaro>(prefabPassaro, transform);
        poolPorcos = new Pool<VisualPorco>(prefabPorco, transform);
        poolTiros = new Pool<VisualProjetil>(prefabProjetil, transform);

        if (torre != null) posicaoTorre = torre.transform.position;
        var sim = jogo.Sim;
        var sons = Sons.Instancia;
        sim.PassaroNasceu += p =>
        {
            CriarPassaro(p);
            if (p.Dados.chefe && sons != null) Sons.Tocar(sons.chefe, 1f, 0f);
        };
        sim.PassaroAtingido += (p, dano) =>
        {
            if (passaros.TryGetValue(p, out var v)) v.Piscar();
            efeitos.Penas(p.Posicao, CorDe(p), 2);
        };
        sim.PassaroEstourou += (p, penas) =>
        {
            efeitos.Penas(p.Posicao, CorDe(p), p.Dados.chefe ? 40 : 10);
            if (penas > 0) efeitos.Texto(p.Posicao, "+" + penas, CorTextoPenas);
            Sons.TocarEstouro(p.Dados.chefe ? 1f : 0.55f);
            if (p.Dados.chefe) TremorCamera.Tremer(0.2f, 0.4f);
            RemoverPassaro(p);
        };
        sim.PassaroChegou += RemoverPassaro;
        sim.TorreAtingida += dano =>
        {
            fimGolpeTorre = Time.unscaledTime + 0.3f;
            TremorCamera.Tremer(Mathf.Min(0.25f, 0.06f + dano * 0.01f), 0.25f);
            if (sons != null) Sons.Tocar(sons.torre, 0.9f);
        };
        sim.OndaComecou += n =>
        {
            if (sons != null) Sons.Tocar(sons.onda, 0.8f, 0f);
        };

        sim.PorcoCriado += p =>
        {
            CriarPorco(p);
            efeitos.Poeira(p.Posicao);
            if (sons != null) Sons.Tocar(sons.compra, 0.8f);
        };
        sim.PorcoVendido += p =>
        {
            efeitos.Poeira(p.Posicao);
            if (sons != null) Sons.Tocar(sons.venda, 0.8f);
            RemoverPorco(p);
        };

        sim.TiroDisparado += (porco, tiro) =>
        {
            CriarTiro(tiro);
            if (porcos.TryGetValue(porco, out var v)) v.Atirou();
            if (sons == null) return;
            var clip = tiro.Tipo == TipoTiro.Arpao ? sons.tiroArpao : tiro.Tipo == TipoTiro.Bomba ? sons.tiroBomba : sons.tiroEstilingue;
            Sons.Tocar(clip, 0.3f);
        };
        sim.ProjetilSumiu += RemoverTiro;
        sim.BombaExplodiu += (centro, raio) =>
        {
            efeitos.Lama(centro, raio);
            TremorCamera.Tremer(0.05f, 0.12f);
            if (sons != null) Sons.Tocar(sons.explosao, 0.55f);
        };
        sim.PorcoMelhorado += p =>
        {
            efeitos.Brilho(p.Posicao);
            if (sons != null) Sons.Tocar(sons.upgrade, 0.9f, 0f);
            if (selecionado != null && selecionado.Porco == p) selecionado.MostrarAlcance(true); // o alcance pode ter crescido
        };

        // O porco selecionado mostra o alcance (ou a aura, no Tamboreiro).
        if (controles != null)
            controles.SelecaoMudou += p =>
            {
                if (selecionado != null) selecionado.MostrarAlcance(false);
                selecionado = VisualDe(p);
                if (selecionado != null) selecionado.MostrarAlcance(true);
            };

        foreach (var p in sim.Passaros) CriarPassaro(p);
        foreach (var p in sim.Porcos) CriarPorco(p);
    }

    Color CorDe(Passaro p)
    {
        var tipo = jogo.TipoDe(p.Especie);
        return tipo != null ? tipo.corPenas : Color.white;
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

    void CriarPorco(Porco p)
    {
        var v = poolPorcos.Pegar();
        v.Iniciar(p, jogo.TipoDe(p.Especie));
        porcos[p] = v;
    }

    void RemoverPorco(Porco p)
    {
        if (!porcos.Remove(p, out var v)) return;
        if (v == selecionado) selecionado = null;
        v.MostrarAlcance(false);
        poolPorcos.Devolver(v);
    }

    void CriarTiro(Projetil t)
    {
        var v = poolTiros.Pegar();
        var tipo = jogo.TipoDe(t.Dono.Especie);
        v.Iniciar(t, tipo != null ? tipo.projetil : null);
        tiros[t] = v;
    }

    void RemoverTiro(Projetil t)
    {
        if (tiros.Remove(t, out var v)) poolTiros.Devolver(v);
    }

    void Update()
    {
        float alfa = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);
        foreach (var v in passaros.Values) v.Atualizar(alfa);
        foreach (var v in porcos.Values) v.Atualizar();
        foreach (var v in tiros.Values) v.Atualizar(alfa);

        // A Torre apanhando: treme para os lados e fica avermelhada por 0,3 s.
        if (torre == null) return;
        float k = Mathf.Clamp01((fimGolpeTorre - Time.unscaledTime) / 0.3f);
        torre.transform.position = posicaoTorre + Vector3.right * Mathf.Sin(Time.unscaledTime * 70f) * 0.07f * k;
        torre.color = Color.Lerp(Color.white, new Color(1f, 0.5f, 0.45f), k);
    }
}
