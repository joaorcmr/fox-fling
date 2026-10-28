using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Monta o jogo inteiro a partir do código: configurações, assets, prefabs e cenas.
// Menu "Fox Fling > Reconstruir tudo". As cenas geradas são sobrescritas;
// os materiais (Assets/FoxFling/Materiais) mantêm os números que você ajustou.
public static class ConstrutorFoxFling
{
    [MenuItem("Fox Fling/Reconstruir tudo")]
    static void ReconstruirTudoPeloMenu()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        ReconstruirTudo();
    }

    public static string ReconstruirTudo()
    {
        ConfigurarProjeto();

        // Começa numa cena vazia: os prefabs são montados nela e depois apagados.
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        ConstrutorAssets.GarantirPasta(ConstrutorAssets.PastaMateriais);
        ConstrutorAssets.ImportarArte();
        ConstrutorPrefabs.Tudo();
        ConstrutorCenas.Tudo();
        AssetDatabase.SaveAssets();

        EditorSceneManager.OpenScene(ConstrutorCenas.CenaMenu);
        ConfigurarGameView();
        return "Fox Fling reconstruído: " + DefinicoesFases.Todas().Length + " fases";
    }

    [MenuItem("Fox Fling/Build WebGL (itch.io)")]
    public static void BuildWebGL()
    {
        ConstrutorCenas.ConfigurarBuild();
        ConfigurarProjeto();
        // O itch.io não envia o header Content-Encoding para arquivos .gz;
        // o fallback embute um descompactador em JS e o jogo abre em qualquer servidor.
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;

        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);

        const string pasta = "Builds/WebGL";
        var cenas = new System.Collections.Generic.List<string>();
        foreach (var c in EditorBuildSettings.scenes)
            if (c.enabled) cenas.Add(c.path);

        var relatorio = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = cenas.ToArray(),
            locationPathName = pasta,
            target = BuildTarget.WebGL,
            targetGroup = BuildTargetGroup.WebGL,
            options = BuildOptions.None,
        });

        var s = relatorio.summary;
        Debug.Log($"[Fox Fling] Build WebGL: {s.result} | {s.totalSize / (1024f * 1024f):F1} MB | " +
                  $"{s.totalTime:mm\\:ss} | erros={s.totalErrors} | pasta={pasta}");
    }

    static void ConfigurarProjeto()
    {
        PlayerSettings.productName = "Fox Fling";
        PlayerSettings.companyName = "joaorcmr";
        PlayerSettings.defaultScreenWidth = 600;
        PlayerSettings.defaultScreenHeight = 480;
        PlayerSettings.defaultWebScreenWidth = 600;
        PlayerSettings.defaultWebScreenHeight = 480;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.resizableWindow = false;
        EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;

        // Mais iterações do solver = pilhas de blocos mais estáveis (custa um pouco de CPU).
        Physics2D.velocityIterations = 10;
        Physics2D.positionIterations = 6;
    }

    // Cria (uma vez) o tamanho 600×480 na Game view e seleciona. Usa API interna do
    // Unity via reflexão; se mudar numa versão futura, só não faz nada.
    static void ConfigurarGameView()
    {
        try
        {
            var asm = typeof(Editor).Assembly;
            var tipoSizes = asm.GetType("UnityEditor.GameViewSizes");
            var tipoSingleton = typeof(ScriptableSingleton<>).MakeGenericType(tipoSizes);
            var sizes = tipoSingleton.GetProperty("instance").GetValue(null);
            var grupoAtual = tipoSizes.GetProperty("currentGroupType").GetValue(sizes);
            var grupo = tipoSizes.GetMethod("GetGroup").Invoke(sizes, new[] { grupoAtual });

            const string nome = "Fox Fling 600x480";
            var textos = (string[])grupo.GetType().GetMethod("GetDisplayTexts").Invoke(grupo, null);
            int indice = Array.FindIndex(textos, t => t.StartsWith(nome));
            if (indice < 0)
            {
                var tipoSize = asm.GetType("UnityEditor.GameViewSize");
                var tipoEnum = asm.GetType("UnityEditor.GameViewSizeType");
                var ctor = tipoSize.GetConstructor(new[] { tipoEnum, typeof(int), typeof(int), typeof(string) });
                var novo = ctor.Invoke(new[] { Enum.Parse(tipoEnum, "FixedResolution"), 600, 480, nome });
                grupo.GetType().GetMethod("AddCustomSize").Invoke(grupo, new[] { novo });
                textos = (string[])grupo.GetType().GetMethod("GetDisplayTexts").Invoke(grupo, null);
                indice = Array.FindIndex(textos, t => t.StartsWith(nome));
            }

            var tipoGameView = asm.GetType("UnityEditor.GameView");
            foreach (var janela in Resources.FindObjectsOfTypeAll(tipoGameView))
            {
                var prop = tipoGameView.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                prop?.SetValue(janela, indice);
            }
        }
        catch (Exception e)
        {
            Debug.Log("Fox Fling: não consegui configurar a Game view para 600x480 (" + e.GetType().Name + ").");
        }
    }
}
