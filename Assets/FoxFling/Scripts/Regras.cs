using UnityEngine;

public enum EstadoFase { Jogando, Vitoria, Derrota }

// Regras "puras" do jogo: só contas, sem GameObject nem cena.
// Ficam separadas para dar pra testar (Tests/EditMode/RegrasTests.cs) e ajustar num lugar só.
public static class Regras
{
    // Nos primeiros instantes da fase a física "acomoda" as peças; impactos nesse
    // tempo não causam dano, senão a torre se quebraria sozinha ao começar.
    public const float TempoAcomodacao = 1f;

    // Arrastar menos que isso conta como clique sem querer (não gasta a raposa).
    public const float PuxadaMinima = 0.25f;

    public const float LimiteEstrela2 = 0.45f;
    public const float LimiteEstrela3 = 0.75f;
    public const int PontosInimigo = 5000;
    public const int BonusRaposa = 10000;

    public static int Estrelas(int pontos, int pontuacaoMaxima, bool venceu)
    {
        if (!venceu) return 0;
        if (pontuacaoMaxima <= 0) return 3;

        float fracao = (float)pontos / pontuacaoMaxima;
        if (fracao >= LimiteEstrela3) return 3;
        if (fracao >= LimiteEstrela2) return 2;
        return 1;
    }

    public static int PontuacaoMaxima(int pontosDestrutiveis, int quantidadeRaposas)
    {
        return pontosDestrutiveis + Mathf.Max(0, quantidadeRaposas - 1) * BonusRaposa;
    }

    // Impulso = quanto a colisão mudou o movimento (massa × variação de velocidade).
    // Batida forte → impulso grande → mais dano. Abaixo do mínimo é só "encostar".
    public static float DanoDoImpulso(float impulso, float impulsoMinimo, float multiplicador)
    {
        if (impulso < impulsoMinimo) return 0f;
        return impulso * multiplicador;
    }

    public static Vector2 LimitarPuxada(Vector2 ancora, Vector2 alvo, float puxadaMaxima)
    {
        return ancora + Vector2.ClampMagnitude(alvo - ancora, puxadaMaxima);
    }

    public static bool PuxadaValida(Vector2 ancora, Vector2 puxada)
    {
        return (puxada - ancora).magnitude >= PuxadaMinima;
    }

    // Como um elástico: a raposa sai para o lado oposto ao que foi puxada,
    // mais rápido quanto mais esticado (até o limite do elástico).
    public static Vector2 VelocidadeDeLancamento(Vector2 ancora, Vector2 puxada, float puxadaMaxima, float forca)
    {
        Vector2 esticado = LimitarPuxada(ancora, puxada, puxadaMaxima) - ancora;
        return -esticado * forca;
    }

    // Equação do movimento com aceleração constante: p = p0 + v·t + ½·g·t²
    public static Vector2 PosicaoNaTrajetoria(Vector2 inicio, Vector2 velocidade, Vector2 gravidade, float t)
    {
        return inicio + velocidade * t + 0.5f * gravidade * t * t;
    }

    // 1 no centro da explosão, caindo em linha reta até 0 na borda do raio.
    public static float FatorExplosao(float distancia, float raio)
    {
        if (raio <= 0f) return 0f;
        return Mathf.Clamp01(1f - distancia / raio);
    }

    // A tela vai de x = -7.5 a 7.5 e o chão acaba nas bordas: quem passa daqui caiu do mapa.
    // Não há limite para cima: a raposa pode voar alto e voltar.
    public static bool ForaDoMundo(Vector2 p)
    {
        return p.x < -9f || p.x > 9f || p.y < -7f;
    }

    // "12.500" — separador de milhar brasileiro, sem depender da língua do computador.
    public static string FormatarPontos(int pontos)
    {
        return pontos.ToString("#,0", System.Globalization.CultureInfo.InvariantCulture).Replace(',', '.');
    }

    // Regra do tower defense: os tubarões são o jogador e as raposas atacam a torre.
    //  - Derrota: o último tubarão morreu (mesmo com raposas ainda por vir).
    //  - Vitória: a leva inteira acabou (ninguém por nascer nem em campo), o mundo parou
    //    de se mexer (pedra caindo ainda pode matar alguém) e sobrou pelo menos 1 tubarão.
    public static EstadoFase EstadoAposOnda(int tubaroesVivos, int raposasPorNascer, int raposasEmCampo, bool mundoParado)
    {
        if (tubaroesVivos <= 0) return EstadoFase.Derrota;
        if (raposasPorNascer > 0 || raposasEmCampo > 0 || !mundoParado) return EstadoFase.Jogando;
        return EstadoFase.Vitoria;
    }

    // Estrelas pelos tubarões que sobraram: todos = 3, metade ou mais = 2, algum = 1.
    public static int EstrelasPorSobreviventes(int tubaroesVivos, int tubaroesNoInicio, bool venceu)
    {
        if (!venceu || tubaroesVivos <= 0) return 0;
        if (tubaroesVivos >= tubaroesNoInicio) return 3;
        if (tubaroesVivos * 2 >= tubaroesNoInicio) return 2;
        return 1;
    }

    public static EstadoFase EstadoAposRaposa(int inimigosVivos, int raposasRestantes, bool mundoParado)
    {
        if (inimigosVivos <= 0) return EstadoFase.Vitoria;
        if (raposasRestantes > 0 || !mundoParado) return EstadoFase.Jogando;
        return EstadoFase.Derrota;
    }
}
