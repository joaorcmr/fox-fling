using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// Monta o Revoada inteiro a partir do código: configurações, dados, prefabs e
// cenas. Menu "Revoada" no Editor. Cada task do plano acrescenta uma etapa em
// ReconstruirTudo.
public static class ConstrutorRevoada
{
    public const int Largura = 600, Altura = 480; // regra da disciplina

    [MenuItem("Revoada/Reconstruir tudo")]
    static void ReconstruirTudoPeloMenu() => Debug.Log(ReconstruirTudo());

    public static string ReconstruirTudo()
    {
        ConfigurarProjeto();
        return "Revoada: projeto configurado";
    }

    public static void ConfigurarProjeto()
    {
        PlayerSettings.productName = "Revoada";
        PlayerSettings.companyName = "joaorcmr";
        PlayerSettings.defaultScreenWidth = Largura;
        PlayerSettings.defaultScreenHeight = Altura;
        PlayerSettings.defaultWebScreenWidth = Largura;
        PlayerSettings.defaultWebScreenHeight = Altura;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.resizableWindow = false;
        EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;

        // A simulação anda em passos de 1/60 s; o FixedUpdate do Unity no mesmo
        // ritmo faz cada FixedUpdate ser exatamente um passo do jogo.
        Time.fixedDeltaTime = 1f / 60f;

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

            const string nome = "Revoada 600x480";
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
            Debug.Log("Revoada: não consegui configurar a Game view para 600x480 (" + e.GetType().Name + ").");
        }
    }
}
