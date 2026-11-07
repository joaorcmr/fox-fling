using System;
using System.Collections.Generic;
using UnityEngine;

// O jogo inteiro, sem desenho. Avança em passos fixos de 1/60 s (Passo) e avisa
// por eventos o que aconteceu; a camada Unity (Jogo, GerenteVisuais, Hud) só
// escuta e desenha. Como nada aqui depende de quadros por segundo, de física ou
// de sorte, os testes e os robôs de balanceamento jogam partidas inteiras sem
// dar Play.
//
// A classe é "partial": este arquivo tem o estado, as ondas e a Torre; os
// porcos e o combate ficam em outros arquivos da mesma classe.
public partial class Simulacao
{
    public const float Dt = 1f / 60f;

    public readonly DadosJogo Dados;
    public readonly Trilha Trilha;
    public readonly Economia Economia;

    public EstadoJogo Estado { get; private set; } = EstadoJogo.Preparando;
    public int Vida { get; private set; }
    public int Onda { get; private set; }     // em andamento, ou a última concluída (0 antes da primeira)
    public int TotalOndas => Dados.ondas.Count;
    public int Penas => Economia.Penas;
    public bool Auto { get; set; }
    public float Tempo { get; private set; }

    readonly List<Passaro> passaros = new List<Passaro>();
    public IReadOnlyList<Passaro> Passaros => passaros;

    readonly List<float> penasBasePorOnda = new List<float>();
    public IReadOnlyList<float> PenasBasePorOnda => penasBasePorOnda;

    readonly GeradorOnda gerador = new GeradorOnda();
    readonly List<GrupoOnda> nascidos = new List<GrupoOnda>();
    float esperaAuto;
    int proximoId = 1;

    public event Action<Passaro> PassaroNasceu;
    public event Action<Passaro, float> PassaroAtingido;  // dano causado
    public event Action<Passaro, int> PassaroEstourou;    // penas que entraram
    public event Action<Passaro> PassaroChegou;           // chegou na Torre
    public event Action<int> TorreAtingida;               // dano
    public event Action<int> OndaComecou;
    public event Action<int, int> OndaTerminou;           // onda, bônus
    public event Action<bool> FimDeJogo;                  // venceu?

    public Simulacao(DadosJogo dados)
    {
        Dados = dados;
        Trilha = new Trilha(dados.mapa.pontos, dados.mapa.raioCanto);
        Economia = new Economia(dados.economia);
        Vida = dados.economia.vidaInicial;
        foreach (var onda in dados.ondas) penasBasePorOnda.Add(PenasDaOnda(onda));
    }

    bool Acabou => Estado == EstadoJogo.Vitoria || Estado == EstadoJogo.Derrota;

    // ------------------------------------------------------------------
    // Comandos
    // ------------------------------------------------------------------
    public bool SoltarOnda()
    {
        if (Estado != EstadoJogo.Preparando && Estado != EstadoJogo.Intervalo) return false;
        if (Onda >= TotalOndas) return false;
        Onda++;
        Estado = EstadoJogo.EmOnda;
        gerador.Iniciar(Dados.ondas[Onda - 1]);
        OndaComecou?.Invoke(Onda);
        return true;
    }

    public Resultado ComprarNinho()
    {
        if (Acabou || Economia.ProximoNinho == null) return Resultado.Indisponivel;
        return Economia.ComprarNinho() ? Resultado.Ok : Resultado.SemPenas;
    }

    // Conta a partir da próxima onda que ainda vai começar.
    public int? OndasParaPagarNinho() => Economia.OndasParaPagar(penasBasePorOnda, Onda + 1);

    // Causa dano direto num pássaro. É por aqui que todo tiro machuca. A
    // couraça (urubu) corta pela metade, a não ser que o golpe a ignore (bomba,
    // arpão com Ponta de Ferro).
    public void Atingir(Passaro p, float dano, bool ignoraCouraca)
    {
        if (p == null || !p.Vivo || Acabou) return;
        float efetivo = p.Dados.couraca && !ignoraCouraca ? dano * 0.5f : dano;
        p.Vida -= efetivo;
        PassaroAtingido?.Invoke(p, efetivo);
        if (p.Vida <= 0f) Estourar(p);
    }

    // Quanto a mais rendem as penas de um pássaro estourado neste ponto
    // (a aura do Tambor das Penas entra aqui).
    public float BonusPenas(Vector2 posicao) => 1f;

    // ------------------------------------------------------------------
    // Consultas para a interface
    // ------------------------------------------------------------------
    public List<KeyValuePair<EspeciePassaro, int>> ResumoOnda(int numero)
    {
        var resumo = new List<KeyValuePair<EspeciePassaro, int>>();
        if (numero < 1 || numero > TotalOndas) return resumo;
        foreach (var g in Dados.ondas[numero - 1].grupos)
        {
            int i = resumo.FindIndex(k => k.Key == g.especie);
            if (i < 0) resumo.Add(new KeyValuePair<EspeciePassaro, int>(g.especie, g.quantidade));
            else resumo[i] = new KeyValuePair<EspeciePassaro, int>(g.especie, resumo[i].Value + g.quantidade);
        }
        return resumo;
    }

    // Pássaros que aparecem pela primeira vez nesta onda ("Novo: Urubu").
    public List<EspeciePassaro> Estreias(int numero)
    {
        var vistos = new HashSet<EspeciePassaro>();
        for (int onda = 1; onda < numero && onda <= TotalOndas; onda++)
            foreach (var g in Dados.ondas[onda - 1].grupos) vistos.Add(g.especie);
        var novos = new List<EspeciePassaro>();
        foreach (var kv in ResumoOnda(numero))
            if (!vistos.Contains(kv.Key)) novos.Add(kv.Key);
        return novos;
    }

    // ------------------------------------------------------------------
    // O passo do jogo (1/60 s), sempre na mesma ordem
    // ------------------------------------------------------------------
    public void Passo()
    {
        if (Acabou) return;
        Tempo += Dt;

        foreach (var p in passaros) p.PosicaoAnterior = p.Posicao;   // (1) para a tela interpolar

        if (Estado == EstadoJogo.EmOnda)                              // (2) nascimentos
        {
            nascidos.Clear();
            gerador.Atualizar(Dt, nascidos);
            foreach (var g in nascidos) Nascer(g.especie, g.multiplicadorVida, 0f);
        }

        MoverPassaros();                                             // (3) andar e chegar na Torre
        if (Acabou) return;                                          //     derrota: o mundo para aqui

        PassoPorcos();                                               // (4) porcos miram e atiram
        PassoTiros();                                                // (5) tiros voam e acertam

        passaros.RemoveAll(p => !p.Vivo);                            // (6) limpeza

        if (Estado == EstadoJogo.EmOnda && gerador.Terminou && passaros.Count == 0)
            TerminarOnda();                                          // (7) fim de onda

        if (Estado == EstadoJogo.Intervalo && Auto)                  // (8) Auto
        {
            esperaAuto -= Dt;
            if (esperaAuto <= 0f) SoltarOnda();
        }
    }

    partial void PassoPorcos();
    partial void PassoTiros();

    Passaro Nascer(EspeciePassaro especie, float multiplicadorVida, float distancia)
    {
        var p = new Passaro(proximoId++, Dados.passaros[especie], multiplicadorVida, distancia);
        p.Posicao = p.PosicaoAnterior = Trilha.Posicao(distancia);
        passaros.Add(p);
        PassaroNasceu?.Invoke(p);
        return p;
    }

    void MoverPassaros()
    {
        for (int i = 0; i < passaros.Count; i++)
        {
            var p = passaros[i];
            if (!p.Vivo) continue;
            p.Distancia += p.Velocidade * Dt;
            if (p.Distancia >= Trilha.Comprimento)
            {
                p.Distancia = Trilha.Comprimento;
                p.Posicao = Trilha.Posicao(p.Distancia);
                ChegouNaTorre(p);
                if (Acabou) return;
                continue;
            }
            p.Posicao = Trilha.Posicao(p.Distancia);
        }
    }

    void ChegouNaTorre(Passaro p)
    {
        p.Vivo = false;
        int dano = p.Dados.danoTorre;
        Vida = Mathf.Max(0, Vida - dano);
        PassaroChegou?.Invoke(p);
        TorreAtingida?.Invoke(dano);
        if (Vida <= 0)
        {
            Estado = EstadoJogo.Derrota;
            FimDeJogo?.Invoke(false);
        }
    }

    void Estourar(Passaro p)
    {
        p.Vivo = false;
        int ganhas = Economia.Creditar(p.Dados.penas * Economia.Multiplicador * BonusPenas(p.Posicao));
        PassaroEstourou?.Invoke(p, ganhas);
    }

    void TerminarOnda()
    {
        int bonus = Economia.Bonus(Onda);
        Economia.Creditar(bonus);
        OndaTerminou?.Invoke(Onda, bonus);
        if (Onda >= TotalOndas)
        {
            Estado = EstadoJogo.Vitoria;
            FimDeJogo?.Invoke(true);
        }
        else
        {
            Estado = EstadoJogo.Intervalo;
            esperaAuto = Dados.economia.esperaAuto;
        }
    }

    float PenasDaOnda(DadosOnda onda)
    {
        float total = 0f;
        foreach (var g in onda.grupos) total += g.quantidade * PenasDe(g.especie, 0);
        return total;
    }

    // Penas de um pássaro mais as dos filhos que ele solta (a pata e os patinhos).
    float PenasDe(EspeciePassaro especie, int profundidade)
    {
        if (!Dados.passaros.TryGetValue(especie, out var dados)) return 0f;
        float total = dados.penas;
        if (dados.quantidadeFilhos > 0 && profundidade < 3)
            total += dados.quantidadeFilhos * PenasDe(dados.filho, profundidade + 1);
        return total;
    }
}
