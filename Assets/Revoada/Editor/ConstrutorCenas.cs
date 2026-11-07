using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Monta as cenas (Assets/Revoada/Cenas) e as Build Settings.
public static class ConstrutorCenas
{
    public const string Pasta = ConstrutorAssets.Raiz + "/Cenas";
    public const string CenaJogo = Pasta + "/Jogo.unity";

    // A câmera mostra x de 0 a 18,75 e y de 0 a 15: o mapa (x até 14) e, à
    // direita, o espaço da barra lateral.
    public static readonly Vector3 PosicaoCamera = new Vector3(9.375f, 7.5f, -10f);

    public static void Tudo()
    {
        ConstrutorAssets.GarantirPasta(Pasta);
        CriarCenaJogo();
        ConfigurarBuild();
    }

    static void CriarCenaJogo()
    {
        var cena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CriarCamera();

        var jogo = new GameObject("Jogo").AddComponent<Jogo>();
        jogo.passaros = ConstrutorDados.Passaros();
        jogo.porcos = ConstrutorDados.Porcos();
        jogo.ondas = ConstrutorDados.Ondas();
        jogo.economia = ConstrutorDados.Economia();
        jogo.mapa = ConstrutorDados.Fazenda();

        var mapa = new GameObject("Mapa").AddComponent<VisualMapa>();
        mapa.mapa = jogo.mapa;
        mapa.grama = ConstrutorAssets.Sprite("grama");
        mapa.arvore = ConstrutorAssets.Sprite("arvore");
        mapa.pedra = ConstrutorAssets.Sprite("pedra");
        mapa.lago = ConstrutorAssets.Sprite("lago");
        mapa.torre = ConstrutorAssets.Sprite("torre");
        mapa.flores = new[] { ConstrutorAssets.Sprite("flor_1"), ConstrutorAssets.Sprite("flor_2") };
        mapa.materialTerra = ConstrutorAssets.MaterialSprite("Terra", ConstrutorAssets.Textura("terra"));
        mapa.materialBorda = ConstrutorAssets.MaterialSprite("TerraBorda", ConstrutorAssets.Textura("terra_borda"));
        mapa.Desenhar();

        var visuais = new GameObject("Visuais").AddComponent<GerenteVisuais>();
        visuais.jogo = jogo;
        visuais.prefabPassaro = AssetDatabase.LoadAssetAtPath<VisualPassaro>(ConstrutorPrefabs.CaminhoPassaro);

        EditorSceneManager.SaveScene(cena, CenaJogo);
    }

    static Camera CriarCamera()
    {
        var go = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        go.tag = "MainCamera";
        go.transform.position = PosicaoCamera;
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
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(CenaJogo, true) };
    }
}
