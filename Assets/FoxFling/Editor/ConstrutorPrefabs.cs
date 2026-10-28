using UnityEditor;
using UnityEngine;
using static ConstrutorAssets;

// Monta os prefabs: raposas, bloco, inimigos, texto flutuante e "Sistemas"
// (câmera, cenário, estilingue, sons e efeitos, que toda fase usa igual).
public static class ConstrutorPrefabs
{
    public const string Pasta = Raiz + "/Prefabs";

    public static Raposa RaposaNormal, RaposaNinja, RaposaBomba;
    public static GameObject Bloco, Inimigo, InimigoGrande, Sistemas;

    // Ordem de desenho (sortingOrder): quem tem número maior aparece na frente.
    public const int OrdemFundo = -100, OrdemEstilingue = 1, OrdemBloco = 2, OrdemInimigo = 3, OrdemRastro = 4,
        OrdemElasticoTras = 5, OrdemRaposa = 6, OrdemElasticoFrente = 7, OrdemChao = 10, OrdemFumaca = 19, OrdemEfeitos = 20;

    static PhysicsMaterial2D fisicaRaposa, fisicaChao, fisicaInimigo;

    public static void Tudo()
    {
        GarantirPasta(Pasta);
        fisicaRaposa = MaterialFisico("Raposa", 0.6f, 0.35f);
        fisicaChao = MaterialFisico("Chao", 0.9f, 0f);
        fisicaInimigo = MaterialFisico("Inimigo", 0.8f, 0.05f);

        RaposaNormal = CriarRaposa("Raposa_Normal", typeof(Raposa), TipoRaposa.Normal, "raposa_normal", 1f,
            "Arraste a raposa para trás e solte!", new Color(1f, 1f, 1f, 0.6f));
        Bloco = CriarBloco();
        Inimigo = CriarInimigo("Inimigo", 1f, 20f);
        InimigoGrande = CriarInimigo("Inimigo_Grande", DefinicoesFases.EscalaGrande, 45f);
        Sistemas = CriarSistemas(CriarTextoFlutuante());
    }

    static GameObject Salvar(GameObject go, string nome)
    {
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, $"{Pasta}/{nome}.prefab");
        Object.DestroyImmediate(go);
        return prefab;
    }

    static GameObject Filho(Transform pai, string nome, Vector3 posicaoLocal)
    {
        var go = new GameObject(nome);
        go.transform.SetParent(pai, false);
        go.transform.localPosition = posicaoLocal;
        return go;
    }

    static SpriteRenderer Desenho(GameObject go, string sprite, int ordem)
    {
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite(sprite);
        sr.sortingOrder = ordem;
        return sr;
    }

    public static Raposa CriarRaposa(string nome, System.Type script, TipoRaposa tipo, string sprite, float massa, string dica, Color corRastro)
    {
        var go = new GameObject(nome);
        Desenho(go, sprite, OrdemRaposa);

        var rb = go.AddComponent<Rigidbody2D>();
        rb.mass = massa;
        rb.linearDamping = 0f;   // sem resistência do ar: a trajetória prevista fica exata
        rb.angularDamping = 0.4f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        // Continuous evita que um objeto rápido "atravesse" um bloco fino entre dois quadros.
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 26f / PixelsPorUnidade;
        col.sharedMaterial = fisicaRaposa;

        var rastro = go.AddComponent<TrailRenderer>();
        rastro.time = 0.35f;
        rastro.widthMultiplier = 0.45f;
        rastro.widthCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
        rastro.sharedMaterial = MaterialRender("Rastro", null);
        rastro.startColor = corRastro;
        rastro.endColor = new Color(corRastro.r, corRastro.g, corRastro.b, 0f);
        rastro.sortingOrder = OrdemRastro;
        rastro.minVertexDistance = 0.05f;
        rastro.emitting = false;

        var r = (Raposa)go.AddComponent(script);
        r.tipo = tipo;
        r.dica = dica;
        return Salvar(go, nome).GetComponent<Raposa>();
    }

    static GameObject CriarBloco()
    {
        var go = new GameObject("Bloco");
        var sr = Desenho(go, "madeira", OrdemBloco);
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        // Começa "dormindo": a física não mexe na torre até alguém encostar nela.
        rb.sleepMode = RigidbodySleepMode2D.StartAsleep;
        rb.angularDamping = 0.05f;

        go.AddComponent<BoxCollider2D>();
        var bloco = go.AddComponent<Bloco>();
        bloco.impulsoMinimo = 1f;
        bloco.multiplicadorDano = 10f;
        bloco.Configurar(MaterialDeBloco(TipoMaterial.Madeira), Vector2.one);
        return Salvar(go, "Bloco");
    }

    static GameObject CriarInimigo(string nome, float escala, float vida)
    {
        var go = new GameObject(nome);
        go.transform.localScale = Vector3.one * escala;
        Desenho(go, "inimigo", OrdemInimigo);

        var rb = go.AddComponent<Rigidbody2D>();
        rb.useAutoMass = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.sleepMode = RigidbodySleepMode2D.StartAsleep;

        // Triângulo exato do desenho (0.8 de base × 0.7 de altura).
        var col = go.AddComponent<PolygonCollider2D>();
        col.pathCount = 1;
        col.SetPath(0, new[] { new Vector2(-0.4f, -0.35f), new Vector2(0.4f, -0.35f), new Vector2(0f, 0.35f) });
        col.density = 1f;
        col.sharedMaterial = fisicaInimigo;

        var inimigo = go.AddComponent<Inimigo>();
        inimigo.vidaMaxima = vida;
        inimigo.impulsoMinimo = 0.6f;
        inimigo.multiplicadorDano = 10f;
        inimigo.pontos = Regras.PontosInimigo;
        inimigo.spriteFerido = Sprite("inimigo_ferido");
        return Salvar(go, nome);
    }

    static TextoFlutuante CriarTextoFlutuante()
    {
        var go = new GameObject("TextoFlutuante");
        var tf = go.AddComponent<TextoFlutuante>();
        var fonte = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        TextMesh Linha(string nome, Vector3 deslocamento, int ordem)
        {
            var filho = Filho(go.transform, nome, deslocamento);
            var tm = filho.AddComponent<TextMesh>();
            tm.font = fonte;
            tm.fontSize = 64;
            tm.characterSize = 0.04f;
            tm.fontStyle = FontStyle.Bold;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            var mr = filho.GetComponent<MeshRenderer>();
            mr.sharedMaterial = fonte.material;
            mr.sortingOrder = ordem;
            return tm;
        }

        tf.sombra = Linha("Sombra", new Vector3(0.03f, -0.03f, 0f), 29);
        tf.texto = Linha("Texto", Vector3.zero, 30);
        return Salvar(go, "TextoFlutuante").GetComponent<TextoFlutuante>();
    }

    static GameObject CriarSistemas(TextoFlutuante prefabTexto)
    {
        var raiz = new GameObject("Sistemas");
        CriarCenario(raiz.transform);
        CriarEstilingue(raiz.transform);
        CriarSons(raiz.transform);
        CriarEfeitos(raiz.transform, prefabTexto);
        ConstrutorUI.CriarHud(raiz.transform);
        return Salvar(raiz, "Sistemas");
    }

    // Câmera, fundo e chão: usados nas fases (dentro de Sistemas) e no menu.
    public static void CriarCenario(Transform pai)
    {
        // Câmera ortográfica: tamanho 6 = metade da altura visível (12 unidades).
        var camGo = Filho(pai, "Main Camera", new Vector3(0f, 0f, -10f));
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.55f, 0.8f, 0.93f);
        camGo.AddComponent<AudioListener>();
        camGo.AddComponent<TremorCamera>();

        var fundo = Filho(pai, "Fundo", Vector3.zero);
        Desenho(fundo, "fundo", OrdemFundo);
        fundo.transform.localScale = Vector3.one * 1.04f; // sobra para o tremor de câmera

        // Chão: um BoxCollider2D sem Rigidbody2D é estático (nunca se move).
        var chao = Filho(pai, "Chao", new Vector3(0f, DefinicoesFases.Chao, 0f));
        Desenho(chao, "chao", OrdemChao);
        var colChao = chao.AddComponent<BoxCollider2D>();
        colChao.size = new Vector2(15f, 1.5f);
        colChao.offset = new Vector2(0f, -0.75f);
        colChao.sharedMaterial = fisicaChao;
    }

    public static void CriarSons(Transform pai)
    {
        var sons = Filho(pai, "Sons", Vector3.zero).AddComponent<Sons>();
        sons.lancar = Som("lancar");
        sons.esticar = Som("esticar");
        sons.pulo = Som("pulo");
        sons.inimigoMorre = Som("inimigo_morre");
        sons.explosao = Som("explosao");
        sons.arrancada = Som("arrancada");
        sons.vitoria = Som("vitoria");
        sons.derrota = Som("derrota");
        sons.clique = Som("clique");
        sons.estrela = Som("estrela");
    }

    static void CriarEstilingue(Transform pai)
    {
        var go = Filho(pai, "Estilingue", new Vector3(-5.5f, DefinicoesFases.Chao, 0f));
        Desenho(go, "estilingue", OrdemEstilingue);
        var est = go.AddComponent<Estilingue>();
        est.ancora = Filho(go.transform, "Ancora", new Vector3(0f, 1.8f, 0f)).transform;
        est.pontaTras = Filho(go.transform, "PontaTras", new Vector3(-0.3f, 1.975f, 0f)).transform;
        est.pontaFrente = Filho(go.transform, "PontaFrente", new Vector3(0.3f, 1.975f, 0f)).transform;

        LineRenderer Elastico(string nome, int ordem)
        {
            var lr = Filho(go.transform, nome, Vector3.zero).AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.useWorldSpace = true;
            lr.widthMultiplier = 0.11f;
            lr.numCapVertices = 3;
            lr.sharedMaterial = MaterialRender("Elastico", null);
            lr.startColor = lr.endColor = new Color(0.33f, 0.19f, 0.1f);
            lr.sortingOrder = ordem;
            return lr;
        }

        est.elasticoTras = Elastico("ElasticoTras", OrdemElasticoTras);
        est.elasticoFrente = Elastico("ElasticoFrente", OrdemElasticoFrente);
        // Posição de repouso (para a cena já aparecer certa fora do Play).
        est.elasticoTras.SetPositions(new[] { est.pontaTras.position, est.ancora.position });
        est.elasticoFrente.SetPositions(new[] { est.pontaFrente.position, est.ancora.position });

        var traj = Filho(go.transform, "Trajetoria", Vector3.zero).AddComponent<Trajetoria>();
        traj.spritePonto = Sprite("ponto");
        traj.chaoY = DefinicoesFases.Chao;
        est.trajetoria = traj;
    }

    static void CriarEfeitos(Transform pai, TextoFlutuante prefabTexto)
    {
        var efeitos = Filho(pai, "Efeitos", Vector3.zero).AddComponent<Efeitos>();
        efeitos.prefabTexto = prefabTexto;
        var matSuave = MaterialRender("Particula", Textura("particula"));

        // Pedaços que voam quando um bloco quebra.
        var detritos = SistemaDeParticulas(efeitos.transform, "Detritos", MaterialRender("Estilhaco", Textura("estilhaco")), OrdemEfeitos);
        var m = detritos.main;
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.3f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5.5f);
        m.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.24f);
        m.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        m.gravityModifier = 1.3f;
        var rot = detritos.rotationOverLifetime;
        rot.enabled = true;
        rot.z = new ParticleSystem.MinMaxCurve(-6f, 6f);
        Desbotar(detritos, 0.7f);
        Forma(detritos, 0.25f);

        // Fumaça (raposa sumindo, inimigo eliminado).
        var fumaca = SistemaDeParticulas(efeitos.transform, "Fumaca", matSuave, OrdemFumaca);
        m = fumaca.main;
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.4f);
        m.startSize = 0.6f;
        m.gravityModifier = -0.05f;
        Crescer(fumaca, 0.6f, 1.6f);
        Desbotar(fumaca, 0f);
        Forma(fumaca, 0.2f);

        // Fogo da explosão.
        var fogo = SistemaDeParticulas(efeitos.transform, "Fogo", matSuave, OrdemEfeitos);
        m = fogo.main;
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.55f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 5f);
        m.startSize = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
        m.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.92f, 0.35f), new Color(1f, 0.45f, 0.1f));
        Crescer(fogo, 1f, 0.4f);
        var cor = fogo.colorOverLifetime;
        cor.enabled = true;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(0.6f, 0.1f, 0.05f), 1f) },
                  new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        cor.color = g;
        Forma(fogo, 0.3f);

        // Onda de choque: um único anel que cresce até o raio da explosão.
        var anel = SistemaDeParticulas(efeitos.transform, "Anel", MaterialRender("Anel", Textura("anel")), OrdemEfeitos);
        m = anel.main;
        m.startLifetime = 0.35f;
        m.startSpeed = 0f;
        var forma = anel.shape;
        forma.enabled = false;
        var tam = anel.sizeOverLifetime;
        tam.enabled = true;
        tam.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.15f, 0f, 3f), new Keyframe(1f, 1f, 0f, 0f)));
        Desbotar(anel, 0f);

        efeitos.detritos = detritos;
        efeitos.fumaca = fumaca;
        efeitos.fogo = fogo;
        efeitos.anel = anel;
    }

    // Um ParticleSystem "parado" (taxa de emissão 0): só emite quando o código chama Emit().
    static ParticleSystem SistemaDeParticulas(Transform pai, string nome, Material material, int ordem)
    {
        var go = Filho(pai, nome, Vector3.zero);
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true;
        main.playOnAwake = true;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 400;
        var emissao = ps.emission;
        emissao.rateOverTime = 0f;
        var r = go.GetComponent<ParticleSystemRenderer>();
        r.sharedMaterial = material;
        r.sortingOrder = ordem;
        return ps;
    }

    static void Forma(ParticleSystem ps, float raio)
    {
        var forma = ps.shape;
        forma.enabled = true;
        forma.shapeType = ParticleSystemShapeType.Circle;
        forma.radius = raio;
    }

    static void Desbotar(ParticleSystem ps, float inicioDoSumico)
    {
        var cor = ps.colorOverLifetime;
        cor.enabled = true;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                  new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, Mathf.Max(0.001f, inicioDoSumico)), new GradientAlphaKey(0f, 1f) });
        cor.color = g;
    }

    static void Crescer(ParticleSystem ps, float de, float ate)
    {
        var tam = ps.sizeOverLifetime;
        tam.enabled = true;
        tam.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, de, 1f, ate));
    }
}
