using System.Collections.Generic;
using UnityEngine;

// Escuta os eventos da simulação e mantém um GameObject para cada coisa viva
// (pássaros, porcos, tiros), pegando e devolvendo do pool. Também dispara os
// efeitos: peninhas a cada golpe, nuvem de penas e "+N" no estouro, poeira
// quando um porco cai no mapa, lama na explosão. No Update, cada visual se
// atualiza com a interpolação entre o último passo fixo e o atual (alfa), para
// o movimento ficar liso mesmo numa tela de 120 Hz.
//
// Só desenho: os sons ficam com SonsDaPartida e a Torre apanhando, com VisualTorre.
public class GerenteVisuais : MonoBehaviour
{
    static readonly Color CorTextoPenas = new Color(1f, 0.86f, 0.35f);

    public Jogo jogo;
    public VisualPassaro prefabPassaro;
    public VisualPorco prefabPorco;
    public VisualProjetil prefabProjetil;
    public Efeitos efeitos;

    VisualPorco selecionado;

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

        var sim = jogo.Sim;
        sim.PassaroNasceu += CriarPassaro;
        sim.PassaroAtingido += (p, dano) =>
        {
            if (passaros.TryGetValue(p, out var v)) v.Piscar();
            efeitos.Penas(p.Posicao, CorDe(p), 2);
        };
        sim.PassaroEstourou += (p, penas) =>
        {
            efeitos.Penas(p.Posicao, CorDe(p), p.Dados.chefe ? 40 : 10);
            if (penas > 0) efeitos.Texto(p.Posicao, "+" + penas, CorTextoPenas);
            if (p.Dados.chefe) TremorCamera.Tremer(0.2f, 0.4f);
            RemoverPassaro(p);
        };
        sim.PassaroChegou += RemoverPassaro;

        sim.PorcoCriado += p =>
        {
            CriarPorco(p);
            efeitos.Poeira(p.Posicao);
        };
        sim.PorcoVendido += p =>
        {
            efeitos.Poeira(p.Posicao);
            RemoverPorco(p);
        };
        sim.PorcoMelhorado += p =>
        {
            efeitos.Brilho(p.Posicao);
            if (selecionado != null && selecionado.Porco == p) selecionado.MostrarAlcance(true); // o alcance pode ter crescido
        };

        sim.TiroDisparado += (porco, tiro) =>
        {
            CriarTiro(tiro);
            if (porcos.TryGetValue(porco, out var v)) v.Atirou();
        };
        sim.ProjetilSumiu += RemoverTiro;
        sim.BombaExplodiu += (centro, raio) =>
        {
            efeitos.Lama(centro, raio);
            TremorCamera.Tremer(0.05f, 0.12f);
        };

        // O porco selecionado mostra o alcance (ou a aura, no Tamboreiro).
        jogo.Selecao.SelecaoMudou += p =>
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
        var tipo = jogo.Tipos.De(p.Especie);
        return tipo != null ? tipo.corPenas : Color.white;
    }

    void CriarPassaro(Passaro p)
    {
        var v = poolPassaros.Pegar();
        v.Iniciar(p, jogo.Tipos.De(p.Especie));
        passaros[p] = v;
    }

    void RemoverPassaro(Passaro p)
    {
        if (passaros.Remove(p, out var v)) poolPassaros.Devolver(v);
    }

    void CriarPorco(Porco p)
    {
        var v = poolPorcos.Pegar();
        v.Iniciar(p, jogo.Tipos.De(p.Especie));
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
        var tipo = jogo.Tipos.De(t.Dono.Especie);
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
    }
}
