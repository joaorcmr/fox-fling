using UnityEngine;

// Toca efeitos sonoros curtos. Usa várias AudioSources em rodízio para que
// sons iguais em sequência (vários blocos quebrando) não se cortem, e varia
// um pouco o tom de cada um para não soar repetitivo.
public class Sons : MonoBehaviour
{
    public static Sons I { get; private set; }

    public AudioClip lancar, esticar, pulo, inimigoMorre, explosao, arrancada, vitoria, derrota, clique, estrela;

    [Range(0f, 1f)] public float volumeGeral = 0.8f;
    public int canais = 8;

    AudioSource[] fontes;
    int proxima;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetarStatics() => I = null;

    void Awake()
    {
        I = this;
        fontes = new AudioSource[canais];
        for (int i = 0; i < canais; i++)
        {
            fontes[i] = gameObject.AddComponent<AudioSource>();
            fontes[i].playOnAwake = false;
        }
    }

    void OnDestroy()
    {
        if (I == this) I = null;
    }

    public static void Tocar(AudioClip clip, float volume = 1f, float variacaoTom = 0.08f)
    {
        if (I == null || clip == null) return;

        var fonte = I.fontes[I.proxima];
        I.proxima = (I.proxima + 1) % I.fontes.Length;
        fonte.pitch = 1f + Random.Range(-variacaoTom, variacaoTom);
        fonte.PlayOneShot(clip, volume * I.volumeGeral);
    }
}
