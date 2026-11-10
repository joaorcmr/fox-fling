using NUnit.Framework;
using UnityEngine;
using static CenarioTeste;

// Upgrades: dois níveis em sequência e, no nível 3, a escolha de uma de duas
// especializações. Tudo cobra moedas e entra no que a venda devolve.
public class MelhoriasTests
{
    static DadosJogo Dados(int moedas = 3000)
    {
        var d = Basico();
        d.economia.moedasIniciais = moedas;
        var porco = Porco(EspeciePorco.Estilingueiro, 120, Tiro(TipoTiro.Pedrinha, recarga: 0.8f));
        porco.nivel2.atributos.recarga = 0.55f;
        porco.nivel3.atributos.dano = 2f;
        porco.especializacaoA.atributos.tiros = 3;
        porco.especializacaoB.atributos.alcance = 5.5f;
        d.porcos[EspeciePorco.Estilingueiro] = porco;
        return d;
    }

    static Porco Comprado(Simulacao s, float x = 7f, float y = 10f)
    {
        Assert.AreEqual(Resultado.Ok, s.Comprar(EspeciePorco.Estilingueiro, new Vector2(x, y), out var p));
        return p;
    }

    [Test]
    public void Melhorar_SobeNivelECobra()
    {
        var s = new Simulacao(Dados());
        var p = Comprado(s);
        int antes = s.Moedas;
        Porco melhorado = null;
        s.PorcoMelhorado += x => melhorado = x;

        Assert.AreEqual(100, s.ProximaMelhoria(p).custo);
        Assert.AreEqual(Resultado.Ok, s.Melhorar(p));

        Assert.AreEqual(2, p.Nivel);
        Assert.AreEqual(antes - 100, s.Moedas);
        Assert.AreEqual(220, p.Gasto);
        Assert.AreEqual(0.55f, p.Atributos.recarga, 1e-5f);
        Assert.AreSame(p, melhorado);
    }

    [Test]
    public void Melhorar_SemMoedas()
    {
        var s = new Simulacao(Dados(moedas: 170));
        var p = Comprado(s); // sobram 50

        Assert.AreEqual(Resultado.SemMoedas, s.Melhorar(p));
        Assert.AreEqual(1, p.Nivel);
        Assert.AreEqual(50, s.Moedas);
    }

    [Test]
    public void Melhorar_NoNivel3_Indisponivel()
    {
        var s = new Simulacao(Dados());
        var p = Comprado(s);
        s.Melhorar(p);
        s.Melhorar(p);

        Assert.AreEqual(3, p.Nivel);
        Assert.IsNull(s.ProximaMelhoria(p));
        Assert.AreEqual(Resultado.Indisponivel, s.Melhorar(p)); // no 3 é hora de especializar
    }

    [Test]
    public void Especializar_SoNoNivel3()
    {
        var s = new Simulacao(Dados());
        var p = Comprado(s);

        Assert.AreEqual(Resultado.Indisponivel, s.Especializar(p, 0));
        Assert.AreEqual(-1, p.Especializacao);
    }

    [Test]
    public void Especializar_EscolheAouB()
    {
        var s = new Simulacao(Dados());
        var a = Comprado(s, 6f, 10f);
        var b = Comprado(s, 8f, 10f);
        foreach (var p in new[] { a, b })
        {
            s.Melhorar(p);
            s.Melhorar(p);
        }

        Assert.AreEqual(Resultado.Ok, s.Especializar(a, 0));
        Assert.AreEqual(Resultado.Ok, s.Especializar(b, 1));

        Assert.AreEqual(4, a.Nivel);
        Assert.AreEqual(0, a.Especializacao);
        Assert.AreEqual(3, a.Atributos.tiros);
        Assert.AreEqual(1, b.Especializacao);
        Assert.AreEqual(5.5f, b.Atributos.alcance, 1e-5f);
        Assert.AreEqual(120 + 100 + 180 + 500, a.Gasto);
    }

    [Test]
    public void Especializar_DuasVezes_Indisponivel()
    {
        var s = new Simulacao(Dados());
        var p = Comprado(s);
        s.Melhorar(p);
        s.Melhorar(p);
        s.Especializar(p, 0);

        Assert.AreEqual(Resultado.Indisponivel, s.Especializar(p, 1));
        Assert.AreEqual(0, p.Especializacao);
    }

    [Test]
    public void ValorVenda_IncluiMelhorias()
    {
        var s = new Simulacao(Dados());
        var p = Comprado(s);
        s.Melhorar(p);
        s.Melhorar(p);

        Assert.AreEqual(280, s.ValorVenda(p)); // 70% de 120 + 100 + 180
    }

    [Test]
    public void Melhoria_NaoMudaTiroNoAr()
    {
        var d = Dados();
        var tipo = d.porcos[EspeciePorco.Estilingueiro];
        tipo.nivel1 = Tiro(TipoTiro.Pedrinha, dano: 1f, alcance: 10f, recarga: 10f, velocidade: 4f);
        tipo.nivel2.atributos = Tiro(TipoTiro.Pedrinha, dano: 2f, alcance: 10f, recarga: 10f, velocidade: 4f);
        d.passaros[EspeciePassaro.Pardal] = Passaro(EspeciePassaro.Pardal, vida: 5f);
        var s = ComPassarosParados(d, EspeciePassaro.Pardal, 3f);
        var p = Comprado(s, 9f, 10f);

        s.Passo();               // atira com dano 1 (o tiro leva ~1,6 s para chegar)
        s.Melhorar(p);           // agora o porco tem dano 2...
        Avancar(s, 3f);

        Assert.AreEqual(4f, s.Passaros[0].Vida, 1e-4f); // ...mas o tiro que já saiu tira 1
    }
}
