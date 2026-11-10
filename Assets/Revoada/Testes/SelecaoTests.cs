using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using static CenarioTeste;

// O que o jogador tem na mão (Selecao): colocar o porco da loja, selecionar um
// porco do mapa e o que acontece com isso quando o porco é vendido ou a partida
// acaba. Antes isso ficava dentro dos Controles, misturado com o teclado.
public class SelecaoTests
{
    static readonly Vector2 LugarBom = new Vector2(7f, 10f);
    static readonly Vector2 NaTrilha = new Vector2(7f, 7.5f);

    static Simulacao Partida(int moedas = 1000)
    {
        var d = Basico();
        d.economia.moedasIniciais = moedas;
        d.economia.vidaInicial = 1;
        d.porcos[EspeciePorco.Estilingueiro] = Porco(EspeciePorco.Estilingueiro, 120, Tiro(TipoTiro.Pedrinha, alcance: 0.1f));
        d.ondas.Add(Onda(Grupo(EspeciePassaro.Pardal, 5)));
        return new Simulacao(d);
    }

    [Test]
    public void Colocar_NumLugarBom_CompraEEsvaziaAMao()
    {
        var s = Partida();
        var selecao = new Selecao(s);
        var mudancas = new List<EspeciePorco?>();
        selecao.ColocacaoMudou += e => mudancas.Add(e);

        selecao.IniciarColocacao(EspeciePorco.Estilingueiro);
        Assert.AreEqual(Resultado.Ok, selecao.TentarColocar(LugarBom));

        Assert.AreEqual(1, s.Porcos.Count);
        Assert.IsFalse(selecao.Colocando.HasValue);
        CollectionAssert.AreEqual(new EspeciePorco?[] { EspeciePorco.Estilingueiro, null }, mudancas);
    }

    [Test]
    public void Colocar_NaTrilha_AvisaEOPorcoContinuaNaMao()
    {
        var s = Partida();
        var selecao = new Selecao(s);
        var falhas = new List<Resultado>();
        selecao.Falhou += r => falhas.Add(r);

        selecao.IniciarColocacao(EspeciePorco.Estilingueiro);
        Assert.AreEqual(Resultado.LugarInvalido, selecao.TentarColocar(NaTrilha));

        Assert.AreEqual(EspeciePorco.Estilingueiro, selecao.Colocando);
        CollectionAssert.AreEqual(new[] { Resultado.LugarInvalido }, falhas);
        Assert.AreEqual(0, s.Porcos.Count);
    }

    [Test]
    public void PegarPorcoDaLoja_TiraASelecaoDoMapa()
    {
        var s = Partida();
        var selecao = new Selecao(s);
        s.Comprar(EspeciePorco.Estilingueiro, LugarBom, out var porco);
        selecao.Selecionar(selecao.PorcoEm(LugarBom + new Vector2(0.2f, 0f)));
        Assert.AreSame(porco, selecao.Selecionado);

        selecao.IniciarColocacao(EspeciePorco.Estilingueiro);

        Assert.IsNull(selecao.Selecionado);
    }

    [Test]
    public void VenderOSelecionado_LimpaASelecao()
    {
        var s = Partida();
        var selecao = new Selecao(s);
        s.Comprar(EspeciePorco.Estilingueiro, LugarBom, out var porco);
        selecao.Selecionar(porco);
        Porco avisado = porco;
        selecao.SelecaoMudou += p => avisado = p;

        s.Vender(porco);

        Assert.IsNull(selecao.Selecionado);
        Assert.IsNull(avisado);
    }

    [Test]
    public void FimDeJogo_NadaFicaNaMao()
    {
        var s = Partida();
        var selecao = new Selecao(s);
        s.Comprar(EspeciePorco.Estilingueiro, LugarBom, out var porco);
        selecao.Selecionar(porco);
        selecao.IniciarColocacao(EspeciePorco.Estilingueiro);

        s.SoltarOnda();
        for (int i = 0; i < 60 * 60 && s.Estado != EstadoJogo.Derrota; i++) s.Passo(); // a Torre tem 1 de vida
        Assert.AreEqual(EstadoJogo.Derrota, s.Estado);

        Assert.IsFalse(selecao.Colocando.HasValue);
        Assert.IsNull(selecao.Selecionado);
    }

    [Test]
    public void PorcoEm_ForaDeMeiaUnidade_NaoAchaNinguem()
    {
        var s = Partida();
        var selecao = new Selecao(s);
        s.Comprar(EspeciePorco.Estilingueiro, LugarBom, out _);

        Assert.IsNull(selecao.PorcoEm(LugarBom + new Vector2(0.6f, 0f)));
    }
}
