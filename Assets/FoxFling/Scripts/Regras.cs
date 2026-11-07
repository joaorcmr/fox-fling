using UnityEngine;

// Preparando: entre as levas, com a loja aberta (as raposas só entram quando o jogador manda).
public enum EstadoFase { Jogando, Vitoria, Derrota, Preparando }

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

    // ---- Pontos da leva (issue #4) ----
    // Ao fim de cada leva o jogador ganha: base da leva (sobe com o nível) + pontos das
    // raposas derrotadas − uma penalidade por cada tubarão que as raposas mataram. Nunca
    // fica abaixo de zero. O resultado vai para o saldo da loja.
    public const int PenalidadePorTubarao = 300;

    public static int BaseDaLeva(int nivel) => 150 + 25 * nivel;

    public static int PontosDaLeva(int pontosDasRaposas, int tubaroesPerdidos, int nivel)
    {
        int bruto = BaseDaLeva(nivel) + pontosDasRaposas - PenalidadePorTubarao * Mathf.Max(0, tubaroesPerdidos);
        return Mathf.Max(0, bruto);
    }

    // ---- Colocar coisas no chão (issues #3 e #7) ----
    // Muros e armas são colocados no chão, em posições de uma grade, dentro desta faixa:
    // do lado esquerdo (por onde as raposas chegam) até o fim da área das estruturas.
    public const float PassoDaColocacao = 0.25f;
    public const float ColocacaoXMin = -6f, ColocacaoXMax = 7.4f;

    // O x onde o item fica: o do mouse "grudado" na grade e sem sair da faixa (a largura
    // conta para o item inteiro caber).
    public static float XDaColocacao(float xDoMouse, float largura)
    {
        float x = Mathf.Round(xDoMouse / PassoDaColocacao) * PassoDaColocacao;
        return Mathf.Clamp(x, ColocacaoXMin + largura / 2f, ColocacaoXMax - largura / 2f);
    }

    // ---- Níveis e levas (issue #5) ----
    // Cada fase tem LevasPorFase levas seguidas. O "nível" conta as levas de todas as
    // fases (fase 1 leva 1 = nível 1; fase 5 leva 3 = nível 15): quanto maior, mais raposas
    // e mais tipos fortes entre elas.
    public const int LevasPorFase = 3;

    public static int NivelDaLeva(int fase, int leva) => (fase - 1) * LevasPorFase + leva;

    public static int QuantidadeDeRaposas(int nivel) => 1 + nivel;

    // Nos níveis mais altos as raposas chegam mais seguidas.
    public static float IntervaloEntreRaposas(int nivel) => Mathf.Max(0.7f, 1.6f - 0.06f * nivel);

    // A leva em ordem de entrada: primeiro as normais, depois ninjas, grandes e bombas
    // (a leva "cresce" até o fim). Cada tipo especial aparece a partir de um nível e sua
    // fração sobe por nível até um teto; a fração das normais é o que sobra.
    public static TipoRaposa[] ComporLeva(int fase, int leva)
    {
        int nivel = Mathf.Max(1, NivelDaLeva(fase, leva));
        int total = QuantidadeDeRaposas(nivel);

        int ninjas = QuantosDoTipo(total, nivel, 2, 0.04f, 0.30f);
        int grandes = QuantosDoTipo(total, nivel, 6, 0.02f, 0.20f);
        int bombas = QuantosDoTipo(total, nivel, 4, 0.025f, 0.20f);

        // Sempre sobra pelo menos uma normal.
        while (ninjas + grandes + bombas > total - 1)
        {
            if (ninjas >= grandes && ninjas >= bombas) ninjas--;
            else if (bombas >= grandes) bombas--;
            else grandes--;
        }

        var composicao = new TipoRaposa[total];
        int i = 0;
        int normais = total - ninjas - grandes - bombas;
        for (int k = 0; k < normais; k++) composicao[i++] = TipoRaposa.Normal;
        for (int k = 0; k < ninjas; k++) composicao[i++] = TipoRaposa.Ninja;
        for (int k = 0; k < grandes; k++) composicao[i++] = TipoRaposa.Grande;
        for (int k = 0; k < bombas; k++) composicao[i++] = TipoRaposa.Bomba;
        return composicao;
    }

    // 0 antes do primeiro nível do tipo; depois, pelo menos 1 e a fração crescente (com teto).
    static int QuantosDoTipo(int total, int nivel, int primeiroNivel, float fracaoPorNivel, float teto)
    {
        if (nivel < primeiroNivel) return 0;
        float fracao = Mathf.Min(teto, fracaoPorNivel * (nivel - primeiroNivel + 1));
        return Mathf.Max(1, Mathf.RoundToInt(total * fracao));
    }
}
