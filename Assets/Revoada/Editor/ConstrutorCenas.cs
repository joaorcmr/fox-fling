using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Monta as cenas e as Build Settings: Menu (Menu/Menu.unity, a primeira, onde
// o jogo abre) e Jogo (Partida/Jogo.unity).
public static class ConstrutorCenas
{
    public const string CenaMenu = ConstrutorAssets.Raiz + "/Menu/" + Navegacao.CenaMenu + ".unity";
    public const string CenaJogo = ConstrutorAssets.Raiz + "/Partida/" + Navegacao.CenaJogo + ".unity";

    // A câmera do jogo mostra x de 0 a 18,75 e y de 0 a 15: o mapa (x até 14) e,
    // à direita, o espaço da barra lateral. A do menu fica centrada no mapa.
    public static readonly Vector3 PosicaoCamera = new Vector3(9.375f, 7.5f, -10f);
    static readonly Vector3 PosicaoCameraMenu = new Vector3(7f, 7.5f, -10f);

    public static void Tudo()
    {
        ConstrutorAssets.GarantirPasta(ConstrutorAssets.Raiz + "/Menu");
        ConstrutorAssets.GarantirPasta(ConstrutorAssets.Raiz + "/Partida");
        CriarCenaJogo();
        CriarCenaMenu();
        ConfigurarBuild();
    }

    static void CriarCenaJogo()
    {
        var cena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camera = CriarCamera(PosicaoCamera);
        camera.gameObject.AddComponent<TremorCamera>();
        var sons = CriarSons();

        var jogo = new GameObject("Jogo").AddComponent<Jogo>();
        jogo.passaros = ConstrutorDados.Passaros();
        jogo.porcos = ConstrutorDados.Porcos();
        jogo.ondas = ConstrutorDados.Ondas();
        jogo.economia = ConstrutorDados.Economia();
        jogo.mapa = ConstrutorDados.Fazenda();
        jogo.fases = ConstrutorDados.Fases();

        // Os sons do que acontece no mapa ficam junto do Sons da cena.
        sons.gameObject.AddComponent<SonsDaPartida>().jogo = jogo;

        var mapa = CriarMapa(jogo.mapa, jogo);
        var torre = new GameObject("Torre").AddComponent<VisualTorre>();
        torre.jogo = jogo;
        torre.mapa = mapa;

        // O caminho novo se desenhando com setas depois de uma troca de fase.
        var revelar = new GameObject("RevelarTrilha").AddComponent<RevelarTrilha>();
        revelar.seta = ConstrutorAssets.Sprite("icone_play");
        revelar.anel = ConstrutorAssets.Sprite("anel");
        revelar.material = ConstrutorAssets.MaterialSprite("Revelar", null);

        var efeitos = new GameObject("Efeitos").AddComponent<Efeitos>();
        efeitos.penas = Instanciar<ParticleSystem>(ConstrutorPrefabs.CaminhoParticulas("Penas"), efeitos.transform);
        efeitos.poeira = Instanciar<ParticleSystem>(ConstrutorPrefabs.CaminhoParticulas("Poeira"), efeitos.transform);
        efeitos.brilho = Instanciar<ParticleSystem>(ConstrutorPrefabs.CaminhoParticulas("Brilho"), efeitos.transform);
        efeitos.prefabLama = AssetDatabase.LoadAssetAtPath<SpriteRenderer>(ConstrutorPrefabs.CaminhoLama);
        efeitos.prefabTexto = AssetDatabase.LoadAssetAtPath<TextoFlutuante>(ConstrutorPrefabs.CaminhoTexto);

        var visuais = new GameObject("Visuais").AddComponent<GerenteVisuais>();
        visuais.jogo = jogo;
        visuais.prefabPassaro = AssetDatabase.LoadAssetAtPath<VisualPassaro>(ConstrutorPrefabs.CaminhoPassaro);
        visuais.prefabPorco = AssetDatabase.LoadAssetAtPath<VisualPorco>(ConstrutorPrefabs.CaminhoPorco);
        visuais.prefabProjetil = AssetDatabase.LoadAssetAtPath<VisualProjetil>(ConstrutorPrefabs.CaminhoProjetil);
        visuais.efeitos = efeitos;

        var controles = new GameObject("Controles").AddComponent<Controles>();
        controles.jogo = jogo;
        controles.cam = camera;
        var fantasma = new GameObject("FantasmaColocacao").AddComponent<FantasmaColocacao>();
        fantasma.transform.SetParent(controles.transform, false);
        fantasma.jogo = jogo;
        fantasma.sprite = ConstrutorPrefabs.Sprite(fantasma.transform, "Fantasma", null, 40);
        fantasma.circulo = ConstrutorPrefabs.Circulo(fantasma.transform, "CirculoFantasma");
        controles.fantasma = fantasma;

        ConstrutorUI.CriarHud(null, jogo, controles, jogo.porcos, efeitos, mapa, revelar);

        EditorSceneManager.SaveScene(cena, CenaJogo);
    }

    static void CriarCenaMenu()
    {
        var cena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CriarCamera(PosicaoCameraMenu);
        CriarSons();
        var dadosMapa = ConstrutorDados.Fazenda();
        CriarMapa(dadosMapa);

        // Pássaros de enfeite voando pela trilha, atrás do véu do título.
        var enfeite = new GameObject("PassarosDecorativos").AddComponent<PassarosDecorativos>();
        enfeite.mapa = dadosMapa;
        enfeite.sombra = ConstrutorAssets.Sprite("sombra");
        enfeite.sprites = new[]
        {
            ConstrutorAssets.Sprite("pardal"), ConstrutorAssets.Sprite("sabia"), ConstrutorAssets.Sprite("arara"),
            ConstrutorAssets.Sprite("beija_flor"), ConstrutorAssets.Sprite("pata"), ConstrutorAssets.Sprite("urubu"),
        };

        ConstrutorUI.CriarMenu(null);
        ConstrutorUI.SistemaDeEventos(null);
        EditorSceneManager.SaveScene(cena, CenaMenu);
    }

    // Com "jogo" preenchido (cena Jogo), o mapa recebe os mapas das fases: na
    // troca de fase, a TrocaDeFase pede para ele redesenhar (VisualMapa.MostrarFase).
    static VisualMapa CriarMapa(Mapa dados, Jogo jogo = null)
    {
        var mapa = new GameObject("Mapa").AddComponent<VisualMapa>();
        mapa.mapa = dados;
        mapa.grama = ConstrutorAssets.Sprite("grama");
        mapa.arvore = ConstrutorAssets.Sprite("arvore");
        mapa.pedra = ConstrutorAssets.Sprite("pedra");
        mapa.lago = ConstrutorAssets.Sprite("lago");
        mapa.torre = ConstrutorAssets.Sprite("torre");
        mapa.flores = new[] { ConstrutorAssets.Sprite("flor_1"), ConstrutorAssets.Sprite("flor_2") };
        mapa.materialTerra = ConstrutorAssets.MaterialSprite("Terra", ConstrutorAssets.Textura("terra"));
        mapa.materialBorda = ConstrutorAssets.MaterialSprite("TerraBorda", ConstrutorAssets.Textura("terra_borda"));
        if (jogo != null && jogo.fases != null) mapa.fases = jogo.fases.mapas.ToArray();
        mapa.Desenhar();
        return mapa;
    }

    static Sons CriarSons()
    {
        var s = new GameObject("Sons").AddComponent<Sons>();
        s.estouros = new[] { ConstrutorAssets.Som("estouro_1"), ConstrutorAssets.Som("estouro_2"), ConstrutorAssets.Som("estouro_3") };
        s.tiroEstilingue = ConstrutorAssets.Som("tiro_estilingue");
        s.tiroArpao = ConstrutorAssets.Som("tiro_arpao");
        s.tiroBomba = ConstrutorAssets.Som("tiro_bomba");
        s.explosao = ConstrutorAssets.Som("explosao");
        s.compra = ConstrutorAssets.Som("compra");
        s.upgrade = ConstrutorAssets.Som("upgrade");
        s.venda = ConstrutorAssets.Som("venda");
        s.erro = ConstrutorAssets.Som("erro");
        s.clique = ConstrutorAssets.Som("clique");
        s.onda = ConstrutorAssets.Som("onda");
        s.chefe = ConstrutorAssets.Som("chefe");
        s.torre = ConstrutorAssets.Som("torre");
        s.vitoria = ConstrutorAssets.Som("vitoria");
        s.derrota = ConstrutorAssets.Som("derrota");
        s.fase = ConstrutorAssets.Som("fase");
        s.ambiente = ConstrutorAssets.Som("ambiente");
        s.musica = ConstrutorAssets.Som("musica");
        return s;
    }

    // Põe na cena uma instância do prefab (ligada a ele) e devolve o componente.
    static T Instanciar<T>(string caminho, Transform pai) where T : Component
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(caminho);
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, pai);
        return go.GetComponent<T>();
    }

    static Camera CriarCamera(Vector3 posicao)
    {
        var go = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        go.tag = "MainCamera";
        go.transform.position = posicao;
        var cam = go.GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 7.5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.18f, 0.29f, 0.14f);
        // Visão de cima: quem está mais para cima na tela fica "atrás". O Unity
        // ordena os sprites pelo eixo Y quando a câmera usa este modo.
        cam.transparencySortMode = TransparencySortMode.CustomAxis;
        cam.transparencySortAxis = Vector3.up;
        return cam;
    }

    public static void ConfigurarBuild()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(CenaMenu, true), // o jogo abre no menu
            new EditorBuildSettingsScene(CenaJogo, true),
        };
    }
}
