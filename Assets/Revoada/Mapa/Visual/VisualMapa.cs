using UnityEngine;

// Desenha o mapa a partir do asset Mapa: grama, a trilha (dois LineRenderer,
// a borda mais larga por baixo), obstáculos, flores e a Torre dos Porcos.
// O construtor chama Desenhar() no Editor, então o mapa já aparece na Scene
// view sem dar Play. Mexeu nos pontos do Mapa no Inspector? Os Gizmos mostram
// a trilha nova na hora; clique com o botão direito no componente →
// "Redesenhar" para atualizar os sprites.
//
// Com as fases preenchidas (cena Jogo de verdade), MostrarFase troca para o
// mapa da fase: redesenha e tinge o cenário para o turno do dia (a Torre fica
// de fora, para não brigar com o flash de "apanhando" do VisualTorre). Quem
// decide a hora é a TrocaDeFase, com a tela escura, para o jogador não ver o
// mapa trocando aos pedaços.
public class VisualMapa : MonoBehaviour
{
    public Mapa mapa;
    [Tooltip("Os mapas das fases, na ordem (o primeiro é o da fase 1).")]
    public Mapa[] fases;

    [Header("Arte")]
    public Sprite grama;
    public Sprite arvore;
    public Sprite pedra;
    public Sprite lago;
    public Sprite torre;
    public Sprite[] flores;
    public Material materialTerra;
    public Material materialBorda;
    public int quantidadeFlores = 45;

    [Tooltip("Preenchido pelo Desenhar: o sprite da Torre dos Porcos.")]
    public SpriteRenderer torreDesenhada;

    static readonly Color TintoDia = Color.white;
    static readonly Color TintoTarde = new Color(1f, 0.8f, 0.62f);
    static readonly Color TintoNoite = new Color(0.46f, 0.52f, 0.78f);
    static readonly Color FundoDia = new Color(0.18f, 0.29f, 0.14f);
    static readonly Color FundoTarde = new Color(0.32f, 0.2f, 0.11f);
    static readonly Color FundoNoite = new Color(0.04f, 0.05f, 0.1f);

    public void MostrarFase(int fase)
    {
        if (fases == null || fase < 1 || fase > fases.Length) return;
        mapa = fases[fase - 1];
        Desenhar();
    }

    [ContextMenu("Redesenhar")]
    public void Desenhar()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var filho = transform.GetChild(i).gameObject;
            if (Application.isPlaying) Destroy(filho);
            else DestroyImmediate(filho);
        }

        var d = mapa.dados;
        var trilha = new Trilha(d.pontos, d.raioCanto);

        var chao = Filho("Grama", grama, d.tamanho / 2f, -10);
        chao.drawMode = SpriteDrawMode.Tiled;
        chao.size = d.tamanho;

        Linha("TrilhaBorda", trilha, d.meiaLargura * 2f + 0.22f, materialBorda, -7);
        Linha("Trilha", trilha, d.meiaLargura * 2f, materialTerra, -6);

        foreach (var o in d.obstaculos)
        {
            if (o.tipo == "lago") Filho("Lago", lago, o.centro, -5);
            else if (o.tipo == "pedra") Filho("Pedra", pedra, o.centro, 0);
            else Filho("Arvore", arvore, o.centro + Vector2.down * o.raio * 0.9f, 0); // pivô no pé do tronco
        }

        torreDesenhada = Filho("Torre", torre, d.torre + Vector2.down * d.raioTorre * 0.9f, 0);

        // Flores espalhadas com semente fixa: o mapa sai sempre igual.
        if (flores == null || flores.Length == 0) return;
        var rnd = new System.Random(7);
        int feitas = 0;
        for (int tentativa = 0; tentativa < 3000 && feitas < quantidadeFlores; tentativa++)
        {
            var p = new Vector2((float)rnd.NextDouble() * d.tamanho.x, (float)rnd.NextDouble() * d.tamanho.y);
            if (trilha.DistanciaAte(p) < d.meiaLargura + 0.35f) continue;
            if (Vector2.Distance(p, d.torre) < d.raioTorre + 0.5f) continue;
            bool livre = true;
            foreach (var o in d.obstaculos)
                if (Vector2.Distance(p, o.centro) < o.raio + 0.3f) { livre = false; break; }
            if (!livre) continue;
            var f = Filho("Flor", flores[rnd.Next(flores.Length)], p, -4);
            f.flipX = rnd.Next(2) == 0;
            feitas++;
        }

        Tingir(d.turno);
    }

    // O cenário (grama, trilha, obstáculos, flores) muda de cor para o turno;
    // a Torre fica de fora (o VisualTorre já mexe na cor dela para o flash de
    // "apanhando", e brigaria com um tingimento por cima). O fundo da câmera
    // também muda, para reforçar a hora do dia.
    void Tingir(TurnoDoDia turno)
    {
        var tinto = turno == TurnoDoDia.Tarde ? TintoTarde : turno == TurnoDoDia.Noite ? TintoNoite : TintoDia;
        foreach (var r in GetComponentsInChildren<SpriteRenderer>())
            if (r != torreDesenhada) r.color = tinto;
        foreach (var lr in GetComponentsInChildren<LineRenderer>())
        {
            lr.startColor = tinto;
            lr.endColor = tinto;
        }

        if (Camera.main != null)
            Camera.main.backgroundColor = turno == TurnoDoDia.Tarde ? FundoTarde : turno == TurnoDoDia.Noite ? FundoNoite : FundoDia;
    }

    SpriteRenderer Filho(string nome, Sprite sprite, Vector2 posicao, int ordem)
    {
        var go = new GameObject(nome);
        go.transform.SetParent(transform, false);
        go.transform.position = posicao;
        var r = go.AddComponent<SpriteRenderer>();
        r.sprite = sprite;
        r.sortingOrder = ordem;
        r.spriteSortPoint = SpriteSortPoint.Pivot; // ordena pela base (o "pé"), como num jogo visto de cima
        return r;
    }

    void Linha(string nome, Trilha trilha, float largura, Material material, int ordem)
    {
        var go = new GameObject(nome);
        go.transform.SetParent(transform, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.positionCount = trilha.Amostras.Count;
        for (int i = 0; i < trilha.Amostras.Count; i++) lr.SetPosition(i, trilha.Amostras[i]);
        lr.widthMultiplier = largura;
        lr.sharedMaterial = material;
        lr.textureMode = LineTextureMode.Tile; // a textura de terra se repete a cada unidade
        lr.numCapVertices = 4;
        lr.numCornerVertices = 4;
        lr.sortingOrder = ordem;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
    }

    void OnDrawGizmos()
    {
        if (mapa == null) return;
        var d = mapa.dados;
        var trilha = new Trilha(d.pontos, d.raioCanto);

        Gizmos.color = Color.yellow;
        for (int i = 0; i < trilha.Amostras.Count - 1; i++)
            Gizmos.DrawLine(trilha.Amostras[i], trilha.Amostras[i + 1]);
        Gizmos.color = Color.red;
        foreach (var o in d.obstaculos) Gizmos.DrawWireSphere(o.centro, o.raio);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(d.torre, d.raioTorre);
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(d.tamanho / 2f, d.tamanho);
    }
}
