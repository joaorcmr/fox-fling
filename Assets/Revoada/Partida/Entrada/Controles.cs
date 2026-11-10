using UnityEngine;
using UnityEngine.EventSystems;

// O mouse e o teclado no mapa. Só lê a entrada e traduz em comandos: quem
// guarda o que o jogador tem na mão é a Selecao, e quem desenha o porco
// fantasma é o FantasmaColocacao.
//
// Com um porco da loja na mão, clicar coloca; sem nada na mão, clicar num
// porco seleciona. Clique direito ou Esc cancela; Esc sem nada selecionado
// abre o menu de pausa. Cliques em cima da interface (a barra lateral) são
// ignorados aqui.
//
// Atalhos: 1–4 escolhem o porco da loja · Espaço solta a onda · P pausa ·
// F troca a velocidade · U melhora o selecionado · Delete/Backspace vende.
public class Controles : MonoBehaviour
{
    public Jogo jogo;
    public Camera cam;
    public FantasmaColocacao fantasma;
    public MenuPausa menuPausa;
    [Tooltip("Durante a troca de fase (quadro na tela), nada no mapa nem no teclado responde.")]
    public TrocaDeFase trocaDeFase;

    Simulacao Sim => jogo.Sim;
    Selecao Selecao => jogo.Selecao;

    void Start()
    {
        // Acabou: daqui em diante só a tela de fim responde. Sem isto, Esc abria
        // o menu de pausa por cima dela e 1–4 punham um porco fantasma no mapa.
        Sim.FimDeJogo += _ => enabled = false;
    }

    void Update()
    {
        if (menuPausa != null && menuPausa.Aberto)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) menuPausa.Fechar();
            return;
        }
        if (trocaDeFase != null && trocaDeFase.Ativa) return; // quem responde é o quadro da troca de fase

        Vector2 mundo = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));
        bool sobreUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        if (fantasma != null) fantasma.Seguir(mundo, sobreUI);

        if (Input.GetMouseButtonDown(0) && !sobreUI)
        {
            if (Selecao.Colocando.HasValue) Selecao.TentarColocar(mundo);
            else Selecao.Selecionar(Selecao.PorcoEm(mundo));
        }

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            if (Selecao.Colocando.HasValue) Selecao.CancelarColocacao();
            else if (Selecao.Selecionado != null) Selecao.Selecionar(null);
            else if (Input.GetKeyDown(KeyCode.Escape) && menuPausa != null) menuPausa.Abrir();
        }

        Atalhos();
    }

    void Atalhos()
    {
        for (int i = 0; i < jogo.porcos.Length && i < 4; i++)
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
                Selecao.IniciarColocacao(jogo.porcos[i].dados.especie);

        if (Input.GetKeyDown(KeyCode.Space)) Sim.SoltarOnda();
        if (Input.GetKeyDown(KeyCode.P)) jogo.Relogio.AlternarPausa();
        if (Input.GetKeyDown(KeyCode.F)) jogo.Relogio.ProximaVelocidade();
        var selecionado = Selecao.Selecionado;
        if (selecionado != null)
        {
            if (Input.GetKeyDown(KeyCode.U)) Selecao.Avisar(Sim.Melhorar(selecionado));
            if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace)) Sim.Vender(selecionado);
        }
    }
}
