using NUnit.Framework;
using UnityEngine;
using static CenarioTeste;

// Os comandos do jogador: comprar, vender, mudar o modo de alvo. Funcionam até
// com o jogo pausado (sem nenhum Passo) e param depois do fim de jogo.
public class SimulacaoTests
{
    static DadosJogo ComEstilingueiro(int penas = 300)
    {
        var d = Basico();
        d.economia.penasIniciais = penas;
        d.porcos[EspeciePorco.Estilingueiro] = Porco(EspeciePorco.Estilingueiro, 120, Tiro(TipoTiro.Pedrinha, recarga: 0.8f));
        return d;
    }

    [Test]
    public void Comprar_SemPenas()
    {
        var s = new Simulacao(ComEstilingueiro(penas: 100));

        Assert.AreEqual(Resultado.SemPenas, s.Comprar(EspeciePorco.Estilingueiro, new Vector2(7f, 10f), out var porco));
        Assert.IsNull(porco);
        Assert.AreEqual(0, s.Porcos.Count);
        Assert.AreEqual(100, s.Penas);
    }

    [Test]
    public void Comprar_LugarInvalido()
    {
        var s = new Simulacao(ComEstilingueiro());

        Assert.AreEqual(Resultado.LugarInvalido, s.Comprar(EspeciePorco.Estilingueiro, new Vector2(7f, 7.5f), out _));
        Assert.AreEqual(300, s.Penas);
    }

    [Test]
    public void Comprar_Ok_DebitaECria()
    {
        var s = new Simulacao(ComEstilingueiro());
        Porco criado = null;
        s.PorcoCriado += p => criado = p;

        Assert.AreEqual(Resultado.Ok, s.Comprar(EspeciePorco.Estilingueiro, new Vector2(7f, 10f), out var porco));

        Assert.AreEqual(180, s.Penas);
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
        Assert.AreEqual(180 + 84, s.Penas);
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
        Assert.AreEqual(Resultado.Indisponivel, s.ComprarNinho());
    }
}
