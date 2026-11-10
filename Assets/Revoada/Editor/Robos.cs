using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// Robôs que jogam o Fox Tower inteiro sem desenhar nada, para medir o
// balanceamento. Os três "mistos" (DefesaPura, Ganancioso, Equilibrado) seguem
// a MESMA ordem de construção; só muda o que fazem com o Cofrinho. Assim,
// se um vence e outro perde, a diferença é a decisão de investir em moedas.
public enum Estrategia { Parado, SoEstilingue, DefesaPura, Ganancioso, Equilibrado }

public class ResultadoPartida
{
    public Estrategia estrategia;
    public bool venceu;
    public int ondaAlcancada;
    public int vidaFinal;
    public List<int> danoPorOnda = new List<int>();
    public List<int> moedasPorOnda = new List<int>();
    public List<int> cofrinhoNaOnda = new List<int>(); // em que onda comprou cada nível do Cofrinho

    public override string ToString() =>
        $"{estrategia}: {(venceu ? "venceu" : "perdeu")} na onda {ondaAlcancada}, vida {vidaFinal}, " +
        $"maior dano numa onda {(danoPorOnda.Count > 0 ? danoPorOnda.Max() : 0)}";
}

// O que o robô lembra entre uma decisão e outra.
public class EstadoRobo
{
    public int proximaAcao;        // posição na ordem de construção
    public int vazouNaUltimaOnda;  // pássaros que chegaram na Torre na última onda
    public int vazouNestaOnda;
}

public static class Robos
{
    // O Equilibrado compra o próximo Cofrinho quando ele se paga em até tantas ondas.
    public static int OndasParaPagarEquilibrado = 6;
    const int PassosEntreDecisoes = 15; // 4 decisões por segundo de jogo

    enum TipoAcao { Comprar, Melhorar, Especializar }

    struct Acao
    {
        public TipoAcao tipo;
        public EspeciePorco especie;
        public int escolha; // especialização: 0 = A, 1 = B
    }

    static Acao C(EspeciePorco e) => new Acao { tipo = TipoAcao.Comprar, especie = e };
    static Acao M(EspeciePorco e) => new Acao { tipo = TipoAcao.Melhorar, especie = e };
    static Acao E(EspeciePorco e, int escolha) => new Acao { tipo = TipoAcao.Especializar, especie = e, escolha = escolha };

    const EspeciePorco Est = EspeciePorco.Estilingueiro, Arp = EspeciePorco.Arpoador,
        Bom = EspeciePorco.Bombardeiro, Tam = EspeciePorco.Tamboreiro;

    // A ordem de construção dos robôs mistos, repetida em ciclo quando acaba.
    // "Melhorar" (M) vale para o porco mais fraco daquela espécie;
    // "Especializar" (A ou B) para o primeiro que estiver no nível 3.
    // Formato: C = comprar, M = melhorar, A/B = especializar; Est, Arp, Bom, Tam.
    // Começa com três Estilingueiros fortes (segura o começo barato), depois
    // Arpoador com Ponta de Ferro e Bombardeiro com lama (contra a couraça dos
    // urubus), o Tamboreiro junto do grupo e as especializações.
    public const string OrdemPadrao =
        "C.Est C.Est M.Est C.Est M.Est M.Est C.Arp M.Est M.Est A.Est " +
        "C.Bom M.Arp M.Arp A.Est C.Tam M.Bom M.Bom C.Est M.Est M.Est " +
        "A.Est A.Arp M.Tam B.Bom C.Arp M.Arp M.Arp B.Arp A.Tam";

    static Acao[] Ordem = Ler(OrdemPadrao);

    // Troca a ordem de construção (para experimentar pelo eval sem recompilar).
    public static void DefinirOrdem(string texto) => Ordem = Ler(texto ?? OrdemPadrao);

    static Acao[] Ler(string texto)
    {
        var acoes = new List<Acao>();
        foreach (var token in texto.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries))
        {
            var partes = token.Split('.');
            var especie = partes[1] == "Est" ? Est : partes[1] == "Arp" ? Arp : partes[1] == "Bom" ? Bom : Tam;
            switch (partes[0])
            {
                case "C": acoes.Add(C(especie)); break;
                case "M": acoes.Add(M(especie)); break;
                case "A": acoes.Add(E(especie, 0)); break;
                default: acoes.Add(E(especie, 1)); break;
            }
        }
        return acoes.ToArray();
    }

    // ------------------------------------------------------------------
    public static ResultadoPartida Jogar(DadosJogo dados, Estrategia estrategia)
    {
        var sim = new Simulacao(dados) { Auto = true };
        var estado = new EstadoRobo();
        var r = new ResultadoPartida { estrategia = estrategia };
        int vidaNoInicio = sim.Vida, moedasNaOnda = 0;

        sim.PassaroChegou += p => estado.vazouNestaOnda++;
        sim.PassaroEstourou += (p, moedas) => moedasNaOnda += moedas;
        sim.OndaTerminou += (onda, bonus) =>
        {
            r.danoPorOnda.Add(vidaNoInicio - sim.Vida);
            r.moedasPorOnda.Add(moedasNaOnda + bonus);
            vidaNoInicio = sim.Vida;
            moedasNaOnda = 0;
            estado.vazouNaUltimaOnda = estado.vazouNestaOnda;
            estado.vazouNestaOnda = 0;
        };
        sim.FimDeJogo += venceu =>
        {
            if (!venceu) r.danoPorOnda.Add(vidaNoInicio - sim.Vida);
        };

        const int limite = 60 * 60 * 90; // 90 minutos de jogo: nenhuma partida dura tanto
        for (int passo = 0; passo < limite && !Acabou(sim); passo++)
        {
            if (passo % PassosEntreDecisoes == 0)
            {
                int nivelAntes = sim.Economia.NivelCofrinho;
                Decidir(sim, estrategia, estado);
                if (sim.Economia.NivelCofrinho > nivelAntes) r.cofrinhoNaOnda.Add(sim.Onda);
            }
            if (sim.Estado == EstadoJogo.Preparando) sim.SoltarOnda(); // o Auto não solta a primeira
            sim.Passo();
        }

        r.venceu = sim.Estado == EstadoJogo.Vitoria;
        r.ondaAlcancada = sim.Onda;
        r.vidaFinal = sim.Vida;
        return r;
    }

    static bool Acabou(Simulacao s) => s.Estado == EstadoJogo.Vitoria || s.Estado == EstadoJogo.Derrota;

    // Uma decisão: o que comprar ou melhorar agora (pode fazer várias coisas
    // seguidas se tiver moedas para todas).
    public static void Decidir(Simulacao sim, Estrategia estrategia, EstadoRobo estado)
    {
        if (estrategia == Estrategia.Parado || Acabou(sim)) return;
        if (estrategia == Estrategia.SoEstilingue)
        {
            DecidirSoEstilingue(sim);
            return;
        }

        var cofrinho = sim.Economia.ProximoCofrinho;
        if (cofrinho != null)
        {
            // Ganancioso: depois dos 2 primeiros Estilingueiros, guarda tudo para o Cofrinho.
            if (estrategia == Estrategia.Ganancioso && estado.proximaAcao >= 2)
            {
                if (sim.Moedas >= cofrinho.custo) sim.ComprarCofrinho();
                return;
            }
            // Equilibrado: compra (e guarda moedas para) o Cofrinho só quando ele se paga
            // rápido e a defesa está segurando (ninguém chegou na Torre na última onda).
            if (estrategia == Estrategia.Equilibrado && estado.vazouNaUltimaOnda == 0)
            {
                int? ondas = sim.OndasParaPagarCofrinho();
                if (ondas.HasValue && ondas.Value <= OndasParaPagarEquilibrado)
                {
                    if (sim.Moedas >= cofrinho.custo) sim.ComprarCofrinho();
                    return;
                }
            }
        }

        for (int tentativa = 0; tentativa < Ordem.Length; tentativa++)
        {
            var r = Executar(sim, Ordem[estado.proximaAcao % Ordem.Length]);
            if (r == Resultado.SemMoedas) return; // espera juntar
            estado.proximaAcao++;                 // fez, ou não dá para fazer: segue a ordem
        }
    }

    static void DecidirSoEstilingue(Simulacao sim)
    {
        for (int tentativa = 0; tentativa < 50; tentativa++)
        {
            var fraco = sim.Porcos.Where(p => p.Especie == Est && p.Nivel < 3).OrderBy(p => p.Nivel).ThenBy(p => p.Id).FirstOrDefault();
            if (fraco != null)
            {
                if (sim.Melhorar(fraco) != Resultado.Ok) return;
                continue;
            }
            var pronto = sim.Porcos.Where(p => p.Especie == Est && p.Nivel == 3).OrderBy(p => p.Id).FirstOrDefault();
            if (pronto != null)
            {
                if (sim.Especializar(pronto, pronto.Id % 2) != Resultado.Ok) return;
                continue;
            }
            if (Executar(sim, C(Est)) != Resultado.Ok) return;
        }
    }

    static Resultado Executar(Simulacao sim, Acao acao)
    {
        if (!sim.Dados.porcos.TryGetValue(acao.especie, out var dados)) return Resultado.Indisponivel;
        switch (acao.tipo)
        {
            case TipoAcao.Comprar:
                if (sim.Moedas < dados.custo) return Resultado.SemMoedas;
                foreach (var lugar in LugaresBons(sim, acao.especie))
                    return sim.Comprar(acao.especie, lugar, out _);
                return Resultado.Indisponivel; // mapa cheio
            case TipoAcao.Melhorar:
                var fraco = sim.Porcos.Where(p => p.Especie == acao.especie && p.Nivel < 3).OrderBy(p => p.Nivel).ThenBy(p => p.Id).FirstOrDefault();
                return fraco == null ? Resultado.Indisponivel : sim.Melhorar(fraco);
            default:
                var pronto = sim.Porcos.Where(p => p.Especie == acao.especie && p.Nivel == 3).OrderBy(p => p.Id).FirstOrDefault();
                return pronto == null ? Resultado.Indisponivel : sim.Especializar(pronto, acao.escolha);
        }
    }

    // ------------------------------------------------------------------
    // Onde colocar: pontos válidos de uma grade de 0,5, do melhor para o pior.
    // Quem atira: quanto mais trilha dentro do alcance, melhor. Tamboreiro:
    // quantos porcos que atiram ficam dentro da aura.
    // ------------------------------------------------------------------
    public static List<Vector2> LugaresBons(Simulacao sim, EspeciePorco especie)
    {
        var dados = sim.Dados.porcos[especie];
        var mapa = sim.Dados.mapa;
        var amostras = AmostrasDaTrilha(sim.Trilha);
        bool tambor = dados.nivel1.tiro == TipoTiro.Nenhum;
        var notas = new List<(Vector2 lugar, float nota)>();

        for (float x = 0.5f; x < mapa.tamanho.x; x += 0.5f)
            for (float y = 0.5f; y < mapa.tamanho.y; y += 0.5f)
            {
                var p = new Vector2(x, y);
                if (!sim.PodeColocar(especie, p)) continue;
                float nota = tambor
                    ? sim.Porcos.Count(o => o.Ataca && Vector2.Distance(o.Posicao, p) <= dados.nivel1.raioAura) * 1000f
                      + Cobertura(amostras, p, dados.nivel1.raioAura)
                    : Cobertura(amostras, p, dados.nivel1.alcance);
                notas.Add((p, nota));
            }

        return notas.OrderByDescending(n => n.nota).ThenBy(n => n.lugar.x).ThenBy(n => n.lugar.y).Select(n => n.lugar).ToList();
    }

    static readonly Dictionary<Trilha, List<Vector2>> cacheAmostras = new Dictionary<Trilha, List<Vector2>>();

    static List<Vector2> AmostrasDaTrilha(Trilha trilha)
    {
        if (cacheAmostras.TryGetValue(trilha, out var lista)) return lista;
        if (cacheAmostras.Count > 16) cacheAmostras.Clear();
        lista = new List<Vector2>();
        for (float d = 0f; d <= trilha.Comprimento; d += 0.25f) lista.Add(trilha.Posicao(d));
        cacheAmostras[trilha] = lista;
        return lista;
    }

    static float Cobertura(List<Vector2> amostras, Vector2 p, float alcance)
    {
        float a2 = alcance * alcance;
        int dentro = 0;
        foreach (var s in amostras)
            if ((s - p).sqrMagnitude <= a2) dentro++;
        return dentro;
    }

    // ------------------------------------------------------------------
    // Joga como o Equilibrado, sem desenhar, até a onda pedida começar (para
    // os prints de ondas avançadas). Funciona na simulação ao vivo do Play.
    public static void AvancarAteOnda(Simulacao sim, int onda)
    {
        var estado = new EstadoRobo();
        bool autoAntes = sim.Auto;
        sim.Auto = true;
        for (int passo = 0; passo < 60 * 60 * 90 && !Acabou(sim) && !(sim.Onda >= onda && sim.Estado == EstadoJogo.EmOnda); passo++)
        {
            if (passo % PassosEntreDecisoes == 0) Decidir(sim, Estrategia.Equilibrado, estado);
            if (sim.Estado == EstadoJogo.Preparando) sim.SoltarOnda();
            if (sim.Onda + 1 >= onda && sim.Estado == EstadoJogo.Intervalo) sim.SoltarOnda();
            sim.Passo();
        }
        sim.Auto = autoAntes;
    }

    public static string Relatorio(DadosJogo dados)
    {
        var texto = new StringBuilder($"Balanceamento do {Identidade.NomeDoJogo} (robôs):\n");
        foreach (Estrategia e in System.Enum.GetValues(typeof(Estrategia)))
        {
            var r = Jogar(dados, e);
            texto.AppendLine(r.ToString());
            texto.AppendLine("   dano por onda: " + string.Join(" ", r.danoPorOnda));
            texto.AppendLine("   moedas por onda: " + string.Join(" ", r.moedasPorOnda));
            if (r.cofrinhoNaOnda.Count > 0) texto.AppendLine("   Cofrinho comprado nas ondas: " + string.Join(" ", r.cofrinhoNaOnda));
        }
        return texto.ToString();
    }
}
