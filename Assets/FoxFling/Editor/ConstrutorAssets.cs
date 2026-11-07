using System.IO;
using UnityEditor;
using UnityEngine;

// Importação de sprites/sons e criação dos materiais (físicos, de bloco e de render).
// Os NÚMEROS dos materiais só são escritos quando o asset é criado: depois disso
// você pode ajustá-los no Inspector e "Reconstruir tudo" não desfaz.
public static class ConstrutorAssets
{
    public const string Raiz = "Assets/FoxFling";
    public const string PastaArte = Raiz + "/Art";
    public const string PastaAudio = Raiz + "/Audio";
    public const string PastaMateriais = Raiz + "/Materiais";
    public const int PixelsPorUnidade = 80;

    static readonly string[] SpritesDeBloco =
    {
        "madeira", "madeira_rachada", "madeira_v", "madeira_v_rachada",
        "vidro", "vidro_rachado", "rocha", "rocha_rachada",
    };

    public static Sprite Sprite(string nome) => AssetDatabase.LoadAssetAtPath<Sprite>($"{PastaArte}/{nome}.png");

    public static Texture2D Textura(string nome) => AssetDatabase.LoadAssetAtPath<Texture2D>($"{PastaArte}/{nome}.png");

    public static AudioClip Som(string nome) => AssetDatabase.LoadAssetAtPath<AudioClip>($"{PastaAudio}/{nome}.wav");

    public static void ImportarArte()
    {
        foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { PastaArte }))
        {
            string caminho = AssetDatabase.GUIDToAssetPath(guid);
            string nome = Path.GetFileNameWithoutExtension(caminho);
            var imp = (TextureImporter)AssetImporter.GetAtPath(caminho);

            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;

            var s = new TextureImporterSettings();
            imp.ReadTextureSettings(s);
            // FullRect é obrigatório para o modo Tiled dos blocos funcionar.
            s.spriteMeshType = SpriteMeshType.FullRect;
            s.spriteAlignment = (int)SpriteAlignment.Center;
            s.spritePivot = new Vector2(0.5f, 0.5f);
            if (nome.StartsWith("raposa_"))
            {
                // Pivô no centro da bola (no meio da largura, 28 px acima do pé da imagem),
                // não no centro da imagem. A bomba é mais alta (72 px) por causa do pavio.
                imp.GetSourceTextureWidthAndHeight(out _, out int altura);
                s.spriteAlignment = (int)SpriteAlignment.Custom;
                s.spritePivot = new Vector2(0.5f, 28f / altura);
            }
            else if (nome == "estilingue") s.spriteAlignment = (int)SpriteAlignment.BottomCenter;
            else if (nome == "chao") s.spriteAlignment = (int)SpriteAlignment.TopCenter;
            imp.SetTextureSettings(s);

            imp.spritePixelsPerUnit = PixelsPorUnidade;
            imp.mipmapEnabled = false;
            imp.filterMode = FilterMode.Bilinear;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.alphaIsTransparency = true;
            imp.wrapMode = TextureWrapMode.Clamp;

            Vector4 borda = Vector4.zero; // esquerda, baixo, direita, cima (em pixels)
            if (System.Array.IndexOf(SpritesDeBloco, nome) >= 0) borda = new Vector4(8, 8, 8, 8);
            else if (nome == "painel") borda = new Vector4(22, 22, 22, 22);
            else if (nome == "botao") borda = new Vector4(16, 16, 16, 16);
            imp.spriteBorder = borda;

            imp.SaveAndReimport();
        }
    }

    public static PhysicsMaterial2D MaterialFisico(string nome, float atrito, float quique)
    {
        string caminho = $"{PastaMateriais}/Fisica_{nome}.physicsMaterial2D";
        var m = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(caminho);
        if (m == null)
        {
            m = new PhysicsMaterial2D(nome) { friction = atrito, bounciness = quique };
            AssetDatabase.CreateAsset(m, caminho);
        }
        return m;
    }

    public static MaterialBloco MaterialDeBloco(TipoMaterial tipo)
    {
        string caminho = $"{PastaMateriais}/{tipo}.asset";
        var m = AssetDatabase.LoadAssetAtPath<MaterialBloco>(caminho);
        if (m == null)
        {
            m = ScriptableObject.CreateInstance<MaterialBloco>();
            m.nome = tipo.ToString();
            switch (tipo)
            {
                case TipoMaterial.Madeira:
                    m.densidade = 1f; m.vida = 50f; m.pontos = 500;
                    m.fisica = MaterialFisico("Madeira", 0.7f, 0.05f);
                    m.corDetritos = new Color(0.79f, 0.54f, 0.26f);
                    break;
                case TipoMaterial.Vidro:
                    // Leve e frágil: quebra com pouco, e a raposa passa reto.
                    m.densidade = 0.6f; m.vida = 15f; m.pontos = 300;
                    m.fisica = MaterialFisico("Vidro", 0.25f, 0.05f);
                    m.corDetritos = new Color(0.75f, 0.93f, 1f, 0.9f);
                    break;
                default:
                    // Pesada e resistente: mais difícil de quebrar, mas esmaga o que estiver embaixo.
                    m.densidade = 3f; m.vida = 160f; m.pontos = 1000;
                    m.fisica = MaterialFisico("Rocha", 0.9f, 0f);
                    m.corDetritos = new Color(0.56f, 0.58f, 0.61f);
                    break;
            }
            AssetDatabase.CreateAsset(m, caminho);
        }

        // Referências (sprites e sons) sempre atualizadas.
        string n = tipo.ToString().ToLowerInvariant();
        m.sprite = Sprite(n);
        m.spriteRachado = Sprite(tipo == TipoMaterial.Madeira ? "madeira_rachada" : tipo == TipoMaterial.Vidro ? "vidro_rachado" : "rocha_rachada");
        m.spriteVertical = tipo == TipoMaterial.Madeira ? Sprite("madeira_v") : null;
        m.spriteVerticalRachado = tipo == TipoMaterial.Madeira ? Sprite("madeira_v_rachada") : null;
        m.somImpacto = Som("impacto_" + n);
        m.somQuebra = Som("quebra_" + n);
        EditorUtility.SetDirty(m);
        return m;
    }

    // Material de render para partículas, rastro e elástico (shader de sprite comum).
    public static Material MaterialRender(string nome, Texture2D textura)
    {
        string caminho = $"{PastaMateriais}/Render_{nome}.mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(caminho);
        if (m == null)
        {
            m = new Material(Shader.Find("Sprites/Default"));
            AssetDatabase.CreateAsset(m, caminho);
        }
        m.mainTexture = textura;
        EditorUtility.SetDirty(m);
        return m;
    }

    // ---- Loja (Assets/FoxFling/Loja) ----
    // Cada item e o catálogo são assets. Como nos materiais de bloco, os NÚMEROS (preço etc.)
    // só são escritos quando o asset é criado: depois você pode ajustá-los no Inspector e
    // "Reconstruir tudo" não desfaz. A lista de itens do catálogo é sempre reescrita.
    public const string PastaLoja = Raiz + "/Loja";

    public static CatalogoLoja CriarCatalogo()
    {
        GarantirPasta(PastaLoja);

        // O prefab do bloco existe depois de "Reconstruir tudo" (ConstrutorPrefabs.Bloco) ou já está em disco.
        var prefabBloco = ConstrutorPrefabs.Bloco != null
            ? ConstrutorPrefabs.Bloco
            : AssetDatabase.LoadAssetAtPath<GameObject>($"{ConstrutorPrefabs.Pasta}/Bloco.prefab");
        var madeira = MaterialDeBloco(TipoMaterial.Madeira);
        var rocha = MaterialDeBloco(TipoMaterial.Rocha);

        var reparo = Item("Reparo", "Reparar", "Recupera a vida de todos os blocos e tubarões da torre.", 250, TipoItem.Reparo);
        var muroMadeira = Item("MuroMadeira", "Muro madeira", "Muro de madeira em pé, barato.", 150, TipoItem.Muro,
            prefabBloco, madeira, new Vector2(0.4f, 1.2f));
        var muroRocha = Item("MuroRocha", "Muro rocha", "Muro de rocha: bem mais resistente, e esmaga quem cair.", 450, TipoItem.Muro,
            prefabBloco, rocha, new Vector2(0.4f, 1.4f));
        var barricada = Item("Barricada", "Barricada", "Bloco baixo e largo de madeira.", 200, TipoItem.Muro,
            prefabBloco, madeira, new Vector2(1.2f, 0.6f));

        string caminho = $"{PastaLoja}/Catalogo.asset";
        var catalogo = AssetDatabase.LoadAssetAtPath<CatalogoLoja>(caminho);
        if (catalogo == null)
        {
            catalogo = ScriptableObject.CreateInstance<CatalogoLoja>();
            AssetDatabase.CreateAsset(catalogo, caminho);
        }
        catalogo.itens = new[] { reparo, muroMadeira, muroRocha, barricada };
        EditorUtility.SetDirty(catalogo);
        AssetDatabase.SaveAssets();
        return catalogo;
    }

    // nome, descrição, preço, tipo e tamanho só na criação; prefab e material (referências) sempre.
    static ItemLoja Item(string arquivo, string nome, string descricao, int preco, TipoItem tipo,
        GameObject prefab = null, MaterialBloco material = null, Vector2? tamanho = null)
    {
        string caminho = $"{PastaLoja}/Item_{arquivo}.asset";
        var item = AssetDatabase.LoadAssetAtPath<ItemLoja>(caminho);
        if (item == null)
        {
            item = ScriptableObject.CreateInstance<ItemLoja>();
            item.nome = nome;
            item.descricao = descricao;
            item.preco = preco;
            item.tipo = tipo;
            if (tamanho.HasValue) item.tamanho = tamanho.Value;
            AssetDatabase.CreateAsset(item, caminho);
        }
        item.prefab = prefab;
        item.material = material;
        EditorUtility.SetDirty(item);
        return item;
    }

    public static void GarantirPasta(string caminho)
    {
        if (AssetDatabase.IsValidFolder(caminho)) return;
        string pai = Path.GetDirectoryName(caminho).Replace('\\', '/');
        GarantirPasta(pai);
        AssetDatabase.CreateFolder(pai, Path.GetFileName(caminho));
    }
}
