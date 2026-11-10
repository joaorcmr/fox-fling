using UnityEngine;

// O porco "fantasma" na ponta do mouse enquanto o jogador escolhe onde colocar
// o porco da loja: verde = pode colocar aqui, vermelho = não pode, com o
// círculo do alcance em volta. Só desenha; a posição do mouse vem dos
// Controles e o porco na mão vem da Selecao.
public class FantasmaColocacao : MonoBehaviour
{
    static readonly Color Pode = new Color(0.75f, 1f, 0.75f, 0.85f);
    static readonly Color NaoPode = new Color(1f, 0.5f, 0.5f, 0.85f);
    static readonly Color CirculoPode = new Color(1f, 1f, 1f, 0.8f);
    static readonly Color CirculoNaoPode = new Color(1f, 0.45f, 0.45f, 0.8f);

    public Jogo jogo;
    public SpriteRenderer sprite;
    public CirculoAlcance circulo;

    void Start()
    {
        Esconder();
        jogo.Selecao.ColocacaoMudou += especie =>
        {
            if (especie.HasValue) sprite.sprite = jogo.Tipos.De(especie.Value).corpo;
            else Esconder();
        };
    }

    // Chamado a cada quadro pelos Controles com o ponto do mapa sob o mouse.
    public void Seguir(Vector2 mundo, bool sobreUI)
    {
        var especie = jogo.Selecao.Colocando;
        if (!especie.HasValue) return;

        bool pode = jogo.Sim.PodeColocar(especie.Value, mundo);
        sprite.enabled = !sobreUI;
        sprite.transform.position = mundo;
        sprite.color = pode ? Pode : NaoPode;
        if (sobreUI) circulo.Esconder();
        else circulo.Mostrar(mundo, jogo.Tipos.De(especie.Value).dados.nivel1.alcance, pode ? CirculoPode : CirculoNaoPode);
    }

    void Esconder()
    {
        sprite.enabled = false;
        circulo.Esconder();
    }
}
