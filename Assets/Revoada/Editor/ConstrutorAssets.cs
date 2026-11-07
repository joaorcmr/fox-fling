using System.IO;
using UnityEditor;
using UnityEngine;

// Importação dos sprites e sons e criação dos materiais de render.
public static class ConstrutorAssets
{
    public const string Raiz = "Assets/Revoada";
    public const string PastaArte = Raiz + "/Arte";
    public const string PastaAudio = Raiz + "/Audio";
    public const string PastaMateriais = Raiz + "/Materiais";
    public const int PixelsPorUnidade = 64; // a arte é desenhada em 2×: na tela, 1 unidade = 32 px

    // Ladrilhos que se repetem (grama em modo Tiled, terra ao longo da trilha).
    static readonly string[] Repetidos = { "grama", "terra", "terra_borda" };

    public static Sprite Sprite(string nome) => AssetDatabase.LoadAssetAtPath<Sprite>($"{PastaArte}/{nome}.png");
    public static Texture2D Textura(string nome) => AssetDatabase.LoadAssetAtPath<Texture2D>($"{PastaArte}/{nome}.png");
    public static AudioClip Som(string nome) => AssetDatabase.LoadAssetAtPath<AudioClip>($"{PastaAudio}/{nome}.wav");

    public static void ImportarArte()
    {
        foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { PastaArte }))
        {
            string caminho = AssetDatabase.GUIDToAssetPath(guid);
            if (caminho.Contains("/Fontes/")) continue; // os originais do Iran só servem de fonte para o gerador
            string nome = Path.GetFileNameWithoutExtension(caminho);
            var imp = (TextureImporter)AssetImporter.GetAtPath(caminho);

            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;

            var s = new TextureImporterSettings();
            imp.ReadTextureSettings(s);
            // FullRect: obrigatório para o modo Tiled da grama e simples para todo o resto.
            s.spriteMeshType = SpriteMeshType.FullRect;
            s.spriteAlignment = (int)SpriteAlignment.Center;
            s.spritePivot = new Vector2(0.5f, 0.5f);
            if (nome == "torre" || nome == "arvore") s.spriteAlignment = (int)SpriteAlignment.BottomCenter;
            else if (nome.StartsWith("arma_") && nome != "arma_tambor")
            {
                // As armas giram em volta de onde o porco segura: (12, 20) numa tela 64×40.
                s.spriteAlignment = (int)SpriteAlignment.Custom;
                s.spritePivot = new Vector2(12f / 64f, 0.5f);
            }
            imp.SetTextureSettings(s);

            imp.spritePixelsPerUnit = PixelsPorUnidade;
            imp.mipmapEnabled = false;
            imp.filterMode = FilterMode.Bilinear;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.alphaIsTransparency = true;
            imp.wrapMode = System.Array.IndexOf(Repetidos, nome) >= 0 ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;

            Vector4 borda = Vector4.zero; // esquerda, baixo, direita, cima (em pixels)
            if (nome == "painel") borda = new Vector4(22, 22, 22, 22);
            else if (nome == "botao") borda = new Vector4(16, 16, 16, 16);
            imp.spriteBorder = borda;

            imp.SaveAndReimport();
        }
    }

    // Material comum de sprite (para LineRenderer e partículas).
    public static Material MaterialSprite(string nome, Texture textura)
    {
        string caminho = $"{PastaMateriais}/Render_{nome}.mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(caminho);
        if (m == null)
        {
            GarantirPasta(PastaMateriais);
            m = new Material(Shader.Find("Sprites/Default"));
            AssetDatabase.CreateAsset(m, caminho);
        }
        m.mainTexture = textura;
        EditorUtility.SetDirty(m);
        return m;
    }

    // Material que desenha o sprite inteiro numa cor só (o "flash" branco quando
    // um pássaro leva um golpe). O shader de texto do Unity usa só o alfa da
    // textura e pinta com a cor do vértice: é um truque clássico para isso.
    public static Material MaterialFlash()
    {
        string caminho = $"{PastaMateriais}/Render_Flash.mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(caminho);
        if (m == null)
        {
            GarantirPasta(PastaMateriais);
            m = new Material(Shader.Find("GUI/Text Shader"));
            AssetDatabase.CreateAsset(m, caminho);
        }
        return m;
    }

    public static void GarantirPasta(string caminho)
    {
        if (AssetDatabase.IsValidFolder(caminho)) return;
        string pai = Path.GetDirectoryName(caminho).Replace('\\', '/');
        GarantirPasta(pai);
        AssetDatabase.CreateFolder(pai, Path.GetFileName(caminho));
    }
}
