using UnityEditor;
using UnityEngine;

// Monta os prefabs por código (Assets/Revoada/Prefabs).
public static class ConstrutorPrefabs
{
    public const string Pasta = ConstrutorAssets.Raiz + "/Prefabs";
    public const string CaminhoPassaro = Pasta + "/Passaro.prefab";

    public static void Tudo()
    {
        ConstrutorAssets.GarantirPasta(Pasta);
        CriarPassaro();
    }

    static void CriarPassaro()
    {
        var raiz = new GameObject("Passaro");
        var v = raiz.AddComponent<VisualPassaro>();
        v.sombra = Sprite(raiz.transform, "Sombra", ConstrutorAssets.Sprite("sombra"), -3);
        v.corpo = Sprite(raiz.transform, "Corpo", null, 0);
        v.flash = Sprite(v.corpo.transform, "Flash", null, 1);
        v.flash.sharedMaterial = ConstrutorAssets.MaterialFlash();
        v.flash.color = new Color(1f, 1f, 1f, 0f);

        var barra = new GameObject("Barra").transform;
        barra.SetParent(raiz.transform, false);
        v.barra = barra;
        var fundo = Sprite(barra, "Fundo", ConstrutorAssets.Sprite("barra"), 30);
        fundo.color = new Color(0.17f, 0.11f, 0.08f, 0.9f);
        fundo.transform.localScale = new Vector3((VisualPassaro.LarguraBarra + 0.06f) / 0.125f, 0.14f / 0.125f, 1f);
        v.barraVida = Sprite(barra, "Vida", ConstrutorAssets.Sprite("barra"), 31);
        v.barraVida.color = new Color(0.47f, 0.86f, 0.32f);
        barra.gameObject.SetActive(false);

        PrefabUtility.SaveAsPrefabAsset(raiz, CaminhoPassaro);
        Object.DestroyImmediate(raiz);
    }

    public static SpriteRenderer Sprite(Transform pai, string nome, Sprite sprite, int ordem)
    {
        var go = new GameObject(nome);
        go.transform.SetParent(pai, false);
        var r = go.AddComponent<SpriteRenderer>();
        r.sprite = sprite;
        r.sortingOrder = ordem;
        r.spriteSortPoint = SpriteSortPoint.Pivot;
        return r;
    }
}
