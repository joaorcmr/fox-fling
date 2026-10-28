using UnityEngine;

// Efeitos visuais sob demanda: um ParticleSystem de cada tipo fica parado na cena
// e "emite" partículas no lugar pedido (mais barato que criar um por efeito).
public class Efeitos : MonoBehaviour
{
    public static Efeitos I { get; private set; }

    public ParticleSystem detritos, fumaca, fogo, anel;
    public TextoFlutuante prefabTexto;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetarStatics() => I = null;

    void Awake() => I = this;

    void OnDestroy()
    {
        if (I == this) I = null;
    }

    static void Emitir(ParticleSystem ps, Vector3 onde, int quantidade, Color cor, float tamanho = -1f)
    {
        if (ps == null) return;
        var p = new ParticleSystem.EmitParams
        {
            position = onde,
            applyShapeToPosition = true,
        };
        if (cor.a > 0f) p.startColor = cor;
        if (tamanho > 0f) p.startSize = tamanho;
        ps.Emit(p, quantidade);
    }

    public static void Detritos(Vector3 onde, Color cor, int quantidade)
    {
        if (I == null) return;
        // Metade com a cor pura e metade um pouco mais escura, para dar volume.
        Emitir(I.detritos, onde, quantidade / 2 + 1, cor);
        Emitir(I.detritos, onde, quantidade / 2, Color.Lerp(cor, Color.black, 0.3f));
    }

    public static void Fumaca(Vector3 onde, float escala)
    {
        if (I == null) return;
        Emitir(I.fumaca, onde, Mathf.RoundToInt(8 * escala) + 2, new Color(0.9f, 0.9f, 0.94f, 0.95f), 0.6f * escala);
    }

    public static void Explosao(Vector3 onde, float raio)
    {
        if (I == null) return;
        Emitir(I.fogo, onde, 26, new Color(0, 0, 0, 0));
        Emitir(I.fumaca, onde, 14, new Color(0.35f, 0.33f, 0.32f, 0.85f), 0.9f);
        Emitir(I.anel, onde, 1, new Color(1f, 0.95f, 0.8f, 0.9f), raio * 2f);
    }

    public static void Pontos(Vector3 onde, int pontos)
    {
        if (I == null || I.prefabTexto == null || pontos <= 0) return;
        var texto = Instantiate(I.prefabTexto, onde, Quaternion.identity);
        bool grande = pontos >= Regras.PontosInimigo;
        texto.Mostrar("+" + pontos, grande ? new Color(1f, 0.85f, 0.25f) : Color.white, grande ? 1.35f : 1f);
    }
}
