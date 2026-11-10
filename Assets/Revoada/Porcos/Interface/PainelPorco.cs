using UnityEngine;
using UnityEngine.UI;
using static Formato;

// O painel do porco selecionado (toma o lugar da loja na barra lateral): nome,
// nível em bolinhas, os números atuais, a próxima melhoria ou as duas
// especializações, o modo de alvo e o botão de vender.
public class PainelPorco : MonoBehaviour
{
    public Jogo jogo;

    [Header("Cabeçalho")]
    public Image icone;
    public Text nome;
    public Image[] pontos;
    public Sprite pontoCheio;
    public Sprite pontoVazio;
    public Text numeros;
    public Button botaoFechar;

    [Header("Próxima melhoria")]
    public Button botaoMelhoria;
    public Text nomeMelhoria;
    public Text descricaoMelhoria;
    public Text custoMelhoria;

    [Header("Especializações (no nível 3)")]
    public Button[] botoesEspecializacao;
    public Text[] nomesEspecializacao;
    public Text[] descricoesEspecializacao;
    public Text[] custosEspecializacao;
    public Text textoEspecializado;

    [Header("Modo de alvo e venda")]
    public Button botaoModo;
    public Text textoModo;
    public Button botaoVender;
    public Text textoVender;

    public Porco Porco { get; private set; }

    void Awake()
    {
        botaoFechar.onClick.AddListener(() => jogo.Selecao.Selecionar(null));
        botaoMelhoria.onClick.AddListener(() => jogo.Selecao.Avisar(jogo.Sim.Melhorar(Porco)));
        for (int i = 0; i < botoesEspecializacao.Length; i++)
        {
            int escolha = i;
            botoesEspecializacao[i].onClick.AddListener(() => jogo.Selecao.Avisar(jogo.Sim.Especializar(Porco, escolha)));
        }
        botaoModo.onClick.AddListener(() => jogo.Sim.MudarModo(Porco, (ModoAlvo)(((int)Porco.Modo + 1) % 4)));
        botaoVender.onClick.AddListener(() => jogo.Sim.Vender(Porco));
    }

    public void Mostrar(Porco p)
    {
        Porco = p;
        gameObject.SetActive(true);
        Atualizar();
    }

    public void Esconder()
    {
        Porco = null;
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (Porco != null) Atualizar();
    }

    void Atualizar()
    {
        var p = Porco;
        var s = jogo.Sim;
        var tipo = jogo.Tipos.De(p.Especie);
        icone.sprite = tipo.corpo;
        nome.text = p.Dados.nome;
        for (int i = 0; i < pontos.Length; i++) pontos[i].sprite = i < Mathf.Min(p.Nivel, 3) ? pontoCheio : pontoVazio;
        numeros.text = Numeros(p);

        var m = s.ProximaMelhoria(p);
        botaoMelhoria.gameObject.SetActive(m != null);
        if (m != null)
        {
            nomeMelhoria.text = m.nome + "  (U)";
            descricaoMelhoria.text = m.descricao;
            custoMelhoria.text = m.custo.ToString();
            botaoMelhoria.interactable = s.Moedas >= m.custo;
        }

        bool escolher = p.Nivel == 3;
        for (int i = 0; i < botoesEspecializacao.Length; i++)
        {
            botoesEspecializacao[i].gameObject.SetActive(escolher);
            if (!escolher) continue;
            var e = i == 0 ? p.Dados.especializacaoA : p.Dados.especializacaoB;
            nomesEspecializacao[i].text = e.nome;
            descricoesEspecializacao[i].text = e.descricao;
            custosEspecializacao[i].text = e.custo.ToString();
            botoesEspecializacao[i].interactable = s.Moedas >= e.custo;
        }

        textoEspecializado.gameObject.SetActive(p.Nivel >= 4);
        if (p.Nivel >= 4)
        {
            var e = p.Especializacao == 0 ? p.Dados.especializacaoA : p.Dados.especializacaoB;
            textoEspecializado.text = $"Especializado: {e.nome}\n{e.descricao}";
        }

        botaoModo.gameObject.SetActive(p.Ataca);
        textoModo.text = "Mira em: " + NomeModo(p.Modo);
        textoVender.text = $"Vender (+{s.ValorVenda(p)})";
    }

    static string Numeros(Porco p)
    {
        var a = p.Atributos;
        if (!p.Ataca)
        {
            string texto = $"Aura {Num(a.raioAura, "0.#")} · {Porcento(a.bonusCadencia)} mais rápido";
            if (a.bonusAlcance > 0f) texto += $"\n+{Porcento(a.bonusAlcance)} de alcance";
            if (a.bonusMoedas > 0f) texto += $"\n+{Porcento(a.bonusMoedas)} de moedas";
            return texto;
        }
        string extra = a.tiro == TipoTiro.Bomba ? $"Área {Num(a.raioArea, "0.#")}" : $"Atravessa {a.perfura}";
        return $"Dano {Num(a.dano, "0.#")} · Alcance {Num(p.Alcance, "0.#")}\nRecarga {Num(a.recarga)} s · {extra}";
    }

    static string NomeModo(ModoAlvo m)
    {
        switch (m)
        {
            case ModoAlvo.Ultimo: return "o último";
            case ModoAlvo.MaisForte: return "o mais forte";
            case ModoAlvo.MaisPerto: return "o mais perto";
            default: return "o primeiro";
        }
    }
}
