using NUnit.Framework;
using static CenarioTeste;

// Dois dos padrões de projeto da issue #17 (ver README > "Padrões de
// projeto") têm um comportamento fácil de travar com um teste: o Flyweight
// (DadosPorco compartilhado) e o Prototype (Atributos.Copia()). Os outros
// sete (State, Observer, Singleton, Game Loop, Update Method, Component,
// Entity Component System) já são cobertos pelo resto da suíte, só que
// indiretamente — são a própria forma como o jogo é escrito, não uma
// regrinha isolada para testar.
public class PadroesTests
{
    static DadosJogo ComDoisEstilingueiros()
    {
        var d = Basico();
        d.porcos[EspeciePorco.Estilingueiro] = Porco(EspeciePorco.Estilingueiro, 120, Tiro(TipoTiro.Pedrinha));
        return d;
    }

    // Flyweight: todo porco da mesma espécie aponta para o MESMO DadosPorco
    // (o "peso-pena" compartilhado) em vez de cada um carregar sua própria
    // cópia de nome, custo, raio e das 3 melhorias.
    [Test]
    public void Flyweight_PorcosDaMesmaEspecieCompartilhamODadosPorco()
    {
        var s = new Simulacao(ComDoisEstilingueiros());
        s.Comprar(EspeciePorco.Estilingueiro, new UnityEngine.Vector2(2f, 2f), out var p1);
        s.Comprar(EspeciePorco.Estilingueiro, new UnityEngine.Vector2(4f, 2f), out var p2);

        Assert.AreSame(p1.Dados, p2.Dados, "dois Estilingueiros deviam compartilhar o mesmo DadosPorco");
    }

    // Prototype: Copia() devolve um objeto SEPARADO (mesmos valores, endereços
    // diferentes), então mudar a cópia não pode mudar o original.
    [Test]
    public void Prototype_CopiaDosAtributosFicaIndependenteDoOriginal()
    {
        var original = Tiro(TipoTiro.Pedrinha, dano: 5f, alcance: 8f);

        var copia = original.Copia();
        copia.dano = 999f;

        Assert.AreNotSame(original, copia, "Copia() devia devolver um objeto novo");
        Assert.AreEqual(5f, original.dano, "mudar a cópia não pode mudar o original");
        Assert.AreEqual(8f, copia.alcance, "a cópia começa com os mesmos valores do original");
    }
}
