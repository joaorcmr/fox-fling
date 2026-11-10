using System.Collections.Generic;
using UnityEngine;

// Todo o som do jogo passa por aqui. Os efeitos tocam num pool de AudioSource
// (vozes); a música e o ambiente ficam em loop. O volume passa por uma "camada
// de ganho" (VolumeEfeitos e VolumeMusica, de 0 a 1), que os controles de
// volume mudam e as Preferencias guardam. Criar um AudioMixer exige a janela do
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
    bool ambienteComecou, musicaComecou;
    int proxima;
    readonly Dictionary<AudioClip, float> ultimaVez = new Dictionary<AudioClip, float>();

    void Awake()
    {
        Instancia = this;
        VolumeEfeitos = Preferencias.Volume(Preferencias.Efeitos);
        VolumeMusica = Preferencias.Volume(Preferencias.Musica);

        fontes = new AudioSource[vozes];
        for (int i = 0; i < vozes; i++) fontes[i] = NovaFonte(false);
        fonteAmbiente = NovaFonte(true);
        fonteMusica = NovaFonte(true);

        // No navegador (WebGL) o próprio navegador decodifica o áudio, sem
        // esperar: um Play() aqui chegaria antes do som estar pronto e não
        // tocaria nada. Então o Awake só pede o carregamento de tudo, e os
        // loops começam no Update, quando o clipe já carregou. No Editor o
        // carregamento é na hora e os loops começam logo no primeiro quadro.
        foreach (var clip in Clipes())
            if (clip != null) clip.LoadAudioData();
    }

    IEnumerable<AudioClip> Clipes()
    {
        if (estouros != null)
            foreach (var e in estouros) yield return e;
        yield return tiroEstilingue; yield return tiroArpao; yield return tiroBomba; yield return explosao;
        yield return compra; yield return upgrade; yield return venda; yield return erro; yield return clique;
        yield return onda; yield return chefe; yield return torre; yield return vitoria; yield return derrota;
        yield return ambiente; yield return musica;
    }

    // A regra do loop, à parte para ser testada (SonsTests): começa uma vez só,
    // e só quando o clipe terminou de carregar.
    public static bool PodeComecarLoop(bool jaComecou, bool temClip, AudioDataLoadState estado) =>
        !jaComecou && temClip && estado == AudioDataLoadState.Loaded;

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

    static void ComecarSePronto(AudioSource fonte, AudioClip clip, ref bool comecou)
    {
        var estado = clip != null ? clip.loadState : AudioDataLoadState.Unloaded;
        if (!PodeComecarLoop(comecou, clip != null, estado)) return;
        fonte.clip = clip;
        fonte.Play();
        comecou = true;
    }

    void Update()
    {
        ComecarSePronto(fonteAmbiente, ambiente, ref ambienteComecou);
        ComecarSePronto(fonteMusica, musica, ref musicaComecou);

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
        if (clip.loadState != AudioDataLoadState.Loaded) return; // no navegador, ainda decodificando
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
        if (canal == Preferencias.Efeitos) VolumeEfeitos = valor;
        else VolumeMusica = valor;
        Preferencias.SalvarVolume(canal, valor);
    }
}
