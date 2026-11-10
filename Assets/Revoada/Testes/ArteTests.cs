using NUnit.Framework;
using UnityEngine;

// Confere a arte ligada aos TipoPassaro/TipoPorco (issue #15): depois da troca
// das sprites procedurais pelas raposas e pelos bichos do Iran, cada espécie
// ainda precisa ter um sprite de verdade, e o corpo do porco (que já vem com
// a arma desenhada nele) não pode ter uma arma avulsa por cima.
public class ArteTests
{
    [Test]
    public void TodoPassaroTemSprite()
    {
        ConstrutorDados.GarantirDados(false);
        foreach (var tipo in ConstrutorDados.Passaros())
        {
            Assert.IsNotNull(tipo.sprite, tipo.dados.especie.ToString());
            Assert.Greater(tipo.sprite.texture.width, 0, tipo.dados.especie.ToString());
        }
    }

    [Test]
    public void TodoPorcoTemCorpoEAcessorios_SemArmaAvulsa()
    {
        ConstrutorDados.GarantirDados(false);
        foreach (var tipo in ConstrutorDados.Porcos())
        {
            var nome = tipo.dados.especie.ToString();
            Assert.IsNotNull(tipo.corpo, nome);
            Assert.IsNull(tipo.arma, $"{nome}: a arma já vem desenhada no corpo");
            Assert.IsNotNull(tipo.acessorioA, nome);
            Assert.IsNotNull(tipo.acessorioB, nome);
        }
    }
}
