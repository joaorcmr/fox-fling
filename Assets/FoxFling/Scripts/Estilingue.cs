using System;
using UnityEngine;

// Lê o mouse: clicar perto da raposa, arrastar para trás e soltar.
// A velocidade de saída é proporcional ao quanto o elástico foi esticado
// (Regras.VelocidadeDeLancamento), como uma mola.
public class Estilingue : MonoBehaviour
{
    [Tooltip("Ponto de descanso da raposa entre as pontas do garfo.")]
    public Transform ancora;
    public Transform pontaTras, pontaFrente;
    public LineRenderer elasticoTras, elasticoFrente;
    public Trajetoria trajetoria;

    [Tooltip("Quanto o elástico estica, em unidades.")]
    public float puxadaMaxima = 1.5f;

    [Tooltip("Velocidade de saída por unidade esticada.")]
    public float forca = 8.5f;

    [Tooltip("Distância do clique até a raposa para conseguir pegá-la.")]
    public float raioPegar = 1.2f;

    public Vector2 Ancora => ancora.position;
    public bool Carregado => raposa != null;
    public bool Pronto => raposa != null && raposa.Estado == EstadoRaposa.NoEstilingue;
    public event Action<Raposa> Lancou;

    Raposa raposa;
    bool arrastando;
    Camera cam;

    void Awake()
    {
        cam = Camera.main;
    }

    public void Carregar(Raposa nova)
    {
        raposa = nova;
        arrastando = false;
    }

    // Tira a raposa do estilingue sem lançar (usado no fim da fase).
    public Raposa Descarregar()
    {
        var r = raposa;
        raposa = null;
        arrastando = false;
        if (trajetoria != null) trajetoria.Esconder();
        return r;
    }

    void Update()
    {
        if (Pronto)
        {
            Vector2 mouse = cam.ScreenToWorldPoint(Input.mousePosition);

            if (!arrastando && Input.GetMouseButtonDown(0) && !Raposa.PonteiroNaInterface()
                && Vector2.Distance(mouse, Ancora) <= raioPegar)
            {
                arrastando = true;
                Sons.Tocar(Sons.I != null ? Sons.I.esticar : null, 0.7f);
            }

            if (arrastando)
                Arrastar(mouse);
        }

        AtualizarElasticos();
    }

    void Arrastar(Vector2 mouse)
    {
        Vector2 puxada = Regras.LimitarPuxada(Ancora, mouse, puxadaMaxima);
        raposa.transform.position = puxada;

        bool valida = Regras.PuxadaValida(Ancora, puxada);
        if (valida) trajetoria.Mostrar(puxada, Regras.VelocidadeDeLancamento(Ancora, puxada, puxadaMaxima, forca));
        else trajetoria.Esconder();

        if (Input.GetMouseButtonUp(0))
        {
            arrastando = false;
            trajetoria.Esconder();

            if (valida) Disparar(puxada);
            else raposa.transform.position = Ancora; // clique sem puxar: volta pro lugar
        }
    }

    // Atalho para testes e depuração: lança como se o jogador tivesse puxado
    // "puxadaRelativa" a partir da âncora (ex.: (-1.5, -0.5) = para trás e para baixo).
    public void LancarCom(Vector2 puxadaRelativa)
    {
        if (!Pronto) return;
        Vector2 puxada = Regras.LimitarPuxada(Ancora, Ancora + puxadaRelativa, puxadaMaxima);
        raposa.transform.position = puxada;
        Disparar(puxada);
    }

    void Disparar(Vector2 puxada)
    {
        var lancada = raposa;
        raposa = null;
        lancada.Lancar(Regras.VelocidadeDeLancamento(Ancora, puxada, puxadaMaxima, forca));
        Sons.Tocar(Sons.I != null ? Sons.I.lancar : null);
        Lancou?.Invoke(lancada);
    }

    void AtualizarElasticos()
    {
        // Com raposa: os dois elásticos vão das pontas até ela.
        // Sem raposa: ficam esticados entre as pontas, em repouso.
        bool segurando = raposa != null && raposa.Estado == EstadoRaposa.NoEstilingue;
        Vector3 destino = segurando ? raposa.transform.position : ancora.position;
        destino.z = 0f;

        elasticoTras.SetPosition(0, pontaTras.position);
        elasticoTras.SetPosition(1, destino);
        elasticoFrente.SetPosition(0, pontaFrente.position);
        elasticoFrente.SetPosition(1, destino);
    }
}
