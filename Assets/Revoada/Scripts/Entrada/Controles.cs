using System;
using UnityEngine;
using UnityEngine.EventSystems;

// O mouse e o teclado no mapa. Escolher um porco na loja põe um "fantasma" na
// ponta do mouse (verde = pode colocar aqui, vermelho = não pode); clicar
// coloca. Sem fantasma, clicar num porco seleciona. Clique direito ou Esc
// cancela; Esc sem nada selecionado abre o menu de pausa. Cliques em cima da
// interface (a barra lateral) são ignorados aqui.
//
// Atalhos: 1–4 escolhem o porco da loja · Espaço solta a onda · P pausa ·
// F troca a velocidade · U melhora o selecionado · Delete/Backspace vende.
public class Controles : MonoBehaviour
{
    static readonly Color Pode = new Color(0.75f, 1f, 0.75f, 0.85f);
    static readonly Color NaoPode = new Color(1f, 0.5f, 0.5f, 0.85f);

    public Jogo jogo;
    public Camera cam;
    public SpriteRenderer fantasma;
    public CirculoAlcance circuloFantasma;
    public MenuPausa menuPausa;

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
        if (menuPausa != null && menuPausa.Aberto)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) menuPausa.Fechar();
            return;
        }

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
            else if (Input.GetKeyDown(KeyCode.Escape) && menuPausa != null) menuPausa.Abrir();
        }

        Atalhos();
    }

    void Atalhos()
    {
        for (int i = 0; i < jogo.porcos.Length && i < 4; i++)
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
                IniciarColocacao(jogo.porcos[i].dados.especie);

        if (Input.GetKeyDown(KeyCode.Space)) Sim.SoltarOnda();
        if (Input.GetKeyDown(KeyCode.P)) jogo.AlternarPausa();
        if (Input.GetKeyDown(KeyCode.F)) jogo.ProximaVelocidade();
        if (Selecionado != null)
        {
            if (Input.GetKeyDown(KeyCode.U)) Avisar(Sim.Melhorar(Selecionado));
            if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace)) Sim.Vender(Selecionado);
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
        else Avisar(r);
        return r;
    }

    // Para a interface avisar quando um comando não deu certo (sem penas etc.).
    public void Avisar(Resultado r)
    {
        if (r != Resultado.Ok) Falhou?.Invoke(r);
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
