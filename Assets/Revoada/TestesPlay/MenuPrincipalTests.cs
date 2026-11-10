using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// "Zerar progresso" apaga para sempre (recorde, vitórias, dicas vistas): um
// clique sem querer não pode zerar o progresso do jogador sem chance de
// voltar atrás (controle e liberdade do usuário, issue #11).
public class MenuPrincipalTests
{
    string prefixoOriginal;

    [SetUp]
    public void Isolar()
    {
        prefixoOriginal = Salvamento.Prefixo;
        Salvamento.Prefixo = "teste_foxtower_";
        Progresso.Zerar();
        Progresso.RegistrarFim(12, false); // um recorde para ter o que perder
    }

    [TearDown]
    public void Restaurar()
    {
        Progresso.Zerar();
        Salvamento.Prefixo = prefixoOriginal;
    }

    static IEnumerator CarregarMenu()
    {
        while (Transicao.Trocando) yield return null;
        SceneManager.LoadScene("Menu");
        yield return null;
        yield return null;
        Assert.IsNotNull(Object.FindAnyObjectByType<MenuPrincipal>(), "a cena Menu não carregou");
    }

    [UnityTest]
    public IEnumerator Zerar_PedeConfirmacao_SoApagaDepoisDeConfirmar()
    {
        yield return CarregarMenu();
        var m = Object.FindAnyObjectByType<MenuPrincipal>();

        Assert.IsFalse(m.painelConfirmarZerar.activeSelf, "o diálogo já nasce aberto");

        m.botaoZerar.onClick.Invoke();
        Assert.IsTrue(m.painelConfirmarZerar.activeSelf, "clicar em \"Zerar progresso\" devia abrir a confirmação");
        Assert.AreEqual(12, Progresso.MelhorOnda, "só clicar não pode ter apagado nada ainda");

        m.botaoConfirmarZerar.onClick.Invoke();
        Assert.IsFalse(m.painelConfirmarZerar.activeSelf, "confirmado, o diálogo deveria fechar");
        Assert.AreEqual(0, Progresso.MelhorOnda, "confirmado, agora sim apaga");
    }

    [UnityTest]
    public IEnumerator Zerar_Cancelar_NaoApagaNada()
    {
        yield return CarregarMenu();
        var m = Object.FindAnyObjectByType<MenuPrincipal>();

        m.botaoZerar.onClick.Invoke();
        m.botaoCancelarZerar.onClick.Invoke();

        Assert.IsFalse(m.painelConfirmarZerar.activeSelf, "cancelar devia fechar o diálogo");
        Assert.AreEqual(12, Progresso.MelhorOnda, "cancelar não pode ter apagado o recorde");
    }
}

