using UnityEngine;

// Desenha o mapa a partir do asset Mapa: grama, a trilha (dois LineRenderer,
// a borda mais larga por baixo), obstáculos, flores e a Torre dos Porcos.
// O construtor chama Desenhar() no Editor, então o mapa já aparece na Scene
// view sem dar Play. Mexeu nos pontos do Mapa no Inspector? Os Gizmos mostram
// a trilha nova na hora; clique com o botão direito no componente →
// "Redesenhar" para atualizar os sprites.
public class VisualMapa : MonoBehaviour
{
    public Mapa mapa;

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
