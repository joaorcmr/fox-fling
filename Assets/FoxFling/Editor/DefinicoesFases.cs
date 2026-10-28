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

    const TipoMaterial Madeira = TipoMaterial.Madeira;

    static readonly TipoRaposa N = TipoRaposa.Normal;

    public static DefinicaoFase[] Todas()
    {
        return new[]
        {
            Fase1(),
        };
    }

    // Fácil: a torre do desenho, toda de madeira, um triângulo em cada andar.
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
        public float Andar(TipoMaterial pilar, TipoMaterial viga, float xEsq, float xDir, float yBase, float altura, float sobra = 0.15f)
        {
            Bloco(pilar, xEsq, yBase, Pilar, altura);
            Bloco(pilar, xDir, yBase, Pilar, altura);
            float esquerda = xEsq - Pilar / 2f - sobra;
            float direita = xDir + Pilar / 2f + sobra;
            Bloco(viga, (esquerda + direita) / 2f, yBase + altura, direita - esquerda, Viga);
            return yBase + altura + Viga;
        }
    }
}
