using System.Collections.Generic;
using UnityEngine;
using static EspeciePassaro;

// Os números PADRÃO do jogo, de onde nascem os assets de Assets/Revoada/Dados.
// Depois de criados, os assets são a fonte da verdade: o que o João mudar no
// Inspector fica valendo. "Revoada > Restaurar números padrão" volta para cá.
// As ondas são o ponto de partida do balanceamento (ajustado pelos robôs).
public static class DefinicoesPadrao
{
    public static DadosPassaro[] Passaros()
    {
        return new[]
        {
            P(Pardal, "Pardal", vida: 1, vel: 2.5f, dano: 1, penas: 1, raio: 0.35f),
            P(Sabia, "Sabiá", vida: 3, vel: 2.8f, dano: 2, penas: 2, raio: 0.35f),
            P(BeijaFlor, "Beija-flor", vida: 2, vel: 5f, dano: 2, penas: 3, raio: 0.35f),
            P(Arara, "Arara-azul", vida: 10, vel: 2f, dano: 4, penas: 6, raio: 0.45f),
            Com(P(Pata, "Pata", vida: 8, vel: 1.8f, dano: 3, penas: 4, raio: 0.45f), p => { p.filho = Patinho; p.quantidadeFilhos = 4; }),
            P(Patinho, "Patinho", vida: 1, vel: 3f, dano: 1, penas: 1, raio: 0.3f),
            Com(P(Urubu, "Urubu", vida: 28, vel: 1.4f, dano: 6, penas: 12, raio: 0.45f), p => p.couraca = true),
            Com(P(Gaviao, "Gavião-real", vida: 250, vel: 0.9f, dano: 30, penas: 100, raio: 0.8f), p => p.chefe = true),
        };
    }

    // Os porcos entram na Task 5 (com o combate).
    public static DadosPorco[] Porcos() => new DadosPorco[0];

    public static List<DadosOnda> Ondas()
    {
        return new List<DadosOnda>
        {
            // 1–5: pardais e sabiás (aprender a comprar e posicionar)
            O(G(Pardal, 12, 0.9f)),
            O(G(Pardal, 18, 0.7f)),
            O(G(Pardal, 15, 0.6f), G(Sabia, 5, 1.2f, 4f)),
            O(G(Pardal, 20, 0.5f), G(Sabia, 8, 1f, 5f)),
            O(G(Sabia, 14, 0.8f), G(Pardal, 16, 0.35f, 3f)),
            // 6–9: beija-flores (rápidos) e as primeiras araras
            O(G(BeijaFlor, 6, 1.5f), G(Sabia, 18, 0.7f, 2f)),
            O(G(Pardal, 30, 0.25f), G(BeijaFlor, 10, 0.8f, 6f)),
            O(G(Arara, 3, 2.5f), G(Sabia, 20, 0.6f, 1f)),
            O(G(Arara, 6, 1.8f), G(BeijaFlor, 12, 0.7f, 3f)),
            // 10: chefe 1
            O(G(Sabia, 20, 0.8f), G(Gaviao, 1, 1f, 4f, 1f)),
            // 11–14: patas (se dividem) e bandos maiores
            O(G(Arara, 10, 1.2f), G(Pardal, 30, 0.3f, 2f)),
            O(G(Pata, 4, 2.5f), G(Sabia, 15, 0.6f, 1f)),
            O(G(BeijaFlor, 15, 0.5f), G(Arara, 8, 1.2f, 4f)),
            O(G(Pata, 8, 1.6f), G(Arara, 10, 1.2f, 2f)),
            // 15–19: urubus (couraça); a 18 é uma revoada densa
            O(G(Urubu, 2, 4f), G(Sabia, 20, 0.5f, 1f)),
            O(G(Urubu, 5, 2.5f), G(BeijaFlor, 25, 0.45f, 2f)),
            O(G(Pata, 12, 1.2f), G(Arara, 12, 1f, 3f)),
            O(G(Pardal, 80, 0.12f), G(BeijaFlor, 20, 0.35f, 4f)),
            O(G(Urubu, 8, 1.8f), G(Pata, 10, 1.4f, 3f)),
            // 20: chefe 2
            O(G(Urubu, 4, 3f), G(Gaviao, 1, 1f, 3f, 2.8f)),
            // 21–29: misturas densas
            O(G(Arara, 25, 0.6f), G(Sabia, 30, 0.3f, 2f)),
            O(G(Urubu, 10, 1.2f), G(BeijaFlor, 30, 0.35f, 3f)),
            O(G(Pata, 16, 0.8f), G(Arara, 20, 0.7f, 2f)),
            O(G(Urubu, 14, 1f), G(Pardal, 40, 0.15f, 4f)),
            O(G(Sabia, 60, 0.15f), G(BeijaFlor, 40, 0.2f, 3f)),
            O(G(Urubu, 18, 0.8f), G(Pata, 12, 1f, 4f)),
            O(G(Arara, 30, 0.4f), G(Pata, 20, 0.6f, 3f)),
            O(G(Urubu, 22, 0.6f), G(BeijaFlor, 50, 0.2f, 3f)),
            O(G(Urubu, 25, 0.5f), G(Pata, 25, 0.5f, 2f), G(Arara, 30, 0.4f, 6f)),
            // 30: chefe final
            O(G(Urubu, 20, 0.8f), G(Arara, 30, 0.5f, 5f), G(Gaviao, 1, 1f, 8f, 6f)),
        };
    }

    public static DadosEconomia Economia()
    {
        return new DadosEconomia
        {
            penasIniciais = 300,
            vidaInicial = 100,
            bonusOndaBase = 25,
            bonusOndaPorOnda = 5,
            fracaoVenda = 0.7f,
            esperaAuto = 2f,
            ninho = new List<NivelNinho>
            {
                new NivelNinho { multiplicador = 1.25f, custo = 250 },
                new NivelNinho { multiplicador = 1.5f, custo = 500 },
                new NivelNinho { multiplicador = 1.75f, custo = 900 },
                new NivelNinho { multiplicador = 2f, custo = 1500 },
                new NivelNinho { multiplicador = 2.5f, custo = 2500 },
            },
        };
    }

    // A Fazenda: uma serpentina que entra pela esquerda, no alto, e desce até a
    // Torre. A reta do alto (y = 13) é a mais longa: o lugar do Arpoador.
    public static DadosMapa Fazenda()
    {
        return new DadosMapa
        {
            nome = "Fazenda",
            tamanho = new Vector2(14f, 15f),
            pontos = new List<Vector2>
            {
                new Vector2(-1f, 13f), new Vector2(12.5f, 13f), new Vector2(12.5f, 9.5f), new Vector2(1.5f, 9.5f),
                new Vector2(1.5f, 6f), new Vector2(12.5f, 6f), new Vector2(12.5f, 2.5f), new Vector2(3.6f, 2.5f),
            },
            raioCanto = 1f,
            meiaLargura = 0.6f,
            torre = new Vector2(2.4f, 2.5f),
            raioTorre = 1.1f,
            obstaculos = new List<Obstaculo>
            {
                Ob("lago", 7f, 7.75f, 0.9f),
                Ob("arvore", 3.6f, 11.25f, 0.55f), Ob("arvore", 10.4f, 4.25f, 0.55f), Ob("arvore", 0.7f, 4.3f, 0.5f),
                Ob("arvore", 13.3f, 0.8f, 0.5f), Ob("arvore", 7f, 0.9f, 0.5f), Ob("arvore", 8.5f, 14.35f, 0.45f),
                Ob("pedra", 9.4f, 11.3f, 0.35f), Ob("pedra", 4.6f, 7.8f, 0.35f), Ob("pedra", 0.8f, 0.9f, 0.4f),
            },
        };
    }

    public static DadosJogo Jogo()
    {
        var d = new DadosJogo
        {
            ondas = Ondas(),
            economia = Economia(),
            mapa = Fazenda(),
        };
        foreach (var p in Passaros()) d.passaros[p.especie] = p;
        foreach (var p in Porcos()) d.porcos[p.especie] = p;
        return d;
    }

    // ------------------------------------------------------------------
    static DadosPassaro P(EspeciePassaro e, string nome, float vida, float vel, int dano, float penas, float raio)
    {
        return new DadosPassaro { especie = e, nome = nome, vida = vida, velocidade = vel, danoTorre = dano, penas = penas, raio = raio };
    }

    static DadosPassaro Com(DadosPassaro p, System.Action<DadosPassaro> ajuste)
    {
        ajuste(p);
        return p;
    }

    static GrupoOnda G(EspeciePassaro e, int quantidade, float intervalo, float atraso = 0f, float vida = 1f)
    {
        return new GrupoOnda { especie = e, quantidade = quantidade, intervalo = intervalo, atraso = atraso, multiplicadorVida = vida };
    }

    static DadosOnda O(params GrupoOnda[] grupos) => new DadosOnda { grupos = new List<GrupoOnda>(grupos) };

    static Obstaculo Ob(string tipo, float x, float y, float raio) => new Obstaculo { tipo = tipo, centro = new Vector2(x, y), raio = raio };
}
