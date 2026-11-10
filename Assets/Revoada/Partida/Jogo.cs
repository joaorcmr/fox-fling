using UnityEngine;

// Monta a partida e a faz andar. É a ponte entre a simulação (C# puro) e o
// Unity: cria a Simulacao com os assets de dados e a avança no FixedUpdate. O
// Unity chama o FixedUpdate em passos fixos de Time.fixedDeltaTime (1/60 s
// aqui), então cada chamada é exatamente um Passo do jogo, não importa quantos
// quadros por segundo a tela tenha.
//
// Cada responsabilidade da partida mora numa classe própria, e o Jogo só as
// junta: o relógio (pausa e velocidade), o catálogo dos assets de cada
// espécie, a seleção do jogador e o registro do resultado no Progresso. Trocar
// de cena é com a Navegacao.
[DefaultExecutionOrder(-100)] // monta a partida antes de os outros componentes a usarem
public class Jogo : MonoBehaviour
{
    [Header("Dados da partida (os assets de cada assunto)")]
    public TipoPassaro[] passaros;
    public TipoPorco[] porcos;
    public ListaOndas ondas;
    public ConfigEconomia economia;
    public Mapa mapa;

    public static Jogo Atual { get; private set; }

    public Simulacao Sim { get; private set; }
    public RelogioPartida Relogio { get; private set; }
    public CatalogoTipos Tipos { get; private set; }
    public Selecao Selecao { get; private set; }
    public FimDePartida Fim { get; private set; }

    void Awake()
    {
        Atual = this;
        Time.fixedDeltaTime = Simulacao.Dt;
        Relogio = new RelogioPartida();
        Sim = new Simulacao(ConversorDados.Montar(passaros, porcos, ondas, economia, mapa));
        Tipos = new CatalogoTipos(passaros, porcos);
        Selecao = new Selecao(Sim);
        Fim = new FimDePartida(Sim);
    }

    void OnDestroy()
    {
        if (Atual == this) Atual = null;
    }

    void FixedUpdate() => Sim.Passo();
}
