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

    // O botão de reparar a Torre, na barra de verdade (issue #14): cura e sobe
    // o preço do próximo, e fica bloqueado com a onda em andamento.
    [UnityTest]
    public IEnumerator BotaoReparo_CuraESobeOPreco_EFicaBloqueadoNaOnda()
    {
        yield return CarregarJogo();
        var jogo = Jogo.Atual;
        var hud = Object.FindAnyObjectByType<Hud>();
        var s = jogo.Sim;

        // Sem porco nenhum, a primeira onda já bate na Torre: tira um pouco de vida de verdade.
        s.SoltarOnda();
        for (int i = 0; i < 60 * 60 * 20 && s.Vida == s.VidaMaxima; i++) s.Passo();
        Assert.Less(s.Vida, s.VidaMaxima, "nenhum pássaro bateu na Torre a tempo");
        for (int i = 0; i < 60 * 60 * 20 && s.Estado == EstadoJogo.EmOnda; i++) s.Passo(); // espera a onda acabar

        int custoAntes = s.Economia.CustoProximoReparo;
        int vidaAntes = s.Vida;
        hud.botaoReparo.onClick.Invoke();

        Assert.Greater(s.Vida, vidaAntes, "o botão de verdade devia ter curado algo");
        Assert.Greater(s.Economia.CustoProximoReparo, custoAntes, "o próximo reparo fica mais caro");

        s.SoltarOnda();
        int vidaEmOnda = s.Vida;
        hud.botaoReparo.onClick.Invoke();
        Assert.AreEqual(vidaEmOnda, s.Vida, "com a onda em andamento, o botão não pode curar nada");
    }

    // A troca de fase é um momento claro, para o jogador não se perder: os
    // porcos voltam como moedas pelo valor inteiro, o mapa escurece e um quadro
    // anuncia a fase nova e espera; enquanto isso, a loja e o mapa não recebem
    // cliques. Só depois de "Ver o mapa novo" o jogo volta a responder — e a
    // próxima onda continua esperando o jogador soltar.
    [UnityTest]
    public IEnumerator TrocaDeFase_DevolveOsPorcos_MostraOQuadro_ETravaAteOJogadorVerOMapa()
    {
        yield return CarregarJogo();
        var s = Jogo.Atual.Sim;
        var troca = Object.FindAnyObjectByType<TrocaDeFase>();

        // Moedas de sobra e Bombardeiros no máximo espalhados: a fase 1 passa sem a Torre cair.
        s.Economia.Creditar(200000);
        for (float x = 1f; x < 14f; x += 1.5f)
            for (float y = 1f; y < 15f; y += 1.5f)
                if (s.Porcos.Count < 25 && s.Comprar(EspeciePorco.Bombardeiro, new Vector2(x, y), out var p) == Resultado.Ok)
                {
                    s.Melhorar(p);
                    s.Melhorar(p);
                    s.Especializar(p, 0);
                }
        Assert.Greater(s.Porcos.Count, 5, "não achou lugar para os porcos");
        int gasto = 0;
        foreach (var p in s.Porcos) gasto += p.Gasto;
        int moedasAntes = s.Moedas;

        for (int onda = 1; onda <= 10 && s.Estado != EstadoJogo.Derrota; onda++)
        {
            s.SoltarOnda();
            for (int i = 0; i < 60 * 60 * 5 && s.Estado == EstadoJogo.EmOnda; i++) s.Passo();
        }
        Assert.AreEqual(2, s.Fase, "a fase 1 devia ter acabado");
        Assert.AreEqual(0, s.Porcos.Count, "na troca, os porcos saem do mapa");
        Assert.GreaterOrEqual(s.Moedas, moedasAntes + gasto, "o valor inteiro dos porcos voltou para a carteira");
        Assert.IsTrue(troca.Ativa, "a troca de fase não começou");
        Assert.IsTrue(troca.grupo.blocksRaycasts, "durante a troca, a loja e o mapa ainda recebem cliques");

        // O quadro aparece depois de os porcos virarem moedas e de o mapa escurecer.
        float limite = Time.realtimeSinceStartup + 5f;
        while (!troca.quadro.gameObject.activeSelf && Time.realtimeSinceStartup < limite) yield return null;
        Assert.IsTrue(troca.quadro.gameObject.activeSelf, "o quadro da fase nova não apareceu");
        StringAssert.Contains("Fase 2", troca.titulo.text);
        Assert.IsTrue(troca.Ativa, "o quadro espera o jogador");

        troca.botaoContinuar.onClick.Invoke();
        limite = Time.realtimeSinceStartup + 5f;
        while (troca.Ativa && Time.realtimeSinceStartup < limite) yield return null;
        Assert.IsFalse(troca.Ativa, "depois de ver o mapa novo, o jogo volta a responder");
        Assert.IsFalse(troca.grupo.blocksRaycasts);
        Assert.AreEqual(EstadoJogo.Intervalo, s.Estado, "nenhuma onda começa sozinha depois da troca");
    }

    // A Esthefanny pediu para tirar o Auto: nenhuma onda pode começar sozinha,
    // só quando o jogador soltar (botão ou Espaço).
    [UnityTest]
    public IEnumerator Hud_NaoTemBotaoAuto_OndaSoComecaAoSoltar()
    {
        yield return CarregarJogo();
        var hud = Object.FindAnyObjectByType<Hud>();
        var s = Jogo.Atual.Sim;

        Assert.IsNull(hud.transform.Find("BarraLateral/BotaoAuto"), "ainda existe um botão de Auto na barra");
        Assert.IsFalse(s.Auto, "a simulação não devia nascer com o Auto ligado");

        for (int i = 0; i < 300; i++) s.Passo(); // 5 s sem ninguém soltar a onda
        Assert.AreEqual(EstadoJogo.Preparando, s.Estado, "a onda começou sozinha");

        hud.botaoSoltar.onClick.Invoke();
        Assert.AreEqual(EstadoJogo.EmOnda, s.Estado, "o botão continua soltando a onda normalmente");
    }

    // O balão da loja tinha altura fixa e descrições compridas (a do
    // Bombardeiro, por exemplo) podiam vazar pra fora dele — o quanto depende
    // da fonte de cada plataforma, então o teste força um texto grande o
    // bastante pra precisar de mais espaço em qualquer fonte, em vez de
    // confiar no tamanho exato de uma descrição real.
    [UnityTest]
    public IEnumerator BalaoDaLoja_CrescePraCaberDescricoesCompridas()
    {
        yield return CarregarJogo();
        var cartao = Object.FindObjectsByType<CartaoPorcoLoja>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        CartaoPorcoLoja algum = null;
        foreach (var c in cartao) algum = c;
        Assert.IsNotNull(algum);
        var balao = algum.balao; // o balão fica desativado até o mouse passar em cima: FindAnyObjectByType não o acha

        string textoComprido = string.Join(" ", System.Linq.Enumerable.Repeat("palavra", 40));
        balao.Mostrar("Título", textoComprido, (RectTransform)algum.transform);
        float alturaComprida = balao.caixa.sizeDelta.y;
        Assert.That(alturaComprida, Is.GreaterThanOrEqualTo(balao.texto.preferredHeight + 29f),
            "o balão precisa ser alto o bastante pro texto comprido inteiro");
        Assert.That(alturaComprida, Is.GreaterThan(62f), "o texto escolhido precisa realmente exigir mais altura (senão o teste não prova nada)");

        balao.Mostrar("Título", "Uma linha.", (RectTransform)algum.transform);
        float alturaCurta = balao.caixa.sizeDelta.y;
        Assert.That(alturaCurta, Is.LessThan(alturaComprida), "um texto de uma linha não devia manter o balão esticado");
    }

    // O aviso "Novo: <pássaro> (<detalhe>)" da prévia quebrava em 2 linhas
    // pros detalhes mais compridos, e a segunda linha ficava embaixo do botão
    // de soltar a onda (o cartão só tem espaço certo pra 1 linha). A onda 6 é
    // onde o Beija-flor (rápido) estreia de verdade.
    [UnityTest]
    public IEnumerator PreviaDaOnda_AvisoDeEstreiaNaoQuebraLinha()
    {
        yield return CarregarJogo();
        var p = Object.FindAnyObjectByType<PreviaOnda>();

        p.Mostrar(6);

        Assert.AreEqual("Novo: Beija-flor", p.aviso.text, "sem o detalhe: com ele, quebra e vaza pro botão embaixo");
        Assert.That(p.aviso.preferredWidth, Is.LessThanOrEqualTo(p.aviso.rectTransform.rect.width));

        // O texto (não só a caixa) precisa ficar acima do botão de soltar a
        // onda: a caixa do aviso central (MiddleCenter) pode encostar no
        // botão mesmo com texto de 1 linha só, se o cartão ficar baixo demais.
        var soltar = Object.FindAnyObjectByType<Hud>().botaoSoltar.GetComponent<RectTransform>();
        var linha = p.aviso.cachedTextGenerator.lines[p.aviso.cachedTextGenerator.lineCount - 1];
        var corners = new Vector3[4];
        p.aviso.rectTransform.GetWorldCorners(corners);
        float linhaBase = corners[1].y + linha.topY - linha.height;
        var soltarCantos = new Vector3[4];
        soltar.GetWorldCorners(soltarCantos);
        Assert.That(linhaBase, Is.GreaterThan(soltarCantos[1].y), "o texto do aviso encosta no botão de soltar a onda");
    }

    // Não ficava claro que dava para soltar a onda fora da troca de fase: o
    // botão só pulsava (chamava atenção) logo depois de um mapa novo. Agora
    // pulsa sempre que a ação está disponível.
    [UnityTest]
    public IEnumerator Hud_BotaoSoltarPulsaMesmoSemTrocaDeFase()
    {
        yield return CarregarJogo();
        var hud = Object.FindAnyObjectByType<Hud>();
        var s = Jogo.Atual.Sim;
        Assert.AreEqual(EstadoJogo.Preparando, s.Estado);

        bool pulsou = false;
        for (int i = 0; i < 60 && !pulsou; i++)
        {
            yield return null;
            pulsou = hud.botaoSoltar.transform.localScale.x > 1.001f;
        }
        Assert.IsTrue(pulsou, "o botão devia pulsar mesmo numa onda comum (sem troca de fase)");
    }

    // Um pássaro novo (a estreia de uma espécie) agora ganha um anúncio na
    // faixa do topo do mapa, não só o texto discreto da prévia.
    [UnityTest]
    public IEnumerator Hud_PassaroNovoApareceNaFaixaDoTopo()
    {
        yield return CarregarJogo();
        var hud = Object.FindAnyObjectByType<Hud>();
        var s = Jogo.Atual.Sim;
        var textoFaixa = hud.faixa.GetComponentInChildren<UnityEngine.UI.Text>(true);

        Robos.AvancarAteOnda(s, 6);
        Assert.AreEqual(6, s.Onda, "a onda 6 (estreia do Beija-flor) não chegou");

        bool anunciou = false;
        float fim = Time.realtimeSinceStartup + 4f; // o anúncio espera a faixa "Onda 6" sumir (1,9 s) antes de aparecer
        while (Time.realtimeSinceStartup < fim && !anunciou)
        {
            yield return null;
            anunciou = textoFaixa.text == "Novo: Beija-flor";
        }
        Assert.IsTrue(anunciou, "a faixa não anunciou o Beija-flor estreando na onda 6");
    }
}
