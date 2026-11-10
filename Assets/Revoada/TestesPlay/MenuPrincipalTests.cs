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

    // A tela de início ficava com o texto direto em cima do mapa de enfeite,
    // difícil de ler (issue #16). Os dois cartões precisam mesmo conter o que
    // deviam agrupar, senão o problema original volta.
    [UnityTest]
    public IEnumerator TelaDeInicio_SubtituloEBotoesFicamDentroDoCartaoDeJogar()
    {
        yield return CarregarMenu();
        var m = Object.FindAnyObjectByType<MenuPrincipal>();

        DentroVerticalmente(m.textoRecorde.rectTransform, m.cartaoJogar);
        DentroVerticalmente(m.botaoJogar.GetComponent<RectTransform>(), m.cartaoJogar);
        DentroVerticalmente(m.botaoComoJogar.GetComponent<RectTransform>(), m.cartaoJogar);
    }

    static void DentroVerticalmente(RectTransform dentro, RectTransform fora)
    {
        var a = new Vector3[4];
        var b = new Vector3[4];
        dentro.GetWorldCorners(a);
        fora.GetWorldCorners(b);
        float topoDentro = a[1].y, baseDentro = a[0].y;
        float topoFora = b[1].y, baseFora = b[0].y;
        Assert.That(topoDentro, Is.LessThanOrEqualTo(topoFora + 0.01f), $"{dentro.name} passa do topo do cartão {fora.name}");
        Assert.That(baseDentro, Is.GreaterThanOrEqualTo(baseFora - 0.01f), $"{dentro.name} passa da base do cartão {fora.name}");
    }
}

