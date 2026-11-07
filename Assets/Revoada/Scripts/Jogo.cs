using UnityEngine;

// A ponte entre a simulação (C# puro) e o Unity. Ele monta a Simulacao com os
// assets de dados e a avança no FixedUpdate: o Unity chama o FixedUpdate em
// passos fixos de Time.fixedDeltaTime (1/60 s aqui), então cada chamada é
// exatamente um Passo do jogo, não importa quantos quadros por segundo a tela
// tenha. Pausar e acelerar (Task 8) é só mudar o Time.timeScale.
[DefaultExecutionOrder(-100)] // monta a simulação antes de os outros componentes a usarem
public class Jogo : MonoBehaviour
{
    [Header("Dados (Assets/Revoada/Dados)")]
    public TipoPassaro[] passaros;
    public TipoPorco[] porcos;
    public ListaOndas ondas;
    public ConfigEconomia economia;
    public Mapa mapa;

    public static Jogo Atual { get; private set; }
    public Simulacao Sim { get; private set; }

    void Awake()
    {
        Atual = this;
        Time.fixedDeltaTime = Simulacao.Dt;
        Time.timeScale = 1f;
        Sim = new Simulacao(ConversorDados.Montar(passaros, porcos, ondas, economia, mapa));
    }

    void OnDestroy()
    {
        if (Atual == this) Atual = null;
    }

    void FixedUpdate() => Sim.Passo();

    // ------------------------------------------------------------------
    // Tempo: pausa e velocidade são só o Time.timeScale do Unity. Com 2× ou 3×
    // o Unity chama o FixedUpdate 2 ou 3 vezes por quadro; com 0 não chama
    // nenhuma. Os comandos (comprar, melhorar) continuam funcionando pausado,
    // porque não dependem do passo.
    // ------------------------------------------------------------------
    public int Velocidade { get; private set; } = 1;
    public bool Pausado { get; private set; }

    public void AlternarPausa()
    {
        Pausado = !Pausado;
        AplicarTempo();
    }

    public void Pausar(bool pausar)
    {
        Pausado = pausar;
        AplicarTempo();
    }

    public void ProximaVelocidade()
    {
        Velocidade = Velocidade >= 3 ? 1 : Velocidade + 1;
        AplicarTempo();
    }

    void AplicarTempo() => Time.timeScale = Pausado ? 0f : Velocidade;

    public void Reiniciar()
    {
        Time.timeScale = 1f; // a próxima cena não pode nascer pausada nem acelerada
        UnityEngine.SceneManagement.SceneManager.LoadScene(gameObject.scene.name);
    }

    public void IrParaMenu()
    {
        Time.timeScale = 1f;
        // O Menu entra nas Build Settings na Task 10; até lá, volta para o jogo.
        string cena = Application.CanStreamedLevelBeLoaded("Menu") ? "Menu" : gameObject.scene.name;
        UnityEngine.SceneManagement.SceneManager.LoadScene(cena);
    }

    public TipoPassaro TipoDe(EspeciePassaro especie)
    {
        foreach (var t in passaros)
            if (t != null && t.dados.especie == especie) return t;
        return null;
    }

    public TipoPorco TipoDe(EspeciePorco especie)
    {
        foreach (var t in porcos)
            if (t != null && t.dados.especie == especie) return t;
        return null;
    }
}
