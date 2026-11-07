using UnityEditor;
using UnityEngine;
using static ConstrutorAssets;

// Monta os prefabs: raposas, bloco, inimigos, texto flutuante e "Sistemas"
// (câmera, cenário, estilingue, sons e efeitos, que toda fase usa igual).
public static class ConstrutorPrefabs
{
    public const string Pasta = Raiz + "/Prefabs";

    public static Raposa RaposaNormal, RaposaNinja, RaposaBomba;
    public static RaposaAtacante AtacanteNormal, AtacanteNinja, AtacanteBomba, AtacanteGrande;
    public static GameObject ArmaEstilingue, ArmaCanhao;
    public static GameObject Bloco, Inimigo, InimigoGrande, Sistemas;

    // Ordem de desenho (sortingOrder): quem tem número maior aparece na frente.
    public const int OrdemFundo = -100, OrdemEstilingue = 1, OrdemBloco = 2, OrdemInimigo = 3, OrdemRastro = 4,
        OrdemElasticoTras = 5, OrdemRaposa = 6, OrdemElasticoFrente = 7, OrdemChao = 10, OrdemFumaca = 19, OrdemEfeitos = 20;

    static PhysicsMaterial2D fisicaRaposa, fisicaChao, fisicaInimigo, fisicaAtacante;

    // Silhueta do tubarão (inimigo.png, 72×64 px, PPU 80, pivô no centro). Fica dentro
    // da caixa de 0.8 × 0.7 do antigo triângulo (DefinicoesFases.TamanhoInimigo), que é
    // o espaço que as fases reservam para cada inimigo; os pés ficam em y = -0.35.
    static readonly Vector2[] ContornoTubarao =
    {
        new Vector2(0.05f, 0.35f), new Vector2(0.125f, 0.35f), new Vector2(0.15f, 0.225f),       // barbatana
        new Vector2(0.2125f, 0.125f), new Vector2(0.25f, 0.025f),                               // costas
        new Vector2(0.375f, 0.1375f), new Vector2(0.4f, 0.1f), new Vector2(0.3625f, -0.0625f),   // rabo
        new Vector2(0.4f, -0.225f), new Vector2(0.275f, -0.2125f), new Vector2(0.175f, -0.2625f),
        new Vector2(0.175f, -0.35f), new Vector2(-0.2125f, -0.35f),                             // pés
        new Vector2(-0.3f, -0.2f), new Vector2(-0.4f, -0.1875f), new Vector2(-0.4f, -0.125f),   // nadadeira
        new Vector2(-0.375f, 0f), new Vector2(-0.3125f, 0.1375f), new Vector2(-0.2f, 0.225f),   // cabeça
    };

    public static void Tudo()
    {
        GarantirPasta(Pasta);
        fisicaRaposa = MaterialFisico("Raposa", 0.6f, 0.35f);
        fisicaChao = MaterialFisico("Chao", 0.9f, 0f);
        fisicaInimigo = MaterialFisico("Inimigo", 0.8f, 0.05f);

        RaposaNormal = CriarRaposa("Raposa_Normal", typeof(Raposa), TipoRaposa.Normal, "raposa_normal", 1f,
            "Arraste a raposa para trás e solte!", new Color(1f, 1f, 1f, 0.6f));
        RaposaNinja = CriarRaposa("Raposa_Ninja", typeof(RaposaNinja), TipoRaposa.Ninja, "raposa_ninja", 0.9f,
            "Raposa ninja: clique no ar para dar uma arrancada!", new Color(0.55f, 0.65f, 1f, 0.7f));
        RaposaBomba = CriarRaposa("Raposa_Bomba", typeof(RaposaBomba), TipoRaposa.Bomba, "raposa_bomba", 1.4f,
            "Raposa bomba: clique para explodir (ou espere o pavio)!", new Color(1f, 0.6f, 0.3f, 0.7f));
        CriarAtacantes();
        CriarArmas();
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

    // As raposas "zumbi" do tower defense (a leva que anda até a torre). Usam a mesma arte das
    // raposas do estilingue, mas ficam em pé (sem girar) e andam sozinhas (RaposaAtacante).
    // Público para dar para criar só elas, sem reconstruir o resto (ver MURAL.md).
    public static void CriarAtacantes()
    {
        GarantirPasta(Pasta);
        fisicaAtacante = MaterialFisico("Atacante", 0f, 0f); // sem atrito nem quique: deslizam até a torre

        //                                  nome             script                 tipo              sprite          massa vida vel  dano  golpe pontos
        AtacanteNormal = CriarAtacante("Atacante_Normal", typeof(RaposaAtacante), TipoRaposa.Normal, "raposa_normal", 0.8f, 30f, 1.0f, 8f, 0.8f, 100);
        AtacanteNinja = CriarAtacante("Atacante_Ninja", typeof(RaposaAtacante), TipoRaposa.Ninja, "raposa_ninja", 0.7f, 20f, 2.0f, 5f, 0.6f, 150);
        AtacanteBomba = CriarAtacante("Atacante_Bomba", typeof(AtacanteBomba), TipoRaposa.Bomba, "raposa_bomba", 1.0f, 25f, 1.2f, 0f, 0.8f, 200);
        AtacanteGrande = CriarGrande();
    }

    // A grande é a normal em tamanho 1,5, avermelhada: lenta, muito resistente e com golpe pesado.
    // Separada para dar para criar só ela sem regravar os outros prefabs (regravar pode trocar os
    // ids internos e desligar as referências que as cenas têm para eles).
    public static RaposaAtacante CriarGrande()
    {
        GarantirPasta(Pasta);
        fisicaAtacante = MaterialFisico("Atacante", 0f, 0f);
        return CriarAtacante("Atacante_Grande", typeof(RaposaAtacante), TipoRaposa.Grande, "raposa_normal", 2.5f, 90f, 0.7f, 20f, 1.0f, 300,
            1.5f, new Color(0.85f, 0.45f, 0.4f));
    }

    static RaposaAtacante CriarAtacante(string nome, System.Type script, TipoRaposa tipo, string sprite,
        float massa, float vida, float velocidade, float dano, float intervalo, int pontos,
        float escala = 1f, Color? cor = null)
    {
        var go = new GameObject(nome);
        var desenho = Desenho(go, sprite, OrdemRaposa);
        if (cor.HasValue) desenho.color = cor.Value;
        go.transform.localScale = Vector3.one * escala;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.mass = massa;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 26f / PixelsPorUnidade;
        col.sharedMaterial = fisicaAtacante;

        var r = (RaposaAtacante)go.AddComponent(script);
        r.tipo = tipo;
        r.vidaMaxima = vida;
        r.velocidade = velocidade;
        r.danoPorGolpe = dano;
        r.intervaloGolpe = intervalo;
        r.pontos = pontos;
        // Andar até a torre não pode machucar a própria raposa: só pancada forte (pedra caindo).
        r.impulsoMinimo = 3f;
        r.multiplicadorDano = 6f;
        return Salvar(go, nome).GetComponent<RaposaAtacante>();
    }

    // As armas de defesa (issue #7). Usam a arte do estilingue antigo (o canhão é o mesmo desenho,
    // mais largo e azul-acinzentado, enquanto não há uma arte própria). Ficam paradas no chão
    // (FreezeAll) mas continuam sendo corpos dinâmicos, para as explosões e as raposas
    // conseguirem machucá-las. Público para dar para criar só elas, sem reconstruir o resto.
    public static void CriarArmas()
    {
        GarantirPasta(Pasta);
        //                            nome               tamanho (L x A)           vida  cor da arma                        cor do tiro
        ArmaEstilingue = CriarArma("Arma_Estilingue", new Vector2(0.6f, 1.0f), 80f, Color.white, new Color(0.9f, 0.8f, 0.6f));
        ArmaCanhao = CriarArma("Arma_Canhao", new Vector2(0.9f, 0.7f), 120f, new Color(0.55f, 0.62f, 0.78f), new Color(0.25f, 0.25f, 0.3f));
    }

    static GameObject CriarArma(string nome, Vector2 tamanho, float vida, Color cor, Color corDoTiro)
    {
        var go = new GameObject(nome);

        var rb = go.AddComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        var caixa = go.AddComponent<BoxCollider2D>();
        caixa.size = tamanho;
        caixa.sharedMaterial = MaterialFisico("Chao", 0.9f, 0f);

        // O desenho fica num filho: o sprite do estilingue tem o pivô na base, então o filho
        // desce até o pé da arma (o objeto em si fica no centro da área que ela ocupa).
        var filho = new GameObject("Desenho");
        filho.transform.SetParent(go.transform, false);
        var desenho = Desenho(filho, "estilingue", OrdemInimigo);
        desenho.color = cor;
        filho.transform.localScale = Vector3.one * (tamanho.y / desenho.sprite.bounds.size.y);
        filho.transform.localPosition = new Vector3(0f, -tamanho.y / 2f, 0f);

        var arma = go.AddComponent<Arma>();
        arma.vidaMaxima = vida;
        arma.impulsoMinimo = 50f; // só dano direto das raposas: encostar nela não conta
        arma.spriteProjetil = Sprite("ponto");
        arma.corProjetil = corDoTiro;
        return Salvar(go, nome);
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

        // Contorno do tubarão (com barbatanas e rabo). Côncavo: o Unity divide em partes convexas.
        var col = go.AddComponent<PolygonCollider2D>();
        col.pathCount = 1;
        col.SetPath(0, ContornoTubarao);
        // Área do contorno ≈ 0.38; com densidade 0.74 a massa fica 0.28, a mesma do
        // antigo triângulo (pesa igual nas batidas e nas explosões).
        col.density = 0.74f;
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
        // Mais largo que a tela (15): as raposas do tower defense entram andando de fora dela
        // (x = -8.3). Passando de |x| = 9 já contam como fora do mundo (Regras.ForaDoMundo).
        colChao.size = new Vector2(19f, 1.5f);
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
