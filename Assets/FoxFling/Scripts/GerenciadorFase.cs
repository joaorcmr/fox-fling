using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Juiz da fase (tower defense): a CPU manda levas de raposas que andam até a torre e
// o jogador tem que manter pelo menos um tubarão vivo até a última leva acabar.
//
// O ciclo de cada leva: PREPARAR (loja aberta, o jogador compra e depois clica em "Iniciar
// leva") → LEVA (as raposas de Regras.ComporLeva entram) → ganha pontos (Regras.PontosDaLeva).
// Perder o último tubarão é derrota; sobreviver à última leva é vitória (Regras.EstadoAposOnda).
public class GerenciadorFase : MonoBehaviour
{
    public static GerenciadorFase Instance { get; private set; }

    public int numeroFase = 1;
    public string nomeFase = "Fase 1";
    public string dificuldade = "Fácil";

    [Tooltip("Quantas levas de raposas a fase tem.")]
    public int levas = Regras.LevasPorFase;

    public RaposaAtacante atacanteNormal, atacanteNinja, atacanteBomba, atacanteGrande;
    public Hud hud;

    [Tooltip("O que a loja vende (Assets/FoxFling/Loja/Catalogo).")]
    public CatalogoLoja catalogo;

    [Tooltip("Pontos para gastar na loja quando a fase começa.")]
    public int saldoInicial = 500;

    [Tooltip("De onde as raposas entram: fora da tela, à esquerda, em cima do chão.")]
    public float xDeEntrada = -8.3f;

    public float alturaDoChao = -4.5f;

    // Depois que a leva acaba, espera o mundo parar por no máximo isto antes de decidir.
    const float EsperaMaximaFimDaLeva = 4f;

    public int Saldo { get; private set; }

    // Soma de tudo que o jogador ganhou nas levas (é o que aparece na tela de resultado).
    public int Pontos { get; private set; }

    public EstadoFase Estado { get; private set; } = EstadoFase.Jogando;
    public int InimigosVivos => inimigos.Count;
    public int RaposasRestantes => porNascer + atacantes.Count;
    public int LevaAtual { get; private set; }
    public Loja Loja { get; private set; }

    readonly List<Inimigo> inimigos = new List<Inimigo>();
    readonly List<RaposaAtacante> atacantes = new List<RaposaAtacante>();
    int porNascer;
    int tubaroesNoInicio;
    int tubaroesNoInicioDaLeva;
    int pontosDaLeva;
    float instanteFimDaLeva = -1f;
    bool iniciarPedido;
    bool resultadoMostrado;
    string mensagemDaLeva = "";

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

    void Update()
    {
        // Atalho para o botão "Iniciar leva". (Enter não: ele também "aperta" botões do UI.)
        if (Estado == EstadoFase.Preparando && Input.GetKeyDown(KeyCode.Space))
            IniciarLeva();
    }

    IEnumerator Start()
    {
        if (hud != null) hud.Configurar(nomeFase, dificuldade);

        if (atacanteNormal == null)
        {
            Debug.LogError("GerenciadorFase sem prefab de raposa atacante (atacanteNormal). " +
                           "Rode o menu Fox Fling > Reconstruir tudo.");
            yield break;
        }

        Saldo = saldoInicial;
        AtualizarSaldo();

        // Os tubarões se registram no Start deles; um quadro depois a lista está completa.
        yield return null;
        tubaroesNoInicio = inimigos.Count;

        Loja = gameObject.AddComponent<Loja>();
        Loja.Configurar(this, catalogo);

        for (LevaAtual = 1; LevaAtual <= levas; LevaAtual++)
        {
            yield return Preparar();
            yield return RodarLeva();
            if (Estado != EstadoFase.Jogando) yield break; // perdeu

            ConcluirLeva();
        }

        StartCoroutine(Vitoria());
    }

    // Loja aberta: espera o jogador clicar em "Iniciar leva".
    IEnumerator Preparar()
    {
        Estado = EstadoFase.Preparando;
        iniciarPedido = false;
        Loja.Abrir(mensagemDaLeva + $"Leva {LevaAtual} de {levas}: compre e clique em Iniciar leva (Espaço)");
        mensagemDaLeva = "";

        while (!iniciarPedido) yield return null;

        Loja.Fechar();
        Estado = EstadoFase.Jogando;
    }

    public void IniciarLeva()
    {
        if (Estado == EstadoFase.Preparando) iniciarPedido = true;
    }

    // Gera as raposas da leva, uma a uma, e espera até a leva acabar (ou o jogador perder).
    IEnumerator RodarLeva()
    {
        var leva = Regras.ComporLeva(numeroFase, LevaAtual);
        float intervalo = Regras.IntervaloEntreRaposas(Regras.NivelDaLeva(numeroFase, LevaAtual));

        porNascer = leva.Length;
        instanteFimDaLeva = -1f;
        pontosDaLeva = 0;
        tubaroesNoInicioDaLeva = inimigos.Count;
        AtualizarSaldo();
        AtualizarDica();

        for (int i = 0; i < leva.Length; i++)
        {
            if (Estado != EstadoFase.Jogando) yield break;

            Gerar(leva[i]);
            porNascer--;
            AtualizarDica();

            if (i < leva.Length - 1)
                yield return new WaitForSeconds(intervalo);
        }

        while (Estado == EstadoFase.Jogando)
        {
            yield return new WaitForSeconds(0.25f);

            bool levaAcabou = porNascer == 0 && atacantes.Count == 0;
            if (levaAcabou && instanteFimDaLeva < 0f) instanteFimDaLeva = Time.time;
            bool parado = levaAcabou && (MundoParado() || Time.time - instanteFimDaLeva > EsperaMaximaFimDaLeva);

            var novo = Regras.EstadoAposOnda(inimigos.Count, porNascer, atacantes.Count, parado);
            if (novo == EstadoFase.Derrota) StartCoroutine(Derrota());
            else if (novo == EstadoFase.Vitoria) yield break; // leva sobrevivida
        }
    }

    // Sobreviveu à leva: os pontos viram saldo, menos a penalidade pelos tubarões perdidos.
    void ConcluirLeva()
    {
        int perdidos = tubaroesNoInicioDaLeva - inimigos.Count;
        int ganho = Regras.PontosDaLeva(pontosDaLeva, perdidos, Regras.NivelDaLeva(numeroFase, LevaAtual));
        Saldo += ganho;
        Pontos += ganho;
        pontosDaLeva = 0;
        AtualizarSaldo();

        mensagemDaLeva = $"Leva {LevaAtual} concluída: +{Regras.FormatarPontos(ganho)} pontos"
                       + (perdidos > 0 ? $"  ·  tubarões perdidos: {perdidos}" : "") + "\n";
    }

    RaposaAtacante Prefab(TipoRaposa tipo)
    {
        RaposaAtacante p;
        switch (tipo)
        {
            case TipoRaposa.Ninja: p = atacanteNinja; break;
            case TipoRaposa.Bomba: p = atacanteBomba; break;
            case TipoRaposa.Grande: p = atacanteGrande; break;
            default: p = atacanteNormal; break;
        }
        return p != null ? p : atacanteNormal;
    }

    void Gerar(TipoRaposa tipo)
    {
        // Nasce em cima do chão, qualquer que seja o tamanho da raposa (raio × escala).
        var prefab = Prefab(tipo);
        float raio = prefab.GetComponent<CircleCollider2D>().radius * Mathf.Abs(prefab.transform.localScale.y);
        var raposa = Instantiate(prefab, new Vector3(xDeEntrada, alturaDoChao + raio + 0.01f, 0f), Quaternion.identity);
        var colisor = raposa.GetComponent<Collider2D>();

        // As raposas atravessam umas às outras: a leva chega junta na torre em vez de
        // ficar em fila atrás da primeira.
        foreach (var outra in atacantes)
            Physics2D.IgnoreCollision(colisor, outra.GetComponent<Collider2D>());

        raposa.Morreu += AoRaposaMorrer;
        atacantes.Add(raposa);
    }

    void AoRaposaMorrer(RaposaAtacante raposa)
    {
        atacantes.Remove(raposa);
        AtualizarDica();
    }

    void AtualizarDica()
    {
        if (hud == null || Estado != EstadoFase.Jogando) return;
        hud.MostrarDica($"Leva {LevaAtual} de {levas}   ·   Raposas: {RaposasRestantes}   ·   Tubarões: {inimigos.Count}");
    }

    public void Mensagem(string texto)
    {
        if (hud != null) hud.MostrarDica(texto);
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
        AtualizarDica();

        // Perdeu o último tubarão: a torre caiu, não importa quantas raposas faltem.
        if (Estado == EstadoFase.Jogando && inimigos.Count == 0 && tubaroesNoInicio > 0)
            StartCoroutine(Derrota());
    }

    // ---- pontos e loja ----

    // Pontos de raposas derrotadas: ficam "pendentes" até o fim da leva (ver ConcluirLeva).
    public void AdicionarPontos(int pontos, Vector3 onde)
    {
        if (resultadoMostrado || pontos <= 0) return;
        pontosDaLeva += pontos;
        AtualizarSaldo();
        Efeitos.Pontos(onde, pontos);
    }

    void AtualizarSaldo()
    {
        if (hud != null) hud.AtualizarPontos(Saldo, pontosDaLeva);
        if (Loja != null) Loja.Atualizar();
    }

    public bool TentarGastar(int valor)
    {
        if (valor > Saldo) return false;
        Saldo -= valor;
        AtualizarSaldo();
        return true;
    }

    // A torre = tudo que quebra e não é raposa (blocos e tubarões).
    IEnumerable<Destrutivel> PecasDaTorre()
    {
        foreach (var d in FindObjectsByType<Destrutivel>())
            if (!(d is RaposaAtacante) && !d.Morto) yield return d;
    }

    public bool TorreDanificada()
    {
        foreach (var d in PecasDaTorre())
            if (d.Danificado) return true;
        return false;
    }

    // Devolve quantas peças foram reparadas.
    public int RepararTorre()
    {
        int n = 0;
        foreach (var d in PecasDaTorre())
        {
            if (!d.Danificado) continue;
            d.RestaurarVida();
            n++;
        }
        return n;
    }

    // ---- fim da fase ----

    IEnumerator Vitoria()
    {
        Estado = EstadoFase.Vitoria;
        if (hud != null) hud.MostrarDica("");
        yield return new WaitForSeconds(0.6f);

        int estrelas = Regras.EstrelasPorSobreviventes(inimigos.Count, tubaroesNoInicio, true);
        Progresso.RegistrarVitoria(numeroFase, estrelas);
        resultadoMostrado = true;
        Sons.Tocar(Sons.I != null ? Sons.I.vitoria : null);
        if (hud != null) hud.MostrarResultado(true, Pontos, estrelas, numeroFase < Progresso.TotalFases);
    }

    IEnumerator Derrota()
    {
        Estado = EstadoFase.Derrota;
        resultadoMostrado = true;
        if (hud != null) hud.MostrarDica("");
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
