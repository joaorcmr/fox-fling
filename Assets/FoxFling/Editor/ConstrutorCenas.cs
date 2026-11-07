using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static ConstrutorAssets;

// Transforma cada DefinicaoFase numa cena (Fase1..Fase5) com o prefab Sistemas,
// o GerenciadorFase e as peças instanciadas a partir dos prefabs.
public static class ConstrutorCenas
{
    public const string Pasta = Raiz + "/Scenes";
    public const string CenaMenu = Pasta + "/Menu.unity";

    public static string CaminhoFase(int n) => $"{Pasta}/Fase{n}.unity";

    public static void Tudo()
    {
        GarantirPasta(Pasta);
        CriarMenu();
        foreach (var def in DefinicoesFases.Todas())
            CriarFase(def);
        ConfigurarBuild();
    }

    static void CriarMenu()
    {
        var cena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var raiz = new GameObject("Cenario").transform;
        ConstrutorPrefabs.CriarCenario(raiz);
        ConstrutorPrefabs.CriarSons(raiz);

        // Decoração viva: raposas pulando na "fila" e tubarões esperando.
        var decoracao = new GameObject("Decoracao").transform;
        Raposa[] raposas = { ConstrutorPrefabs.RaposaNormal, ConstrutorPrefabs.RaposaNinja, ConstrutorPrefabs.RaposaBomba };
        for (int i = 0; i < raposas.Length; i++)
        {
            if (raposas[i] == null) continue;
            var r = (Raposa)PrefabUtility.InstantiatePrefab(raposas[i], decoracao);
            r.transform.position = new Vector3(-6.3f + i * 0.75f, DefinicoesFases.Chao + 0.33f, 0f);
        }
        for (int i = 0; i < 2; i++)
        {
            var inimigo = (GameObject)PrefabUtility.InstantiatePrefab(i == 0 ? ConstrutorPrefabs.InimigoGrande : ConstrutorPrefabs.Inimigo, decoracao);
            float altura = DefinicoesFases.TamanhoInimigo.y * (i == 0 ? DefinicoesFases.EscalaGrande : 1f);
            inimigo.transform.position = new Vector3(5.2f + i * 1.1f, DefinicoesFases.Chao + altura / 2f, 0f);
        }

        ConstrutorUI.CriarMenu(raiz, DefinicoesFases.Todas());
        EditorSceneManager.SaveScene(cena, CenaMenu);
    }

    static void CriarFase(DefinicaoFase def)
    {
        var cena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var sistemas = (GameObject)PrefabUtility.InstantiatePrefab(ConstrutorPrefabs.Sistemas);

        var fase = new GameObject("Fase").AddComponent<GerenciadorFase>();
        fase.numeroFase = def.numero;
        fase.nomeFase = def.nome;
        fase.dificuldade = def.dificuldade;
        fase.atacanteNormal = ConstrutorPrefabs.AtacanteNormal;
        fase.atacanteNinja = ConstrutorPrefabs.AtacanteNinja;
        fase.atacanteBomba = ConstrutorPrefabs.AtacanteBomba;
        fase.atacanteGrande = ConstrutorPrefabs.AtacanteGrande;
        fase.hud = sistemas.GetComponentInChildren<Hud>();

        var estrutura = new GameObject("Estrutura").transform;
        foreach (var peca in def.pecas)
            CriarPeca(peca, estrutura);

        EditorSceneManager.SaveScene(cena, CaminhoFase(def.numero));
    }

    static void CriarPeca(Peca peca, Transform pai)
    {
        GameObject prefab = peca.tipo == TipoPeca.Bloco ? ConstrutorPrefabs.Bloco
            : peca.tipo == TipoPeca.InimigoGrande ? ConstrutorPrefabs.InimigoGrande
            : ConstrutorPrefabs.Inimigo;

        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, pai);
        go.transform.position = peca.centro;
        PrefabUtility.RecordPrefabInstancePropertyModifications(go.transform);

        if (peca.tipo != TipoPeca.Bloco) return;

        go.name = $"{peca.material} {peca.tamanho.x:0.##}x{peca.tamanho.y:0.##}";
        var bloco = go.GetComponent<Bloco>();
        bloco.Configurar(MaterialDeBloco(peca.material), peca.tamanho);

        // Mudanças feitas por código numa instância de prefab precisam ser
        // "registradas", senão o Unity volta para os valores do prefab ao salvar.
        PrefabUtility.RecordPrefabInstancePropertyModifications(bloco);
        PrefabUtility.RecordPrefabInstancePropertyModifications(go.GetComponent<SpriteRenderer>());
        PrefabUtility.RecordPrefabInstancePropertyModifications(go.GetComponent<BoxCollider2D>());
        PrefabUtility.RecordPrefabInstancePropertyModifications(go.GetComponent<Rigidbody2D>());
    }

    public static void ConfigurarBuild()
    {
        var cenas = new List<EditorBuildSettingsScene>();
        if (System.IO.File.Exists(CenaMenu)) cenas.Add(new EditorBuildSettingsScene(CenaMenu, true));
        foreach (var def in DefinicoesFases.Todas())
            cenas.Add(new EditorBuildSettingsScene(CaminhoFase(def.numero), true));
        EditorBuildSettings.scenes = cenas.ToArray();
    }
}
