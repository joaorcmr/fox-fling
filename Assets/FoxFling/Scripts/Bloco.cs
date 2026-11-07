using UnityEngine;

// Retângulos e quadrados (obstáculos). O tamanho e o material definem tudo:
// o sprite repete no modo Tiled, o BoxCollider2D acompanha o tamanho e a massa
// sai da densidade do material × área (Rigidbody2D.useAutoMass).
[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class Bloco : Destrutivel
{
    public MaterialBloco material;
    public Vector2 tamanho = Vector2.one;

    SpriteRenderer sr;
    bool rachado;
    float proximoSom;

    public void Configurar(MaterialBloco novoMaterial, Vector2 novoTamanho)
    {
        material = novoMaterial;
        tamanho = novoTamanho;
        Aplicar();
    }

    // Copia os números do material para os componentes. Clique direito no
    // componente → "Aplicar material" para ver a mudança sem dar Play.
    [ContextMenu("Aplicar material")]
    public void Aplicar()
    {
        if (material == null) return;

        var desenho = GetComponent<SpriteRenderer>();
        desenho.drawMode = SpriteDrawMode.Tiled;
        desenho.sprite = EscolherSprite(rachado);
        desenho.size = tamanho;

        // useAutoMass antes da densidade: o Unity só aceita densidade com massa automática.
        GetComponent<Rigidbody2D>().useAutoMass = true;

        var caixa = GetComponent<BoxCollider2D>();
        caixa.size = tamanho;
        caixa.density = material.densidade;
        caixa.sharedMaterial = material.fisica;

        vidaMaxima = material.vida;
        pontos = material.pontos;
    }

    protected override void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        Aplicar();
        base.Awake();
    }

    Sprite EscolherSprite(bool quebrado)
    {
        bool vertical = tamanho.y > tamanho.x && material.spriteVertical != null;
        if (vertical) return quebrado ? material.spriteVerticalRachado : material.spriteVertical;
        return quebrado ? material.spriteRachado : material.sprite;
    }

    protected override void AoBater(float impulso)
    {
        // Um "toc" proporcional à pancada, sem repetir o som a cada quadro.
        if (impulso < 0.4f || Time.time < proximoSom) return;
        proximoSom = Time.time + 0.12f;
        Sons.Tocar(material.somImpacto, Mathf.Clamp01(impulso / 6f));
    }

    protected override void AoMudarVida()
    {
        if (!rachado && Vida < vidaMaxima * 0.5f)
        {
            rachado = true;
            sr.sprite = EscolherSprite(true);
            sr.size = tamanho;
        }
    }

    // No tower defense a torre é do jogador: quebrar um bloco não rende pontos.
    protected override int PontosAoMorrer => 0;

    protected override void Morrer()
    {
        if (Morto) return;
        int pedacos = Mathf.Clamp(Mathf.RoundToInt(tamanho.x * tamanho.y * 24f), 6, 24);
        Efeitos.Detritos(transform.position, material.corDetritos, pedacos);
        Sons.Tocar(material.somQuebra, 0.8f);
        base.Morrer();
    }
}
