using System.IO;
using NUnit.Framework;
using UnityEditor;

// O jogo se chama Fox Tower. O nome antigo (Revoada) só pode sobrar em nomes de
// código (pasta, assemblies, classes), nunca no que o jogador ou quem abre o
// Editor lê: título do jogo, textos das cenas e menu do Editor.
public class IdentidadeTests
{
    const string NomeAntigo = "Revoada";

    [Test]
    public void NomeDoJogo_EFoxTower()
    {
        Assert.AreEqual("Fox Tower", Identidade.NomeDoJogo);
    }

    [Test]
    public void TituloDoProjeto_EONomeDoJogo()
    {
        // Quem grava o título é o ConstrutorRevoada.ConfigurarProjeto (Reconstruir tudo).
        Assert.AreEqual(Identidade.NomeDoJogo, PlayerSettings.productName);
    }

    // Os textos da interface ficam gravados nas cenas. Nenhum deles pode citar o
    // nome antigo (nem "soltar a revoada": agora é "soltar a onda").
    [TestCase(ConstrutorCenas.CenaMenu)]
    [TestCase(ConstrutorCenas.CenaJogo)]
    public void TextosDaCena_NaoCitamONomeAntigo(string cena)
    {
        Assert.IsTrue(File.Exists(cena), cena + " não existe");
        foreach (var linha in File.ReadAllLines(cena))
        {
            // "Revoada::Hud" é o assembly do componente, não um texto da tela.
            if (linha.Contains("m_EditorClassIdentifier")) continue;
            StringAssert.DoesNotContain(NomeAntigo.ToLowerInvariant(), linha.ToLowerInvariant(), cena);
        }
    }

    [Test]
    public void MenuDoEditor_TemONomeDoJogo()
    {
        int doJogo = 0;
        foreach (var metodo in TypeCache.GetMethodsWithAttribute<MenuItem>())
        {
            if (metodo.DeclaringType != typeof(ConstrutorRevoada)) continue;
            foreach (MenuItem item in metodo.GetCustomAttributes(typeof(MenuItem), false))
            {
                StringAssert.StartsWith(Identidade.NomeDoJogo + "/", item.menuItem);
                doJogo++;
            }
        }
        Assert.GreaterOrEqual(doJogo, 5, "os itens do menu do construtor sumiram");
    }
}
