using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

// Monta os prefabs por código (Assets/Revoada/Prefabs).
public static class ConstrutorPrefabs
{
    public const string Pasta = ConstrutorAssets.Raiz + "/Prefabs";
    public const string PastaEfeitos = Pasta + "/Efeitos";
    public const string CaminhoPassaro = Pasta + "/Passaro.prefab";
    public const string CaminhoPorco = Pasta + "/Porco.prefab";
    public const string CaminhoProjetil = Pasta + "/Projetil.prefab";
    public const string CaminhoTexto = Pasta + "/TextoFlutuante.prefab";
    public const string CaminhoLama = PastaEfeitos + "/Lama.prefab";

    public static string CaminhoParticulas(string nome) => $"{PastaEfeitos}/{nome}.prefab";

    public static void Tudo()
    {
        ConstrutorAssets.GarantirPasta(Pasta);
        ConstrutorAssets.GarantirPasta(PastaEfeitos);
        CriarPassaro();
        CriarPorco();
        CriarProjetil();
        CriarTexto();
        CriarLama();
        CriarParticulas("Penas", "pena", vida: (0.7f, 1.1f), velocidade: (1.5f, 3.4f), tamanho: (0.26f, 0.42f),
            gravidade: 0.35f, ordem: 20, giro: 3f, freio: 0.08f, crescer: 0.7f);
        CriarParticulas("Poeira", "poeira", vida: (0.4f, 0.7f), velocidade: (0.6f, 1.4f), tamanho: (0.35f, 0.6f),
            gravidade: 0f, ordem: 19, giro: 0.5f, freio: 0.15f, crescer: 1.5f);
        CriarParticulas("Brilho", "brilho", vida: (0.35f, 0.6f), velocidade: (0.5f, 1.6f), tamanho: (0.22f, 0.38f),
            gravidade: -0.1f, ordem: 21, giro: 4f, freio: 0.1f, crescer: 0f);
    }

    // O SortingGroup faz o pássaro (corpo, flash, sombra, barra) ser ordenado
    // como uma peça só pela posição Y, em vez de cada sprite separado.
    static void CriarPassaro()
    {
        var raiz = new GameObject("Passaro");
        var v = raiz.AddComponent<VisualPassaro>();
        raiz.AddComponent<SortingGroup>();
        v.sombra = Sprite(raiz.transform, "Sombra", ConstrutorAssets.Sprite("sombra"), -1);
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

        Salvar(raiz, CaminhoPassaro);
    }

    // A "Figura" (sombra, corpo, arma, acessório) fica num SortingGroup; o
    // círculo de alcance fica fora dele, no chão.
    static void CriarPorco()
    {
        var raiz = new GameObject("Porco");
        var v = raiz.AddComponent<VisualPorco>();
        var figura = new GameObject("Figura").transform;
        figura.SetParent(raiz.transform, false);
        figura.gameObject.AddComponent<SortingGroup>();

        var sombra = Sprite(figura, "Sombra", ConstrutorAssets.Sprite("sombra"), -1);
        sombra.transform.localPosition = new Vector3(0f, -0.45f, 0f);
        sombra.transform.localScale = Vector3.one * 0.95f;
        v.corpo = Sprite(figura, "Corpo", null, 0);
        v.pivoArma = new GameObject("PivoArma").transform;
        v.pivoArma.SetParent(figura, false);
        v.arma = Sprite(v.pivoArma, "Arma", null, 1);
        v.acessorio = Sprite(figura, "Acessorio", null, 3);
        v.alcance = Circulo(raiz.transform, "Alcance");

        Salvar(raiz, CaminhoPorco);
    }

    static void CriarProjetil()
    {
        var raiz = new GameObject("Projetil");
        var v = raiz.AddComponent<VisualProjetil>();
        v.sprite = Sprite(raiz.transform, "Sprite", null, 10);
        v.sombra = Sprite(raiz.transform, "Sombra", ConstrutorAssets.Sprite("sombra"), -1);
        v.sombra.transform.localScale = Vector3.one * 0.45f;
        Salvar(raiz, CaminhoProjetil);
    }

    static void CriarTexto()
    {
        var fonte = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var raiz = new GameObject("TextoFlutuante");
        var v = raiz.AddComponent<TextoFlutuante>();
        v.texto = TextMesh(raiz.transform, "Texto", fonte, 26, Vector3.zero);
        v.sombra = TextMesh(raiz.transform, "Sombra", fonte, 25, new Vector3(0.03f, -0.03f, 0f));
        Salvar(raiz, CaminhoTexto);
    }

    static TextMesh TextMesh(Transform pai, string nome, Font fonte, int ordem, Vector3 deslocamento)
    {
        var go = new GameObject(nome);
        go.transform.SetParent(pai, false);
        go.transform.localPosition = deslocamento;
        var t = go.AddComponent<TextMesh>();
        t.font = fonte;
        t.fontSize = 64;
        t.characterSize = 0.07f;
        t.fontStyle = FontStyle.Bold;
        t.anchor = TextAnchor.MiddleCenter;
        t.alignment = TextAlignment.Center;
        var r = go.GetComponent<MeshRenderer>();
        r.sharedMaterial = fonte.material;
        r.sortingOrder = ordem;
        return t;
    }

    static void CriarLama()
    {
        var go = new GameObject("Lama");
        var r = go.AddComponent<SpriteRenderer>();
        r.sprite = ConstrutorAssets.Sprite("mancha_lama");
        r.sortingOrder = -3; // no chão, por baixo de pássaros e porcos
        Salvar(go, CaminhoLama);
    }

    // Um sistema de partículas que só emite quando o Efeitos pede (Emit).
    static void CriarParticulas(string nome, string textura, (float, float) vida, (float, float) velocidade, (float, float) tamanho,
        float gravidade, int ordem, float giro, float freio, float crescer)
    {
        var go = new GameObject(nome);
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 1f;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(vida.Item1, vida.Item2);
        main.startSpeed = new ParticleSystem.MinMaxCurve(velocidade.Item1, velocidade.Item2);
        main.startSize = new ParticleSystem.MinMaxCurve(tamanho.Item1, tamanho.Item2);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.gravityModifier = gravidade;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 800;

        var emissao = ps.emission;
        emissao.rateOverTime = 0f;

        var forma = ps.shape;
        forma.shapeType = ParticleSystemShapeType.Circle; // no plano XY, como o jogo
        forma.radius = 0.18f;

        var cor = ps.colorOverLifetime;
        cor.enabled = true;
        var gradiente = new Gradient();
        gradiente.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.55f), new GradientAlphaKey(0f, 1f) });
        cor.color = gradiente;

        var tam = ps.sizeOverLifetime;
        tam.enabled = true;
        tam.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, crescer));

        var rot = ps.rotationOverLifetime;
        rot.enabled = true;
        rot.z = new ParticleSystem.MinMaxCurve(-giro, giro);

        var limite = ps.limitVelocityOverLifetime;
        limite.enabled = true;
        limite.limit = 0.6f;
        limite.dampen = freio;

        var r = go.GetComponent<ParticleSystemRenderer>();
        r.sharedMaterial = ConstrutorAssets.MaterialSprite("Particula_" + nome, ConstrutorAssets.Textura(textura));
        r.renderMode = ParticleSystemRenderMode.Billboard;
        r.sortingOrder = ordem;

        Salvar(go, CaminhoParticulas(nome));
    }

    public static CirculoAlcance Circulo(Transform pai, string nome)
    {
        var go = new GameObject(nome);
        go.transform.SetParent(pai, false);
        var anel = go.AddComponent<SpriteRenderer>();
        anel.sprite = ConstrutorAssets.Sprite("anel");
        anel.sortingOrder = -2; // no chão, por cima da trilha e por baixo dos bichos
        anel.enabled = false;
        var c = go.AddComponent<CirculoAlcance>();
        c.anel = anel;
        return c;
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

    static void Salvar(GameObject raiz, string caminho)
    {
        PrefabUtility.SaveAsPrefabAsset(raiz, caminho);
        Object.DestroyImmediate(raiz);
    }
}
