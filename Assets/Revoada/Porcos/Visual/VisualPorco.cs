using UnityEngine;

// O desenho de um porco (prefab Porco): corpo, arma que gira para a mira,
// acessório da especialização, tranco ao atirar e um quique ao ser colocado.
public class VisualPorco : MonoBehaviour
{
    public SpriteRenderer corpo;
    public Transform pivoArma;      // a arma gira em volta daqui (a "mão" do porco)
    public SpriteRenderer arma;
    public SpriteRenderer acessorio;
    public CirculoAlcance alcance;
    public SpriteRenderer[] pontos;   // bolinhas de nível ●●○
    public Sprite pontoCheio;
    public Sprite pontoVazio;
    public SpriteRenderer nota;       // ♪ quando está dentro da aura de um Tamboreiro

    [Header("Estado ao vivo (só leitura, para ver no Inspector durante o Play)")]
    public EspeciePorco especie;
    public int nivel;
    public ModoAlvo modo;
    public float alcanceAtual;

    public Porco Porco { get; private set; }

    TipoPorco tipo;
    float tranco;       // 1 logo depois de atirar, cai até 0
    float inicioQuique;

    public void Iniciar(Porco p, TipoPorco t)
    {
        Porco = p;
        tipo = t;
        corpo.sprite = t.corpo;
        arma.sprite = t.arma;
        acessorio.sprite = null;
        transform.position = p.Posicao;
        tranco = 0f;
        inicioQuique = Time.time;

        if (p.Ataca)
        {
            pivoArma.localPosition = new Vector3(0f, -0.12f, 0f);
            arma.sortingOrder = 1;
        }
        else
        {
            // O tambor fica na frente da barriga e não gira.
            pivoArma.localPosition = new Vector3(0f, -0.32f, 0f);
            pivoArma.localRotation = Quaternion.identity;
            arma.sortingOrder = 2;
        }
        MostrarAlcance(false);
        Atualizar();
    }

    public void Atualizar()
    {
        var p = Porco;
        if (p == null) return;

        if (p.Ataca)
        {
            bool esquerda = p.Mira.x < -0.05f;
            corpo.flipX = esquerda;
            float angulo = Mathf.Atan2(p.Mira.y, p.Mira.x) * Mathf.Rad2Deg;
            pivoArma.localRotation = Quaternion.Euler(0f, 0f, angulo);
            arma.flipY = esquerda; // senão a arma fica de cabeça para baixo apontando para a esquerda
        }

        // Tranco (achata e volta) e quique de quando cai no mapa.
        tranco = Mathf.MoveTowards(tranco, 0f, Time.deltaTime * 6f);
        float q = Mathf.Clamp01((Time.time - inicioQuique) / 0.35f);
        float altura = q < 1f ? Mathf.Abs(Mathf.Sin(q * Mathf.PI * 2f)) * (1f - q) * 0.35f : 0f;
        float achata = 1f + tranco * 0.18f;
        corpo.transform.localScale = new Vector3(achata, 1f / achata, 1f);
        corpo.transform.localPosition = new Vector3(0f, altura, 0f);

        acessorio.sprite = p.Especializacao >= 0 && tipo != null ? (p.Especializacao == 0 ? tipo.acessorioA : tipo.acessorioB) : null;
        acessorio.transform.localPosition = corpo.transform.localPosition;
        acessorio.flipX = corpo.flipX;

        for (int i = 0; i < pontos.Length; i++)
            pontos[i].sprite = i < Mathf.Min(p.Nivel, 3) ? pontoCheio : pontoVazio;

        bool fortalecido = p.MultCadencia > 1.001f || p.MultAlcance > 1.001f;
        nota.enabled = fortalecido;
        if (fortalecido)
            nota.transform.localPosition = new Vector3(0.4f, 0.55f + Mathf.Sin(Time.time * 4f + p.Id) * 0.06f, 0f);

        if (alcance.Visivel) alcance.Mostrar(p.Posicao, p.Alcance, alcance.Cor);

        especie = p.Especie;
        nivel = p.Nivel;
        modo = p.Modo;
        alcanceAtual = p.Alcance;
    }

    public void Atirou() => tranco = 1f;

    public void MostrarAlcance(bool mostrar)
    {
        if (mostrar && Porco != null) alcance.Mostrar(Porco.Posicao, Porco.Alcance, new Color(1f, 1f, 1f, 0.85f));
        else alcance.Esconder();
    }

    void OnDrawGizmosSelected()
    {
        if (Porco == null) return;
        Gizmos.color = Porco.Ataca ? Color.cyan : Color.green;
        Gizmos.DrawWireSphere(Porco.Posicao, Porco.Ataca ? Porco.Alcance : Porco.Atributos.raioAura);
    }
}
