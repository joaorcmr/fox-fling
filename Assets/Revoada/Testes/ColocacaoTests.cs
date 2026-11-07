using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using static CenarioTeste;

// Onde dá para colocar um porco: fora da trilha, dos obstáculos, da Torre, dos
// outros porcos e dentro do mapa (a barra lateral fica fora, em x > 14).
public class ColocacaoTests
{
    const float Raio = 0.45f;
    DadosMapa mapa;
    Trilha trilha;
    List<Porco> porcos;

    [SetUp]
    public void Montar()
    {
        var d = Basico(); // trilha reta em y = 7,5, meia-largura 0,6, Torre em (12, 3)
        mapa = d.mapa;
        mapa.obstaculos.Add(new Obstaculo { tipo = "pedra", centro = new Vector2(3f, 11f), raio = 0.5f });
        trilha = new Trilha(mapa.pontos, mapa.raioCanto);
        porcos = new List<Porco>
        {
            new Porco(1, Porco(EspeciePorco.Estilingueiro, 120, Tiro(TipoTiro.Pedrinha)), new Vector2(7f, 11f)),
        };
    }

    bool Valida(float x, float y) => Colocacao.Valida(new Vector2(x, y), Raio, mapa, trilha, porcos);

    [Test] public void NaTrilha_Invalido() => Assert.IsFalse(Valida(7f, 7.5f));
    [Test] public void PertoDaTrilha_Invalido() => Assert.IsFalse(Valida(7f, 8.4f));   // 0,9 < 0,6 + 0,45
    [Test] public void LongeDaTrilha_Valido() => Assert.IsTrue(Valida(7f, 9.6f));
    [Test] public void ForaDoMapa_Invalido() => Assert.IsFalse(Valida(13.8f, 11f));     // a barra lateral começa em x = 14
    [Test] public void SobreObstaculo_Invalido() => Assert.IsFalse(Valida(3.5f, 11.3f));
    [Test] public void SobreOutroPorco_Invalido() => Assert.IsFalse(Valida(7.8f, 11f)); // 0,8 < 0,45 + 0,45
    [Test] public void EncostadoNoOutro_Valido() => Assert.IsTrue(Valida(7.95f, 11f));
    [Test] public void NaTorre_Invalido() => Assert.IsFalse(Valida(12f, 3.8f));
}
