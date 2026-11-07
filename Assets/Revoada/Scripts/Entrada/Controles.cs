using System;
using UnityEngine;
using UnityEngine.EventSystems;

// O mouse no mapa: escolher um porco na loja põe um "fantasma" na ponta do
// mouse (verde = pode colocar aqui, vermelho = não pode); clicar coloca.
// Sem fantasma, clicar num porco seleciona. Clique direito ou Esc cancela.
// Cliques em cima da interface (a barra lateral) são ignorados aqui.
public class Controles : MonoBehaviour
{
    static readonly Color Pode = new Color(0.75f, 1f, 0.75f, 0.85f);
    static readonly Color NaoPode = new Color(1f, 0.5f, 0.5f, 0.85f);

    public Jogo jogo;
    public Camera cam;
    public SpriteRenderer fantasma;
    public CirculoAlcance circuloFantasma;

    public EspeciePorco? Colocando { get; private set; }
    public Porco Selecionado { get; private set; }

    public event Action<Porco> SelecaoMudou;
    public event Action<Resultado> Falhou;

    Simulacao Sim => jogo.Sim;

    void Start()
    {
        fantasma.enabled = false;
        circuloFantasma.Esconder();
        Sim.PorcoVendido += p =>
        {
            if (p == Selecionado) Selecionar(null);
        };
    }

    void Update()
    {
        Vector2 mundo = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));
        bool sobreUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        if (Colocando.HasValue) AtualizarFantasma(mundo, sobreUI);

        if (Input.GetMouseButtonDown(0) && !sobreUI)
        {
            if (Colocando.HasValue) TentarColocar(mundo);
            else Selecionar(PorcoEm(mundo));
        }

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            if (Colocando.HasValue) CancelarColocacao();
            else if (Selecionado != null) Selecionar(null);
        }
    }

    void AtualizarFantasma(Vector2 mundo, bool sobreUI)
    {
        var tipo = jogo.TipoDe(Colocando.Value);
        bool pode = Sim.PodeColocar(Colocando.Value, mundo);
        fantasma.enabled = !sobreUI;
        fantasma.transform.position = mundo;
        fantasma.color = pode ? Pode : NaoPode;
        if (sobreUI) circuloFantasma.Esconder();
        else circuloFantasma.Mostrar(mundo, tipo.dados.nivel1.alcance, pode ? new Color(1f, 1f, 1f, 0.8f) : new Color(1f, 0.45f, 0.45f, 0.8f));
    }

    public void IniciarColocacao(EspeciePorco especie)
    {
        Selecionar(null);
        Colocando = especie;
        fantasma.sprite = jogo.TipoDe(especie).corpo;
    }

    public void CancelarColocacao()
    {
        Colocando = null;
        fantasma.enabled = false;
        circuloFantasma.Esconder();
    }

    // Público para dar para testar pelo eval (o Input antigo não é simulável).
    public Resultado TentarColocar(Vector2 mundo)
    {
        if (!Colocando.HasValue) return Resultado.Indisponivel;
        var r = Sim.Comprar(Colocando.Value, mundo, out _);
        if (r == Resultado.Ok) CancelarColocacao();
        else Falhou?.Invoke(r);
        return r;
    }

    public void Selecionar(Porco p)
    {
        if (p == Selecionado) return;
        Selecionado = p;
        SelecaoMudou?.Invoke(p);
    }

    Porco PorcoEm(Vector2 mundo)
    {
        Porco melhor = null;
        float melhorDist = 0.5f;
        foreach (var p in Sim.Porcos)
        {
            float d = Vector2.Distance(p.Posicao, mundo);
            if (d <= melhorDist)
            {
                melhor = p;
                melhorDist = d;
            }
        }
        return melhor;
    }
}
