using NUnit.Framework;
using UnityEngine;
using static CenarioTeste;

// O Tamboreiro e o "agrupar": porcos dentro da aura atiram mais rápido (ou
// alcançam mais longe, ou rendem mais penas). Auras não se somam: vale a maior.
public class AurasTests
{
    static DadosJogo ComTambor(float bonusCadencia = 0.2f, float raio = 2.5f, float bonusAlcance = 0f, float bonusPenas = 0f)
    {
        var d = Basico();
        d.economia.penasIniciais = 5000;
        d.porcos[EspeciePorco.Estilingueiro] = Porco(EspeciePorco.Estilingueiro, 120, Tiro(TipoTiro.Pedrinha, alcance: 3f, recarga: 1f));
        d.porcos[EspeciePorco.Tamboreiro] = Porco(EspeciePorco.Tamboreiro, 300, new Atributos
        {
            tiro = TipoTiro.Nenhum, alcance = raio, raioAura = raio,
            bonusCadencia = bonusCadencia, bonusAlcance = bonusAlcance, bonusPenas = bonusPenas,
        });
        return d;
    }

    static Porco Comprar(Simulacao s, EspeciePorco e, float x, float y)
    {
        Assert.AreEqual(Resultado.Ok, s.Comprar(e, new Vector2(x, y), out var p));
        return p;
    }

    [Test]
    public void Tamboreiro_AceleraQuemEstaNoRaio()
    {
        var s = new Simulacao(ComTambor());
        Comprar(s, EspeciePorco.Tamboreiro, 7f, 11f);
        var perto = Comprar(s, EspeciePorco.Estilingueiro, 9f, 11f);   // a 2,0 do tambor
        var longe = Comprar(s, EspeciePorco.Estilingueiro, 10f, 11f);  // a 3,0

        Assert.AreEqual(1.2f, perto.MultCadencia, 1e-5f);
        Assert.AreEqual(1f, longe.MultCadencia, 1e-5f);
    }

    [Test]
    public void Auras_NaoSeSomam()
    {
        var d = ComTambor(bonusCadencia: 0.2f);
        d.porcos[EspeciePorco.Tamboreiro].nivel2.atributos.bonusCadencia = 0.35f;
        var s = new Simulacao(d);
        Comprar(s, EspeciePorco.Tamboreiro, 6f, 11f);
        var forte = Comprar(s, EspeciePorco.Tamboreiro, 8f, 11f);
        s.Melhorar(forte);
        var porco = Comprar(s, EspeciePorco.Estilingueiro, 7f, 12.2f); // dentro das duas auras

        Assert.AreEqual(1.35f, porco.MultCadencia, 1e-5f); // a maior, não 1 + 0,2 + 0,35
    }

    [Test]
    public void GritoDeGuerra_AumentaAlcance()
    {
        var s = new Simulacao(ComTambor(bonusCadencia: 0f, bonusAlcance: 0.2f));
        Comprar(s, EspeciePorco.Tamboreiro, 7f, 11f);
        var porco = Comprar(s, EspeciePorco.Estilingueiro, 8.5f, 11f);

        Assert.AreEqual(1.2f, porco.MultAlcance, 1e-5f);
        Assert.AreEqual(3.6f, porco.Alcance, 1e-4f);
    }

    [Test]
    public void TamborDasPenas_RendeMaisNaAura()
    {
        var d = ComTambor(bonusCadencia: 0f, bonusPenas: 0.5f);
        d.passaros[EspeciePassaro.Pardal] = Passaro(EspeciePassaro.Pardal, penas: 2f);
        var s = ComPassarosParados(d, EspeciePassaro.Pardal, 7f); // em (7; 7,5)
        Comprar(s, EspeciePorco.Tamboreiro, 7f, 9.5f);             // a aura chega no pardal
        int antes = s.Penas;

        s.Atingir(s.Passaros[0], 10f, false);

        Assert.AreEqual(antes + 3, s.Penas); // 2 × 1,5
    }

    [Test]
    public void VenderTambor_TiraAura()
    {
        var s = new Simulacao(ComTambor());
        var tambor = Comprar(s, EspeciePorco.Tamboreiro, 7f, 11f);
        var porco = Comprar(s, EspeciePorco.Estilingueiro, 9f, 11f);
        Assert.AreEqual(1.2f, porco.MultCadencia, 1e-5f);

        s.Vender(tambor);

        Assert.AreEqual(1f, porco.MultCadencia, 1e-5f);
    }

    [Test]
    public void Aura_FazAtirarMaisRapido()
    {
        var d = ComTambor(bonusCadencia: 0.5f);
        d.passaros[EspeciePassaro.Pardal] = Passaro(EspeciePassaro.Pardal, vida: 1000f);
        var s = ComPassarosParados(d, EspeciePassaro.Pardal, 7f);
        Comprar(s, EspeciePorco.Tamboreiro, 5.5f, 10.5f);
        Comprar(s, EspeciePorco.Estilingueiro, 7f, 10f);
        int tiros = 0;
        s.TiroDisparado += (porco, tiro) => tiros++;

        Avancar(s, 3f); // recarga 1 ÷ 1,5: atira em 0; 0,67; 1,33; 2,0 e 2,67 s

        Assert.AreEqual(5, tiros);
    }
}
