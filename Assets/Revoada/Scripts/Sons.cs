using System.Collections.Generic;
using UnityEngine;

// Todo o som do jogo passa por aqui. Os efeitos tocam num pool de AudioSource
// (vozes); a música e o ambiente ficam em loop. O volume passa por uma "camada
// de ganho" (VolumeEfeitos e VolumeMusica, de 0 a 1), que os controles de
// volume mudam e o Progresso guarda. Criar um AudioMixer exige a janela do
// Editor; essa camada faz o mesmo papel e roda no WebGL igual.
public class Sons : MonoBehaviour
{
    public static Sons Instancia { get; private set; }
    public static float VolumeEfeitos = 0.8f;
    public static float VolumeMusica = 0.5f;

    [Header("Efeitos")]
    public AudioClip[] estouros;
    public AudioClip tiroEstilingue, tiroArpao, tiroBomba, explosao;
    public AudioClip compra, upgrade, venda, erro, clique;
    public AudioClip onda, chefe, torre, vitoria, derrota;

    [Header("Fundo (em loop)")]
    public AudioClip ambiente;
    public AudioClip musica;
    public float volumeAmbiente = 0.35f;
    public float volumeMusicaBase = 0.5f;

    [Header("Ajustes")]
    public int vozes = 16;
    [Tooltip("O mesmo som não repete antes disso (uma revoada de 80 pardais não vira chiado).")]
    public float intervaloMinimo = 0.045f;

    AudioSource[] fontes;
    AudioSource fonteAmbiente, fonteMusica;
    int proxima;
    readonly Dictionary<AudioClip, float> ultimaVez = new Dictionary<AudioClip, float>();

    void Awake()
    {
        Instancia = this;
        VolumeEfeitos = Progresso.Volume("efeitos", 0.8f);
        VolumeMusica = Progresso.Volume("musica", 0.5f);

        fontes = new AudioSource[vozes];
        for (int i = 0; i < vozes; i++) fontes[i] = NovaFonte(false);
        fonteAmbiente = NovaFonte(true);
        fonteMusica = NovaFonte(true);
        Loop(fonteAmbiente, ambiente);
        Loop(fonteMusica, musica);
    }

    void OnDestroy()
    {
        if (Instancia == this) Instancia = null;
    }

    AudioSource NovaFonte(bool emLoop)
    {
        var f = gameObject.AddComponent<AudioSource>();
        f.playOnAwake = false;
        f.loop = emLoop;
        f.spatialBlend = 0f; // som de interface e de jogo visto de cima: tudo 2D
        return f;
    }

    static void Loop(AudioSource fonte, AudioClip clip)
    {
        if (clip == null) return;
        fonte.clip = clip;
        fonte.Play();
    }

    void Update()
    {
        // Pausado, a música abaixa 6 dB (metade do volume) em vez de parar.
        float abaixar = Time.timeScale == 0f ? 0.5f : 1f;
        fonteMusica.volume = volumeMusicaBase * VolumeMusica * abaixar;
        fonteAmbiente.volume = volumeAmbiente * VolumeMusica * abaixar;
    }

    public static void Tocar(AudioClip clip, float volume = 1f, float variacaoTom = 0.08f)
    {
        if (Instancia != null && clip != null) Instancia.TocarAqui(clip, volume, variacaoTom);
    }

    public static void TocarEstouro(float volume = 0.6f)
    {
        if (Instancia == null || Instancia.estouros == null || Instancia.estouros.Length == 0) return;
        Tocar(Instancia.estouros[Random.Range(0, Instancia.estouros.Length)], volume);
    }

    void TocarAqui(AudioClip clip, float volume, float variacaoTom)
    {
        float agora = Time.unscaledTime;
        if (ultimaVez.TryGetValue(clip, out float antes) && agora - antes < intervaloMinimo) return;
        ultimaVez[clip] = agora;

        var f = fontes[proxima];
        proxima = (proxima + 1) % fontes.Length;
        f.clip = clip;
        f.pitch = 1f + Random.Range(-variacaoTom, variacaoTom); // repetir com tom igual cansa o ouvido
        f.volume = volume * VolumeEfeitos;
        f.Play();
    }

    public static void DefinirVolume(string canal, float valor)
    {
        if (canal == "efeitos") VolumeEfeitos = valor;
        else VolumeMusica = valor;
        Progresso.SalvarVolume(canal, valor);
    }
}
