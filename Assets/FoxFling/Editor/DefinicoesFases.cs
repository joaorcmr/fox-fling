using System.Collections.Generic;
using UnityEngine;

public enum TipoPeca { Bloco, Inimigo, InimigoGrande }

public enum TipoMaterial { Madeira, Vidro, Rocha }

public struct Peca
{
    public TipoPeca tipo;
    public TipoMaterial material;
    public Vector2 centro;
    public Vector2 tamanho;

    public float Base => centro.y - tamanho.y / 2f;
    public float Topo => centro.y + tamanho.y / 2f;
    public float Esquerda => centro.x - tamanho.x / 2f;
    public float Direita => centro.x + tamanho.x / 2f;
}

public class DefinicaoFase
{
    public int numero;
    public string nome;
    public string dificuldade;
    public TipoRaposa[] raposas;
    public List<Peca> pecas = new List<Peca>();
}

// Os mapas do jogo, escritos como "plantas" de construção. O ConstrutorCenas
// transforma cada definição numa cena (Fase1..Fase5). Para mudar um mapa: edite
// aqui e use o menu "Fox Fling > Reconstruir tudo" (ou edite a cena direto no Unity).
//
// Coordenadas em unidades do mundo: o chão fica em y = -4.5 e a área das
// estruturas vai de x = 0 até x = 7.4. Cada peça é dada pela BASE (onde ela apoia).
public static class DefinicoesFases
{
    public const float Chao = -4.5f;
    public const float Pilar = 0.25f;   // largura de um pilar
    public const float Viga = 0.25f;    // espessura de uma viga
    public static readonly Vector2 TamanhoInimigo = new Vector2(0.8f, 0.7f);
    public const float EscalaGrande = 1.4f;

    const TipoMaterial Madeira = TipoMaterial.Madeira, Vidro = TipoMaterial.Vidro, Rocha = TipoMaterial.Rocha;

    static readonly TipoRaposa N = TipoRaposa.Normal, Ni = TipoRaposa.Ninja, B = TipoRaposa.Bomba;

    public static DefinicaoFase[] Todas()
    {
        return new[]
        {
            Fase1(), Fase2(), Fase3(), Fase4(), Fase5(),
        };
    }

    // Fácil: a torre do desenho, toda de madeira, um tubarão em cada andar.
    static DefinicaoFase Fase1()
    {
        var p = new Planta();
        float andar1 = p.Andar(Madeira, Madeira, 2.6f, 4.4f, Chao, 1.4f);
        p.Inimigo(3.5f, Chao);
        float andar2 = p.Andar(Madeira, Madeira, 2.85f, 4.15f, andar1, 1.2f);
        p.Inimigo(3.5f, andar1);
        p.Bloco(Madeira, 3.1f, andar2, 0.4f, 0.4f);
        p.Bloco(Madeira, 3.9f, andar2, 0.4f, 0.4f);

        return new DefinicaoFase
        {
            numero = 1, nome = "Fase 1", dificuldade = "Fácil",
            raposas = new[] { N, N },
            pecas = p.pecas,
        };
    }

    // Médio: uma casinha de vidro (frágil) e uma torre de madeira com andar de vidro.
    static DefinicaoFase Fase2()
    {
        var p = new Planta();
        float teto = p.Andar(Vidro, Madeira, 1.2f, 2.6f, Chao, 1.0f);
        p.Inimigo(1.9f, Chao);
        p.Inimigo(1.9f, teto);

        float andar1 = p.Andar(Madeira, Madeira, 4.4f, 6.0f, Chao, 1.2f);
        p.Bloco(Vidro, 5.2f, Chao, 0.6f, 0.6f);
        float andar2 = p.Andar(Vidro, Madeira, 4.6f, 5.8f, andar1, 1.1f);
        p.Inimigo(5.2f, andar1);
        p.Bloco(Madeira, 4.8f, andar2, 0.4f, 0.4f);
        p.Bloco(Madeira, 5.6f, andar2, 0.4f, 0.4f);

        return new DefinicaoFase
        {
            numero = 2, nome = "Fase 2", dificuldade = "Médio",
            raposas = new[] { N, N, Ni },
            pecas = p.pecas,
        };
    }

    // Médio: mesa de rocha (difícil de quebrar) com uma torre de madeira em cima
    // e um tubarão lá no alto.
    static DefinicaoFase Fase3()
    {
        var p = new Planta();
        float mesa = p.Andar(Rocha, Rocha, 2.4f, 5.0f, Chao, 1.2f, larguraPilar: 0.4f, espessuraViga: 0.3f);
        p.Inimigo(3.7f, Chao);
        float meio = p.Andar(Madeira, Madeira, 2.9f, 4.5f, mesa, 1.3f);
        p.Inimigo(3.7f, mesa);
        p.Andar(Madeira, Madeira, 3.15f, 4.25f, meio, 0.9f);
        p.Inimigo(3.7f, meio);
        p.Bloco(Rocha, 6.6f, Chao, 0.4f, 1.0f);

        return new DefinicaoFase
        {
            numero = 3, nome = "Fase 3", dificuldade = "Médio",
            raposas = new[] { N, N, Ni },
            pecas = p.pecas,
        };
    }

    // Difícil: fortaleza com paredes de rocha, dois quartos no térreo (um com o
    // tubarão grande) e um andar de vidro em cima. Uma pedra no topo esmaga tudo se cair.
    static DefinicaoFase Fase4()
    {
        var p = new Planta();
        p.Bloco(Rocha, 0.6f, Chao, 0.4f, 1.2f); // muro da frente

        foreach (float x in new[] { 1.6f, 3.9f, 6.2f })
            p.Bloco(Rocha, x, Chao, 0.4f, 1.4f);
        float terreo = Chao + 1.4f;
        p.Bloco(Madeira, 3.9f, terreo, 5.3f, Viga);
        terreo += Viga;
        p.Inimigo(2.75f, Chao, grande: true);
        p.Inimigo(5.05f, Chao);
        p.Bloco(Vidro, 5.75f, Chao, 0.5f, 0.5f);

        float topo = p.Andar(Vidro, Madeira, 2.3f, 5.2f, terreo, 1.2f);
        p.Inimigo(3.2f, terreo);
        p.Inimigo(4.4f, terreo);
        p.Bloco(Rocha, 3.75f, topo, 0.6f, 0.6f);

        return new DefinicaoFase
        {
            numero = 4, nome = "Fase 4", dificuldade = "Difícil",
            raposas = new[] { N, N, Ni, B },
            pecas = p.pecas,
        };
    }

    // Desafio: só 3 raposas contra 5 tubarões em duas fortalezas mistas.
    static DefinicaoFase Fase5()
    {
        var p = new Planta();
        float laje = p.Andar(Rocha, Rocha, 1.0f, 2.4f, Chao, 1.8f, larguraPilar: 0.4f, espessuraViga: 0.3f);
        p.Inimigo(1.7f, Chao);
        p.Andar(Vidro, Madeira, 1.1f, 2.3f, laje, 1.0f);
        p.Inimigo(1.7f, laje);

        foreach (float x in new[] { 4.3f, 5.6f, 6.9f })
            p.Bloco(Rocha, x, Chao, 0.4f, 1.2f);
        float terreo = Chao + 1.2f;
        p.Bloco(Madeira, 5.6f, terreo, 3.3f, Viga);
        terreo += Viga;
        p.Inimigo(4.95f, Chao);
        p.Inimigo(6.25f, Chao);
        p.Andar(Vidro, Rocha, 4.6f, 6.6f, terreo, 1.0f, espessuraViga: 0.3f);
        p.Inimigo(5.6f, terreo, grande: true);

        return new DefinicaoFase
        {
            numero = 5, nome = "Fase 5", dificuldade = "Desafio",
            raposas = new[] { N, Ni, B },
            pecas = p.pecas,
        };
    }

    // Ajudante para montar estruturas sem fazer conta de centro na mão.
    class Planta
    {
        public readonly List<Peca> pecas = new List<Peca>();

        public Planta Bloco(TipoMaterial m, float x, float yBase, float largura, float altura)
        {
            pecas.Add(new Peca
            {
                tipo = TipoPeca.Bloco, material = m,
                centro = new Vector2(x, yBase + altura / 2f),
                tamanho = new Vector2(largura, altura),
            });
            return this;
        }

        public Planta Inimigo(float x, float yBase, bool grande = false)
        {
            Vector2 t = TamanhoInimigo * (grande ? EscalaGrande : 1f);
            pecas.Add(new Peca
            {
                tipo = grande ? TipoPeca.InimigoGrande : TipoPeca.Inimigo,
                centro = new Vector2(x, yBase + t.y / 2f),
                tamanho = t,
            });
            return this;
        }

        // Dois pilares (centros em xEsq e xDir) com uma viga por cima que passa
        // um pouco das bordas. Devolve a altura do topo da viga (base do próximo andar).
        public float Andar(TipoMaterial pilar, TipoMaterial viga, float xEsq, float xDir, float yBase, float altura,
            float sobra = 0.15f, float larguraPilar = Pilar, float espessuraViga = Viga)
        {
            Bloco(pilar, xEsq, yBase, larguraPilar, altura);
            Bloco(pilar, xDir, yBase, larguraPilar, altura);
            float esquerda = xEsq - larguraPilar / 2f - sobra;
            float direita = xDir + larguraPilar / 2f + sobra;
            Bloco(viga, (esquerda + direita) / 2f, yBase + altura, direita - esquerda, espessuraViga);
            return yBase + altura + espessuraViga;
        }
    }
}
