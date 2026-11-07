using UnityEngine;

// Colocar um item da loja no chão: depois de clicar no botão, uma "sombra" segue o mouse
// (grudada numa grade, em cima do chão). Verde = dá para colocar; vermelho = lugar ocupado
// ou sem saldo. Clique esquerdo coloca e cobra; clique direito (ou Esc, pelo HUD) cancela.
// Continua no modo de colocação enquanto houver saldo para outro igual.
public class Colocador : MonoBehaviour
{
    static readonly Color Livre = new Color(0.55f, 1f, 0.55f, 0.6f);
    static readonly Color Ocupado = new Color(1f, 0.4f, 0.4f, 0.6f);

    // Encolhe a caixa de teste, para encostar no chão ou num vizinho não contar como ocupado
    // (a física 2D considera uma "pele" de cerca de 0,02 em volta dos colliders).
    const float Folga = 0.12f;

    GerenciadorFase fase;
    ItemLoja item;
    GameObject sombra;
    SpriteRenderer desenhoDaSombra;
    Camera cam;

    public bool Ativo => item != null;
    public ItemLoja ItemAtual => item;

    public void Configurar(GerenciadorFase gerenciador)
    {
        fase = gerenciador;
    }

    public void Comecar(ItemLoja novo)
    {
        Cancelar();
        item = novo;
        cam = Camera.main;
        CriarSombra();
    }

    // Devolve true se estava colocando algo (e agora não está mais).
    public bool Cancelar()
    {
        if (item == null) return false;
        item = null;
        if (sombra != null) Destroy(sombra);
        sombra = null;
        return true;
    }

    void CriarSombra()
    {
        sombra = new GameObject("Sombra de " + item.nome);
        desenhoDaSombra = sombra.AddComponent<SpriteRenderer>();
        desenhoDaSombra.sortingOrder = 8;

        if (item.tipo == TipoItem.Muro && item.material != null)
        {
            bool vertical = item.tamanho.y > item.tamanho.x && item.material.spriteVertical != null;
            desenhoDaSombra.sprite = vertical ? item.material.spriteVertical : item.material.sprite;
            desenhoDaSombra.drawMode = SpriteDrawMode.Tiled;
            desenhoDaSombra.size = item.tamanho;
        }
        else if (item.prefab != null)
        {
            var original = item.prefab.GetComponent<SpriteRenderer>();
            if (original != null) desenhoDaSombra.sprite = original.sprite;
            sombra.transform.localScale = item.prefab.transform.localScale;
        }
    }

    void Update()
    {
        if (item == null) return;

        if (fase == null || fase.Estado != EstadoFase.Preparando)
        {
            Cancelar();
            return;
        }

        Vector2 mouse = cam.ScreenToWorldPoint(Input.mousePosition);
        bool sobreInterface = Raposa.PonteiroNaInterface();

        float x = Regras.XDaColocacao(mouse.x, item.tamanho.x);
        var centro = new Vector2(x, fase.alturaDoChao + item.tamanho.y / 2f);
        bool livre = EspacoLivre(centro);
        bool pagavel = fase.Saldo >= item.preco;

        sombra.SetActive(!sobreInterface);
        sombra.transform.position = centro;
        desenhoDaSombra.color = livre && pagavel ? Livre : Ocupado;

        if (Input.GetMouseButtonDown(1))
        {
            Cancelar();
            fase.Mensagem("Colocação cancelada.");
        }
        else if (Input.GetMouseButtonDown(0) && !sobreInterface)
        {
            if (!livre) fase.Mensagem("Esse lugar está ocupado.");
            else if (!pagavel) fase.Mensagem("Saldo insuficiente.");
            else Colocar(centro);
        }
    }

    bool EspacoLivre(Vector2 centro)
    {
        return Physics2D.OverlapBox(centro, item.tamanho - Vector2.one * Folga, 0f) == null;
    }

    void Colocar(Vector2 centro)
    {
        if (!fase.TentarGastar(item.preco)) return;

        var go = Instantiate(item.prefab, centro, Quaternion.identity);
        if (item.tipo == TipoItem.Muro)
        {
            // O Awake do prefab usa o material padrão (madeira 1×1); Configurar troca pelo do item
            // e RestaurarVida acerta a vida para a do novo material.
            var bloco = go.GetComponent<Bloco>();
            bloco.Configurar(item.material, item.tamanho);
            bloco.RestaurarVida();
            go.name = item.nome.Replace(' ', '_');

            // Muro é defesa: as raposas não podem empurrá-lo nem derrubá-lo, só quebrá-lo.
            // (Os blocos da torre continuam livres: a torre desmoronar é parte do jogo.)
            go.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        }

        Sons.Tocar(Sons.I != null ? Sons.I.clique : null);
        fase.Mensagem($"{item.nome} colocado! (clique direito termina)");

        // Sem saldo para outro igual: sai do modo de colocação.
        if (fase.Saldo < item.preco) Cancelar();
    }

    void OnDestroy()
    {
        if (sombra != null) Destroy(sombra);
    }
}
