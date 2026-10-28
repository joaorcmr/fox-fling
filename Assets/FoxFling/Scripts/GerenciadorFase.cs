using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Juiz da fase: cria a fila de raposas, entrega uma de cada vez ao estilingue,
// conta pontos e inimigos e decide vitória ou derrota (Regras.EstadoAposRaposa).
public class GerenciadorFase : MonoBehaviour
{
    public static GerenciadorFase Instance { get; private set; }

    public int numeroFase = 1;
    public string nomeFase = "Fase 1";
    public string dificuldade = "Fácil";

    [Tooltip("Raposas disponíveis, na ordem em que entram no estilingue.")]
    public TipoRaposa[] raposas = { TipoRaposa.Normal, TipoRaposa.Normal };

    public Raposa prefabNormal, prefabNinja, prefabBomba;
    public Estilingue estilingue;
    public Hud hud;

    [Tooltip("Lugar da primeira raposa da fila; as outras ficam à esquerda.")]
    public Vector2 inicioFila = new Vector2(-6.45f, -4.17f);
    public float espacoFila = 0.62f;

    public int Pontos { get; private set; }
    public EstadoFase Estado { get; private set; } = EstadoFase.Jogando;
    public int InimigosVivos => inimigos.Count;
    public int RaposasNaFila => fila.Count;

    readonly List<Inimigo> inimigos = new List<Inimigo>();
    readonly List<Raposa> fila = new List<Raposa>();
    int pontuacaoMaxima;
    bool resultadoMostrado;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetarStatics() => Instance = null;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    IEnumerator Start()
    {
        if (hud != null) hud.Configurar(nomeFase, dificuldade);

        for (int i = 0; i < raposas.Length; i++)
        {
            Vector2 lugar = LugarNaFila(i);
            var r = Instantiate(Prefab(raposas[i]), lugar, Quaternion.identity);
            r.IrParaFila(lugar);
            r.Terminou += AoRaposaTerminar;
            fila.Add(r);
        }

        // Pontuação máxima = tudo destruído + bônus de todas as raposas menos a primeira.
        int soma = 0;
        foreach (var d in FindObjectsByType<Destrutivel>())
            soma += d.pontos;
        pontuacaoMaxima = Regras.PontuacaoMaxima(soma, raposas.Length);
        if (hud != null) hud.AtualizarPontos(0);

        yield return new WaitForSeconds(0.6f);
        CarregarProxima();
    }

    Vector2 LugarNaFila(int i) => inicioFila + Vector2.left * espacoFila * i;

    Raposa Prefab(TipoRaposa tipo)
    {
        switch (tipo)
        {
            case TipoRaposa.Ninja: return prefabNinja != null ? prefabNinja : prefabNormal;
            case TipoRaposa.Bomba: return prefabBomba != null ? prefabBomba : prefabNormal;
            default: return prefabNormal;
        }
    }

    void CarregarProxima()
    {
        if (Estado != EstadoFase.Jogando || fila.Count == 0) return;

        var r = fila[0];
        fila.RemoveAt(0);
        for (int i = 0; i < fila.Count; i++)
            fila[i].IrParaFila(LugarNaFila(i));

        r.IrParaEstilingue(estilingue.Ancora, 0.45f);
        estilingue.Carregar(r);
        if (hud != null) hud.MostrarDica(r.dica);
    }

    void AoRaposaTerminar(Raposa r)
    {
        if (Estado != EstadoFase.Jogando) return;
        StartCoroutine(DepoisDaRaposa());
    }

    IEnumerator DepoisDaRaposa()
    {
        if (fila.Count > 0)
        {
            yield return new WaitForSeconds(0.5f);
            CarregarProxima();
            yield break;
        }

        // Era a última raposa: espera o mundo parar (blocos caindo ainda podem matar alguém).
        yield return new WaitForSeconds(0.5f);
        float limite = Time.time + 4f;
        while (Estado == EstadoFase.Jogando && Time.time < limite && !MundoParado())
            yield return null;

        if (Regras.EstadoAposRaposa(inimigos.Count, 0, true) == EstadoFase.Derrota && Estado == EstadoFase.Jogando)
            StartCoroutine(Derrota());
    }

    bool MundoParado()
    {
        foreach (var corpo in FindObjectsByType<Rigidbody2D>())
        {
            if (corpo.bodyType != RigidbodyType2D.Dynamic || corpo.IsSleeping()) continue;
            if (corpo.linearVelocity.sqrMagnitude > 0.01f) return false;
        }
        return true;
    }

    public void RegistrarInimigo(Inimigo inimigo)
    {
        if (!inimigos.Contains(inimigo)) inimigos.Add(inimigo);
    }

    public void InimigoMorreu(Inimigo inimigo)
    {
        inimigos.Remove(inimigo);
        if (Estado == EstadoFase.Jogando && Regras.EstadoAposRaposa(inimigos.Count, fila.Count, false) == EstadoFase.Vitoria)
            StartCoroutine(Vitoria());
    }

    public void AdicionarPontos(int pontos, Vector3 onde)
    {
        if (resultadoMostrado) return;
        Pontos += pontos;
        if (hud != null) hud.AtualizarPontos(Pontos);
        Efeitos.Pontos(onde, pontos);
    }

    IEnumerator Vitoria()
    {
        Estado = EstadoFase.Vitoria;
        if (hud != null) hud.MostrarDica("");

        // Deixa os blocos terminarem de cair (e pontuarem) antes do bônus.
        yield return new WaitForSeconds(1.5f);

        var sobraram = new List<Raposa>();
        var noEstilingue = estilingue.Descarregar();
        if (noEstilingue != null) sobraram.Add(noEstilingue);
        sobraram.AddRange(fila);
        fila.Clear();

        foreach (var r in sobraram)
        {
            if (r == null) continue;
            AdicionarPontos(Regras.BonusRaposa, r.transform.position + Vector3.up * 0.5f);
            Sons.Tocar(Sons.I != null ? Sons.I.estrela : null, 0.6f);
            Efeitos.Fumaca(r.transform.position, 0.7f);
            Destroy(r.gameObject);
            yield return new WaitForSeconds(0.4f);
        }

        int estrelas = Regras.Estrelas(Pontos, pontuacaoMaxima, true);
        Progresso.RegistrarVitoria(numeroFase, estrelas);
        resultadoMostrado = true;
        Sons.Tocar(Sons.I != null ? Sons.I.vitoria : null);
        if (hud != null) hud.MostrarResultado(true, Pontos, estrelas, numeroFase < Progresso.TotalFases);
    }

    IEnumerator Derrota()
    {
        Estado = EstadoFase.Derrota;
        resultadoMostrado = true;
        yield return new WaitForSeconds(0.3f);
        Sons.Tocar(Sons.I != null ? Sons.I.derrota : null);
        if (hud != null) hud.MostrarResultado(false, Pontos, 0, false);
    }

    public void Reiniciar() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    public void IrParaMenu() => SceneManager.LoadScene("Menu");

    public void ProximaFase()
    {
        if (numeroFase < Progresso.TotalFases) SceneManager.LoadScene("Fase" + (numeroFase + 1));
        else IrParaMenu();
    }
}
