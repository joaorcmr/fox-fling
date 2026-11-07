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
