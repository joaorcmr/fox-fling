using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Monta o Fox Tower inteiro a partir do código: configurações, dados, prefabs e
// cenas. Menu "Fox Tower" no Editor. Cada task do plano acrescenta uma etapa em
// ReconstruirTudo.
public static class ConstrutorRevoada
{
    public const int Largura = 600, Altura = 480; // regra da disciplina

    [MenuItem(Identidade.NomeDoJogo + "/Reconstruir tudo")]
    static void ReconstruirTudoPeloMenu()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Debug.Log(ReconstruirTudo());
    }

    // Gera tudo a partir do código. As cenas são sobrescritas; os números dos
    // assets de dados que já existem são mantidos.
    public static string ReconstruirTudo()
    {
        ConfigurarProjeto();
        ConstrutorAssets.ImportarArte();
        ConstrutorAssets.ImportarAudio();
        ConstrutorDados.GarantirDados(false);
        ConstrutorPrefabs.Tudo();
        ConstrutorCenas.Tudo();
        AssetDatabase.SaveAssets();
        var d = ConstrutorDados.Carregar();
        return $"{Identidade.NomeDoJogo} reconstruído: {d.passaros.Count} pássaros, {d.porcos.Count} porcos, {d.ondas.Count} ondas";
    }

    [MenuItem(Identidade.NomeDoJogo + "/Balanceamento (robôs)")]
    static void Balanceamento() => Debug.Log(Robos.Relatorio(ConstrutorDados.Carregar()));

    // No Play: o robô Equilibrado joga até a onda 18 (a revoada densa), para ver de perto.
    [MenuItem(Identidade.NomeDoJogo + "/Pular para a onda 18 (robô)")]
    static void PularParaOnda18()
    {
        if (!Application.isPlaying || Jogo.Atual == null)
        {
            Debug.Log(Identidade.NomeDoJogo + ": dê Play primeiro.");
            return;
        }
        Robos.AvancarAteOnda(Jogo.Atual.Sim, 18);
    }

    // No Play: o robô Equilibrado joga a fase 1 inteira, até a troca de mapa
    // (para ver de perto os porcos virando moedas, o quadro e o caminho novo).
    [MenuItem(Identidade.NomeDoJogo + "/Pular para a troca de fase (robô)")]
    static void PularParaTrocaDeFase()
    {
        if (!Application.isPlaying || Jogo.Atual == null)
        {
            Debug.Log(Identidade.NomeDoJogo + ": dê Play primeiro.");
            return;
        }
        var sim = Jogo.Atual.Sim;
        if (sim.Fase >= sim.TotalFases)
        {
            Debug.Log(Identidade.NomeDoJogo + ": já está na última fase.");
            return;
        }
        Robos.AvancarAteFase(sim, sim.Fase + 1);
    }

    // O jogo para navegador, em Builds/WebGL (precisa do módulo Web Build
    // Support). Pelo terminal: unity build --target WebGL --execute-method ConstrutorRevoada.BuildWebGL
    [MenuItem(Identidade.NomeDoJogo + "/Build WebGL (itch.io)")]
    public static void BuildWebGL()
    {
        ConstrutorCenas.ConfigurarBuild();
        ConfigurarProjeto();
        // O itch.io não envia o header Content-Encoding para arquivos .gz; o
        // fallback embute um descompactador em JS e o jogo abre em qualquer servidor.
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
        Debug.Log($"[{Identidade.NomeDoJogo}] Build WebGL: {s.result} | {s.totalSize / (1024f * 1024f):F1} MB | " +
                  $"{s.totalTime:mm\\:ss} | erros={s.totalErrors} | pasta={pasta}");
        if (Application.isBatchMode && s.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) EditorApplication.Exit(1);
    }

    [MenuItem(Identidade.NomeDoJogo + "/Restaurar números padrão")]
    static void RestaurarNumerosPadrao()
    {
        ConstrutorDados.GarantirDados(true);
        Debug.Log(Identidade.NomeDoJogo + ": números padrão restaurados (as pastas Dados de cada assunto)");
    }

    public static void ConfigurarProjeto()
    {
        PlayerSettings.productName = Identidade.NomeDoJogo;
        PlayerSettings.companyName = "joaorcmr";
        PlayerSettings.defaultScreenWidth = Largura;
        PlayerSettings.defaultScreenHeight = Altura;
        PlayerSettings.defaultWebScreenWidth = Largura;
        PlayerSettings.defaultWebScreenHeight = Altura;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.resizableWindow = false;
        EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;

        // A simulação anda em passos de 1/60 s; o FixedUpdate do Unity no mesmo
        // ritmo faz cada FixedUpdate ser exatamente um passo do jogo. O Unity 6
        // guarda o passo fixo como fração (m_Count / m_Rate) no TimeManager;
        // mudar Time.fixedDeltaTime fora do Play não chega ao arquivo, então a
        // fração é escrita direto (141120000 / 60 = 2352000).
        var tempo = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TimeManager.asset");
        if (tempo.Length > 0)
        {
            var so = new SerializedObject(tempo[0]);
            var contagem = so.FindProperty("Fixed Timestep.m_Count");
            var numerador = so.FindProperty("Fixed Timestep.m_Rate.m_Numerator");
            var denominador = so.FindProperty("Fixed Timestep.m_Rate.m_Denominator");
            if (contagem != null && numerador != null && denominador != null)
            {
                numerador.longValue = 141120000;
                denominador.longValue = 1;
                contagem.longValue = 141120000 / 60;
                so.ApplyModifiedPropertiesWithoutUndo();
                // Configurações do projeto só vão para o disco se marcadas como sujas.
                EditorUtility.SetDirty(tempo[0]);
                AssetDatabase.SaveAssetIfDirty(tempo[0]);
            }
        }

        ConfigurarGameView();
    }

    // Cria (uma vez) o tamanho 600×480 na Game view e seleciona. Usa API interna
    // do Unity via reflexão; se mudar numa versão futura, só não faz nada.
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

            const string nome = Identidade.NomeDoJogo + " 600x480";
            var textos = (string[])grupo.GetType().GetMethod("GetDisplayTexts").Invoke(grupo, null);
            int indice = Array.FindIndex(textos, t => t.StartsWith(nome));
            if (indice < 0)
            {
                var tipoSize = asm.GetType("UnityEditor.GameViewSize");
                var tipoEnum = asm.GetType("UnityEditor.GameViewSizeType");
                var ctor = tipoSize.GetConstructor(new[] { tipoEnum, typeof(int), typeof(int), typeof(string) });
                var novo = ctor.Invoke(new[] { Enum.Parse(tipoEnum, "FixedResolution"), Largura, Altura, nome });
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
            Debug.Log(Identidade.NomeDoJogo + ": não consegui configurar a Game view para 600x480 (" + e.GetType().Name + ").");
        }
    }
}
