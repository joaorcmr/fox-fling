using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// Testes de Play Mode: o Unity carrega a cena Jogo de verdade e roda quadros.
// Cobrem o que só existe com cenas e tempo — o escurecer entre cenas, o
// Time.timeScale e o fim de jogo travando a interface —, que os testes
// EditMode da simulação não alcançam.
public class FluxoDoJogoTests
{
    string prefixoOriginal;

    [SetUp]
    public void Isolar()
    {
        // Dicas vistas, recorde e volumes ficam em chaves de teste: o progresso
        // de quem joga no Editor não muda.
        prefixoOriginal = Salvamento.Prefixo;
        Salvamento.Prefixo = "teste_foxtower_";
        Progresso.Zerar();
    }

    [TearDown]
    public void Restaurar()
    {
        Progresso.Zerar();
        PlayerPrefs.DeleteKey(Salvamento.Prefixo + "volume_efeitos");
        PlayerPrefs.DeleteKey(Salvamento.Prefixo + "volume_musica");
        Salvamento.Prefixo = prefixoOriginal;
        Time.timeScale = 1f;
    }

    static IEnumerator CarregarJogo()
    {
        // Um teste que falhou no meio de um escurecer deixa a troca de cena
        // andando (a Transicao sobrevive entre cenas); espera ela acabar.
        while (Transicao.Trocando) yield return null;
        SceneManager.LoadScene("Jogo");
        yield return null; // a cena entra no quadro seguinte
        yield return null; // e os Start rodam
        Assert.IsNotNull(Jogo.Atual, "a cena Jogo não carregou");
    }

    // Pássaros voando, jogo em 3× e pausado (é assim que se chega ao menu de pausa).
    static IEnumerator OndaEmAndamentoPausadaEm3x()
    {
        yield return CarregarJogo();
        var jogo = Jogo.Atual;
        jogo.Sim.SoltarOnda();
        yield return new WaitForSeconds(0.2f);
        jogo.Relogio.ProximaVelocidade();
        jogo.Relogio.ProximaVelocidade();
        Assert.AreEqual(3, jogo.Relogio.Velocidade);
        jogo.Relogio.Pausar(true);
    }

    [UnityTest]
    public IEnumerator Reiniciar_Pausado_OJogoNaoAndaDuranteOEscurecer()
    {
        yield return OndaEmAndamentoPausadaEm3x();
        var jogo = Jogo.Atual;
        float tempo = jogo.Sim.Tempo;

        Navegacao.ReiniciarPartida();
        yield return new WaitForSecondsRealtime(0.15f); // no meio do escurecer de 0,3 s

        Assert.AreSame(jogo, Jogo.Atual, "a cena trocou antes do fim do escurecer");
        Assert.AreEqual(tempo, jogo.Sim.Tempo, "o jogo andou durante o escurecer, com o menu de pausa na tela");

        yield return new WaitForSecondsRealtime(0.8f);
        Assert.AreNotSame(jogo, Jogo.Atual, "a cena não recarregou");
        Assert.AreEqual(1f, Time.timeScale, "a partida nova nasceu pausada ou acelerada");
    }

    [UnityTest]
    public IEnumerator IrParaMenu_Pausado_OJogoNaoAndaEOMenuNasceEm1x()
    {
        yield return OndaEmAndamentoPausadaEm3x();
        var jogo = Jogo.Atual;
        float tempo = jogo.Sim.Tempo;

        Navegacao.IrParaMenu();
        yield return new WaitForSecondsRealtime(0.15f);
        Assert.AreEqual(tempo, jogo.Sim.Tempo, "o jogo andou durante o escurecer, com o menu de pausa na tela");

        yield return new WaitForSecondsRealtime(0.8f);
        Assert.AreEqual("Menu", SceneManager.GetActiveScene().name);
        Assert.AreEqual(1f, Time.timeScale, "o menu nasceu pausado ou acelerado");
    }

    [UnityTest]
    public IEnumerator FimDeJogo_TravaControlesEBotoes_EAOndaBateComATela()
    {
        yield return CarregarJogo();
        var s = Jogo.Atual.Sim;
        var selecao = Jogo.Atual.Selecao;
        var controles = Object.FindAnyObjectByType<Controles>();
        var hud = Object.FindAnyObjectByType<Hud>();
        var tela = Object.FindAnyObjectByType<TelaFim>();

        selecao.IniciarColocacao(EspeciePorco.Estilingueiro); // com um porco na mão quando a Torre cai
        s.Auto = true;
        s.SoltarOnda();
        for (int i = 0; i < 60 * 60 * 15 && s.Estado != EstadoJogo.Derrota; i++) s.Passo(); // sem porcos, a Torre cai
        Assert.AreEqual(EstadoJogo.Derrota, s.Estado);
        Assert.IsTrue(tela.grupo.blocksRaycasts, "antes de a tela de fim aparecer, cliques ainda chegam à loja e ao mapa");

        yield return null; // um quadro para a HUD se atualizar

        Assert.IsFalse(controles.enabled, "teclas e cliques no mapa continuam valendo depois do fim");
        Assert.IsFalse(selecao.Colocando.HasValue, "o fantasma do porco continua no mapa");
        Assert.IsFalse(hud.botaoMenu.interactable, "o ☰ abre o menu de pausa por cima da tela de fim");
        Assert.IsFalse(hud.botaoVelocidade.interactable);
        Assert.AreEqual($"Onda {s.Onda}/{s.TotalOndas}", hud.textoOnda.text, "a HUD mostra uma onda diferente da tela de fim");
    }

    // Um ícone de pausa avulso ao lado do ☰ pausava do mesmo jeito, por um
    // caminho diferente: dois controles para a mesma coisa (issue #13). Só o
    // ☰ fica na barra, e ele continua pausando ao abrir o menu.
    [UnityTest]
    public IEnumerator Hud_SoOMenuPausaPeloMouse_NaoTemBotaoDePausaAvulso()
    {
        yield return CarregarJogo();
        var jogo = Jogo.Atual;
        var hud = Object.FindAnyObjectByType<Hud>();

        Assert.IsNull(hud.transform.Find("BarraLateral/BotaoPausa"), "ainda existe um botão de pausa avulso na barra");
        Assert.IsFalse(jogo.Relogio.Pausado);

        hud.botaoMenu.onClick.Invoke();

        Assert.IsTrue(jogo.Relogio.Pausado, "o ☰ precisa continuar pausando ao abrir o menu");
        Assert.IsTrue(hud.menuPausa.Aberto);
    }
}
