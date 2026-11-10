using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using static CenarioTeste;

// Derrotar um pássaro rende MOEDAS (a moeda do jogo já foram as penas, e o
// Cofrinho já foi o Ninho de Penas). Aqui ficam a regra do ganho e a garantia
// de que nada do que o jogador lê ou vê ainda fala em penas. As penas que
// continuam no jogo são só as que voam dos pássaros (efeito visual).
public class MoedasTests
{
    // "pena"/"penas" como palavra inteira ("apenas" não conta) e "ninho".
    static readonly Regex NomeAntigo = new Regex(@"\bpenas?\b|ninho", RegexOptions.IgnoreCase);

    static bool FalaDePenas(string texto) => NomeAntigo.IsMatch(texto.Replace("Vale a pena", ""));

    [Test]
    public void EstourarPassaro_RendeAsMoedasDele()
    {
        var d = Basico();
        d.economia.moedasIniciais = 0;
        d.passaros[EspeciePassaro.Pardal] = Passaro(EspeciePassaro.Pardal, vida: 2f, moedas: 3f);
        var s = ComPassarosParados(d, EspeciePassaro.Pardal, 5f);
        int avisadas = -1;
        s.PassaroEstourou += (p, moedas) => avisadas = moedas;

        s.Atingir(s.Passaros[0], 1f, false);
        Assert.AreEqual(0, s.Moedas, "pássaro só ferido não rende nada");

        s.Atingir(s.Passaros[0], 1f, false);
        Assert.AreEqual(3, s.Moedas);
        Assert.AreEqual(3, avisadas, "o \"+N\" que aparece na tela tem que ser o que entrou");
    }

    [Test]
    public void PassaroQueChegaNaTorre_NaoRendeMoedas()
    {
        var d = Basico();
        d.economia.moedasIniciais = 0;
        d.passaros[EspeciePassaro.Pardal] = Passaro(EspeciePassaro.Pardal, velocidade: 7f, moedas: 3f);
        d.ondas.Add(Onda(Grupo(EspeciePassaro.Pardal, 1)));
        d.ondas.Add(Onda(Grupo(EspeciePassaro.Pardal, 1)));
        var s = new Simulacao(d);
        int chegaram = 0;
        s.PassaroChegou += p => chegaram++;
        s.SoltarOnda();

        // No passo em que o pardal chega, a onda também acaba (e paga o bônus dela).
        for (int i = 0; i < 600 && chegaram == 0; i++) s.Passo();

        Assert.AreEqual(1, chegaram);
        Assert.AreEqual(s.Economia.Bonus(1), s.Moedas, "só o bônus da onda: o pardal que passou não rende moedas");
    }

    [Test]
    public void Cofrinho_FazCadaPassaroRenderMaisMoedas()
    {
        var d = Basico();
        d.economia.moedasIniciais = 250;
        d.passaros[EspeciePassaro.Pardal] = Passaro(EspeciePassaro.Pardal, moedas: 4f);
        var s = ComPassarosParados(d, EspeciePassaro.Pardal, 5f);

        Assert.AreEqual(Resultado.Ok, s.ComprarCofrinho()); // ×1,25 por 250
        s.Atingir(s.Passaros[0], 1f, false);

        Assert.AreEqual(5, s.Moedas);
    }

    [Test]
    public void TextosDoJogo_NaoFalamEmPenas()
    {
        var textos = new List<string>();
        foreach (var p in DefinicoesPadrao.Passaros()) textos.Add(p.nome);
        foreach (var p in DefinicoesPadrao.Porcos())
        {
            textos.Add(p.nome);
            textos.Add(p.papel);
            foreach (var m in new[] { p.nivel2, p.nivel3, p.especializacaoA, p.especializacaoB })
            {
                textos.Add(m.nome);
                textos.Add(m.descricao);
            }
        }
        foreach (var id in CatalogoDicas.Ids) textos.Add(CatalogoDicas.Texto(id));

        foreach (var texto in textos)
            Assert.IsFalse(FalaDePenas(texto ?? ""), "ainda fala em penas ou Ninho: \"" + texto + "\"");
    }

    // Os textos fixos da interface ficam gravados nas cenas (m_Text).
    [TestCase(ConstrutorCenas.CenaMenu)]
    [TestCase(ConstrutorCenas.CenaJogo)]
    public void TextosDaCena_NaoFalamEmPenas(string cena)
    {
        foreach (var linha in File.ReadAllLines(cena))
            if (linha.Contains("m_Text:"))
                Assert.IsFalse(FalaDePenas(linha), cena + " ainda fala em penas ou Ninho: " + linha.Trim());
    }

    [Test]
    public void Icones_SaoDeMoedaEDeCofrinho()
    {
        Assert.IsNotNull(ConstrutorAssets.Sprite("icone_moeda"), "falta o ícone da moeda");
        Assert.IsNotNull(ConstrutorAssets.Sprite("icone_cofrinho"), "falta o ícone do Cofrinho");
        Assert.IsNull(ConstrutorAssets.Sprite("icone_pena"), "o ícone da pena ainda existe");
        Assert.IsNull(ConstrutorAssets.Sprite("icone_ninho"), "o ícone do Ninho ainda existe");
    }
}
