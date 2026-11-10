using NUnit.Framework;
using UnityEngine;
using static CenarioTeste;

// Os comandos do jogador: comprar, vender, mudar o modo de alvo. Funcionam até
// com o jogo pausado (sem nenhum Passo) e param depois do fim de jogo.
public class SimulacaoTests
{
    static DadosJogo ComEstilingueiro(int moedas = 300)
    {
        var d = Basico();
        d.economia.moedasIniciais = moedas;
        d.porcos[EspeciePorco.Estilingueiro] = Porco(EspeciePorco.Estilingueiro, 120, Tiro(TipoTiro.Pedrinha, recarga: 0.8f));
        return d;
    }

    [Test]
    public void Comprar_SemMoedas()
    {
        var s = new Simulacao(ComEstilingueiro(moedas: 100));

        Assert.AreEqual(Resultado.SemMoedas, s.Comprar(EspeciePorco.Estilingueiro, new Vector2(7f, 10f), out var porco));
        Assert.IsNull(porco);
        Assert.AreEqual(0, s.Porcos.Count);
        Assert.AreEqual(100, s.Moedas);
    }

    [Test]
    public void Comprar_LugarInvalido()
    {
        var s = new Simulacao(ComEstilingueiro());

        Assert.AreEqual(Resultado.LugarInvalido, s.Comprar(EspeciePorco.Estilingueiro, new Vector2(7f, 7.5f), out _));
        Assert.AreEqual(300, s.Moedas);
    }

    [Test]
    public void Comprar_Ok_DebitaECria()
    {
        var s = new Simulacao(ComEstilingueiro());
        Porco criado = null;
        s.PorcoCriado += p => criado = p;

        Assert.AreEqual(Resultado.Ok, s.Comprar(EspeciePorco.Estilingueiro, new Vector2(7f, 10f), out var porco));

        Assert.AreEqual(180, s.Moedas);
        Assert.AreSame(porco, criado);
        Assert.AreEqual(120, porco.Gasto);
        Assert.AreEqual(new Vector2(7f, 10f), porco.Posicao);
        Assert.AreEqual(1, s.Porcos.Count);
    }

    [Test]
    public void Vender_Devolve70PorCento()
    {
        var s = new Simulacao(ComEstilingueiro());
        s.Comprar(EspeciePorco.Estilingueiro, new Vector2(7f, 10f), out var porco);
        Porco vendido = null;
        s.PorcoVendido += p => vendido = p;

        Assert.AreEqual(84, s.ValorVenda(porco));
        Assert.AreEqual(84, s.Vender(porco));
        Assert.AreEqual(180 + 84, s.Moedas);
        Assert.AreSame(porco, vendido);
        Assert.AreEqual(0, s.Porcos.Count);
        Assert.AreEqual(0, s.Vender(porco)); // já foi vendido
    }

    [Test]
    public void Vender_ComTiroNoAr_NaoQuebra()
    {
        var d = ComEstilingueiro();
        d.porcos[EspeciePorco.Estilingueiro].nivel1 = Tiro(TipoTiro.Pedrinha, alcance: 10f, velocidade: 4f);
        d.passaros[EspeciePassaro.Pardal] = Passaro(EspeciePassaro.Pardal, vida: 5f);
        var s = ComPassarosParados(d, EspeciePassaro.Pardal, 3f);
        s.Comprar(EspeciePorco.Estilingueiro, new Vector2(9f, 10f), out var porco);

        s.Passo();          // atira (o tiro é lento: fica um tempo no ar)
        Assert.AreEqual(1, s.Projeteis.Count);
        s.Vender(porco);
        Assert.DoesNotThrow(() => Avancar(s, 3f));

        Assert.AreEqual(4f, s.Passaros[0].Vida, 1e-4f); // o tiro que já tinha saído acertou
    }

    [Test]
    public void Comandos_FuncionamSemPassarTempo()
    {
        var s = new Simulacao(ComEstilingueiro());

        Assert.AreEqual(Resultado.Ok, s.Comprar(EspeciePorco.Estilingueiro, new Vector2(7f, 10f), out var porco));
        s.MudarModo(porco, ModoAlvo.MaisForte);
        Assert.AreEqual(ModoAlvo.MaisForte, porco.Modo);
        Assert.AreEqual(84, s.Vender(porco));
    }

    [Test]
    public void FimDeJogo_BloqueiaComandos()
    {
        var d = ComEstilingueiro();
        d.porcos[EspeciePorco.Estilingueiro].nivel1.alcance = 0.5f;
        d.economia.vidaInicial = 1;
        d.passaros[EspeciePassaro.Pardal] = Passaro(EspeciePassaro.Pardal, velocidade: 7f, dano: 5);
        d.ondas.Add(Onda(Grupo(EspeciePassaro.Pardal, 1)));
        var s = new Simulacao(d);
        s.Comprar(EspeciePorco.Estilingueiro, new Vector2(1f, 12f), out var porco); // longe: não alcança o pardal
        s.SoltarOnda();
        Avancar(s, 2.1f);
        Assert.AreEqual(EstadoJogo.Derrota, s.Estado);

        Assert.AreEqual(Resultado.Indisponivel, s.Comprar(EspeciePorco.Estilingueiro, new Vector2(7f, 10f), out _));
        Assert.AreEqual(0, s.Vender(porco));
        Assert.AreEqual(Resultado.Indisponivel, s.ComprarCofrinho());
        Assert.AreEqual(Resultado.Indisponivel, s.ComprarReparo());
    }

    // Tira "dano" de vida deixando 1 pardal (bem rápido, sem porco no caminho)
    // chegar na Torre; a onda termina (Intervalo) logo em seguida, então o
    // teste já pode comprar reparo fora de uma onda em andamento.
    static Simulacao ComDanoNaTorre(int vidaInicial, int dano, int moedasIniciais = 1000)
    {
        var d = ComEstilingueiro(moedas: moedasIniciais);
        d.economia.vidaInicial = vidaInicial;
        d.passaros[EspeciePassaro.Pardal] = Passaro(EspeciePassaro.Pardal, velocidade: 100f, dano: dano);
        d.ondas.Add(Onda(Grupo(EspeciePassaro.Pardal, 1)));
        d.ondas.Add(Onda(Grupo(EspeciePassaro.Pardal, 1)));
        var s = new Simulacao(d);
        s.SoltarOnda();
        Avancar(s, 1f); // o pardal é bem rápido: já chega e a onda termina (Intervalo)
        Assert.AreEqual(vidaInicial - dano, s.Vida);
        Assert.AreNotEqual(EstadoJogo.EmOnda, s.Estado);
        return s;
    }

    // Comprar saúde para a Torre, com custo que sobe a cada compra (issue #14).
    [Test]
    public void ComprarReparo_CuraSemPassarDaVidaInicial()
    {
        var s = ComDanoNaTorre(vidaInicial: 100, dano: 10); // Vida = 90
        s.Dados.economia.curaPorReparo = 20;
        s.Dados.economia.custoReparoBase = 50;
        int? curado = null;
        s.TorreReparada += c => curado = c;

        Assert.AreEqual(Resultado.Ok, s.ComprarReparo());
        Assert.AreEqual(100, s.Vida, "90 + 20 passaria de 100: a cura é limitada à vida inicial");
        Assert.AreEqual(10, curado, "só curou os 10 que faltavam");
        Assert.AreEqual(Resultado.Indisponivel, s.ComprarReparo(), "vida cheia: não tem o que reparar");
    }

    [Test]
    public void ComprarReparo_SemMoedas()
    {
        var s = ComDanoNaTorre(vidaInicial: 100, dano: 50, moedasIniciais: 0); // Vida = 50, sem moedas

        Assert.AreEqual(Resultado.SemMoedas, s.ComprarReparo());
        Assert.AreEqual(50, s.Vida);
    }

    [Test]
    public void ComprarReparo_ComAOndaEmAndamento_NaoDeixaComprar()
    {
        var d = ComEstilingueiro();
        d.ondas.Add(Onda(Grupo(EspeciePassaro.Pardal, 1)));
        var s = new Simulacao(d);
        s.SoltarOnda();

        Assert.AreEqual(Resultado.OndaEmAndamento, s.ComprarReparo());
    }
}
