using System;

// O jogo inteiro, sem desenho. Avança em passos fixos de 1/60 s (Passo) e avisa
// por eventos o que aconteceu; a camada Unity (Jogo, GerenteVisuais, Hud) só
// escuta e desenha. Como nada aqui depende de quadros por segundo, de física ou
// de sorte, os testes e os robôs de balanceamento jogam partidas inteiras sem
// dar Play.
//
// A classe é "partial" e cada assunto do jogo guarda a sua parte na própria
// pasta: aqui ficam o estado da partida e a ordem do passo; os pássaros, as
// ondas, a Torre, as moedas, os porcos e o combate ficam em Simulacao.*.cs, na
// pasta Regras de cada assunto.
public partial class Simulacao
{
    public const float Dt = 1f / 60f;

    public readonly DadosJogo Dados;
    public readonly Trilha Trilha;

    public EstadoJogo Estado { get; private set; } = EstadoJogo.Preparando;
    public bool Auto { get; set; }
    public float Tempo { get; private set; }

    public event Action<bool> FimDeJogo;                  // venceu?

    public Simulacao(DadosJogo dados)
    {
        Dados = dados;
        Trilha = new Trilha(dados.mapa.pontos, dados.mapa.raioCanto);
        Economia = new Economia(dados.economia);
        Vida = dados.economia.vidaInicial;
        CalcularMoedasDasOndas();
    }

    bool Acabou => Estado == EstadoJogo.Vitoria || Estado == EstadoJogo.Derrota;

    // ------------------------------------------------------------------
    // O passo do jogo (1/60 s), sempre na mesma ordem
    // ------------------------------------------------------------------
    public void Passo()
    {
        if (Acabou) return;
        Tempo += Dt;

        foreach (var p in passaros) p.PosicaoAnterior = p.Posicao;   // (1) para a tela interpolar

        if (Estado == EstadoJogo.EmOnda) NascerDaOnda();             // (2) nascimentos

        MoverPassaros();                                             // (3) andar e chegar na Torre
        if (Acabou) return;                                          //     derrota: o mundo para aqui

        PassoPorcos();                                               // (4) porcos miram e atiram
        PassoTiros();                                                // (5) tiros voam e acertam

        passaros.RemoveAll(p => !p.Vivo);                            // (6) limpeza

        if (Estado == EstadoJogo.EmOnda && gerador.Terminou && passaros.Count == 0)
            TerminarOnda();                                          // (7) fim de onda

        if (Estado == EstadoJogo.Intervalo && Auto)                  // (8) Auto
            EsperarAuto();
    }

    partial void PassoPorcos();
    partial void PassoTiros();
}
