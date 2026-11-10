using System;
using UnityEngine;

// O que o jogador tem "na mão": um porco da loja esperando lugar no mapa
// (Colocando) ou um porco do mapa selecionado. Recebe os comandos de quem
// quer que seja (teclado e mouse nos Controles, botões da interface) e avisa
// por eventos. Não lê teclado nem desenha nada, por isso dá para testar sem
// dar Play.
//
// Só dá para começar a colocar um porco ENTRE ondas (Preparando ou Intervalo):
// com a onda em andamento, o jogo espera o jogador comprar e só segue para a
// próxima quando ele soltar (issue #14). Se a onda começa com um porco ainda
// na mão (o Auto não espera), a colocação é cancelada sozinha.
public class Selecao
{
    readonly Simulacao sim;

    public EspeciePorco? Colocando { get; private set; }
    public Porco Selecionado { get; private set; }

    public event Action<Porco> SelecaoMudou;
    public event Action<EspeciePorco?> ColocacaoMudou;   // null = não tem mais porco na mão
    public event Action<Resultado> Falhou;

    public Selecao(Simulacao sim)
    {
        this.sim = sim;
        sim.PorcoVendido += p =>
        {
            if (p == Selecionado) Selecionar(null);
        };
        sim.OndaComecou += _ => CancelarColocacao();
        // Troca de fase: todos os porcos saem do mapa, então nada fica na mão.
        sim.PorcosRecolhidos += (porcos, moedas) =>
        {
            CancelarColocacao();
            Selecionar(null);
        };
        // Acabou: nada fica na mão do jogador (daqui em diante só a tela de fim responde).
        sim.FimDeJogo += _ =>
        {
            CancelarColocacao();
            Selecionar(null);
        };
    }

    public void IniciarColocacao(EspeciePorco especie)
    {
        if (sim.Estado == EstadoJogo.EmOnda)
        {
            Falhou?.Invoke(Resultado.OndaEmAndamento);
            return;
        }
        Selecionar(null);
        Colocando = especie;
        ColocacaoMudou?.Invoke(especie);
    }

    public void CancelarColocacao()
    {
        if (!Colocando.HasValue) return;
        Colocando = null;
        ColocacaoMudou?.Invoke(null);
    }

    // Coloca o porco da mão neste ponto do mapa (se der) e devolve o resultado.
    public Resultado TentarColocar(Vector2 mundo)
    {
        if (!Colocando.HasValue) return Resultado.Indisponivel;
        var r = sim.Comprar(Colocando.Value, mundo, out _);
        if (r == Resultado.Ok) CancelarColocacao();
        else Avisar(r);
        return r;
    }

    // Para a interface avisar quando um comando não deu certo (sem moedas etc.).
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

    // O porco mais perto do ponto clicado, até meia unidade de distância.
    public Porco PorcoEm(Vector2 mundo)
    {
        Porco melhor = null;
        float melhorDist = 0.5f;
        foreach (var p in sim.Porcos)
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
