using System.Collections.Generic;
using UnityEngine;
using static EspeciePassaro;

// Os números PADRÃO do jogo, de onde nascem os assets das pastas Dados de cada assunto.
// Depois de criados, os assets são a fonte da verdade: o que o João mudar no
// Inspector fica valendo. "Revoada > Restaurar números padrão" volta para cá.
// As ondas são o ponto de partida do balanceamento (ajustado pelos robôs).
public static class DefinicoesPadrao
{
    public static DadosPassaro[] Passaros()
    {
        return new[]
        {
            // Penas: o dobro do ponto de partida do design (os robôs mostraram que,
            // com 1 pena por pardal, o Ninho nunca se pagava). Chefe: 90 de vida
            // base (250 era mais do que qualquer defesa da onda 10 aguenta).
            P(Pardal, "Pardal", vida: 1, vel: 2.5f, dano: 1, penas: 2, raio: 0.35f),
            P(Sabia, "Sabiá", vida: 3, vel: 2.8f, dano: 2, penas: 4, raio: 0.35f),
            P(BeijaFlor, "Beija-flor", vida: 2, vel: 5f, dano: 2, penas: 6, raio: 0.35f),
            P(Arara, "Arara-azul", vida: 10, vel: 2f, dano: 4, penas: 12, raio: 0.45f),
            Com(P(Pata, "Pata", vida: 8, vel: 1.8f, dano: 3, penas: 8, raio: 0.45f), p => { p.filho = Patinho; p.quantidadeFilhos = 4; }),
            P(Patinho, "Patinho", vida: 1, vel: 3f, dano: 1, penas: 2, raio: 0.3f),
            Com(P(Urubu, "Urubu", vida: 28, vel: 1.4f, dano: 6, penas: 24, raio: 0.45f), p => p.couraca = true),
            Com(P(Gaviao, "Gavião-real", vida: 90, vel: 0.9f, dano: 30, penas: 200, raio: 0.8f), p => p.chefe = true),
        };
    }

    // Tabelas "Porcos" e "Upgrades" do design. Cada melhoria guarda o bloco
    // COMPLETO: o do nível anterior com o efeito dela por cima (Mais(...)).
    public static DadosPorco[] Porcos()
    {
        var estilingue1 = new Atributos
        {
            tiro = TipoTiro.Pedrinha, dano = 1f, alcance = 3f, recarga = 0.8f,
            velocidadeProjetil = 12f, raioProjetil = 0.15f, perfura = 1, alcanceVoo = 1.3f,
        };
        var estilingue2 = Mais(estilingue1, a => a.recarga = 0.55f);
        var estilingue3 = Mais(estilingue2, a => { a.dano = 2f; a.alcance = 3.5f; });

        var arpao1 = new Atributos
        {
            tiro = TipoTiro.Arpao, dano = 2f, alcance = 5f, recarga = 1.6f,
            velocidadeProjetil = 14f, raioProjetil = 0.18f, perfura = 4, alcanceVoo = 1.6f,
        };
        var arpao2 = Mais(arpao1, a => { a.perfura = 7; a.alcanceVoo = 2f; });
        var arpao3 = Mais(arpao2, a => { a.dano = 3f; a.ignoraCouraca = true; });

        var bomba1 = new Atributos
        {
            tiro = TipoTiro.Bomba, dano = 2f, alcance = 3.5f, recarga = 1.5f,
            velocidadeProjetil = 8f, raioProjetil = 0.2f, perfura = 1, alcanceVoo = 1f, raioArea = 1f,
        };
        var bomba2 = Mais(bomba1, a => a.raioArea = 1.4f);
        var bomba3 = Mais(bomba2, a => { a.lentidao = 0.4f; a.duracaoLentidao = 1.5f; });

        // O Tamboreiro não atira: o "alcance" dele é o raio da aura (é o que o círculo mostra).
        var tambor1 = new Atributos { tiro = TipoTiro.Nenhum, alcance = 2.5f, raioAura = 2.5f, bonusCadencia = 0.2f };
        var tambor2 = Mais(tambor1, a => { a.bonusCadencia = 0.35f; a.raioAura = 3f; a.alcance = 3f; });
        var tambor3 = Mais(tambor2, a => a.bonusAlcance = 0.2f);

        return new[]
        {
            new DadosPorco
            {
                especie = EspeciePorco.Estilingueiro, nome = "Estilingueiro", papel = "Barato e rápido: segura o começo.", custo = 120,
                nivel1 = estilingue1,
                nivel2 = M("Elástico Reforçado", "Atira mais rápido (recarga 0,55 s).", 100, estilingue2),
                nivel3 = M("Pedra Afiada", "Dano 2 e alcance 3,5.", 180, estilingue3),
                especializacaoA = M("Rajada", "Solta 3 pedrinhas em leque.", 500, Mais(estilingue3, a => { a.tiros = 3; a.abertura = 15f; })),
                especializacaoB = M("Mira Certeira", "Alcance 5,5, dano 4, atravessa 2.", 550,
                    Mais(estilingue3, a => { a.alcance = 5.5f; a.dano = 4f; a.perfura = 2; })),
            },
            new DadosPorco
            {
                especie = EspeciePorco.Arpoador, nome = "Arpoador", papel = "O arpão atravessa a fila: rende nas retas longas.", custo = 220,
                nivel1 = arpao1,
                nivel2 = M("Arpão Comprido", "Atravessa 7 e voa até 2× o alcance.", 160, arpao2),
                nivel3 = M("Ponta de Ferro", "Dano 3 e ignora a couraça.", 260, arpao3),
                especializacaoA = M("Arpão Gigante", "Atravessa 20, dano 6, arpão mais largo.", 750,
                    Mais(arpao3, a => { a.perfura = 20; a.dano = 6f; a.raioProjetil = 0.35f; })),
                especializacaoB = M("Arpão Ligeiro", "Atira muito mais rápido (recarga 0,6 s).", 650, Mais(arpao3, a => a.recarga = 0.6f)),
            },
            new DadosPorco
            {
                especie = EspeciePorco.Bombardeiro, nome = "Bombardeiro", papel = "Bomba de lama em área: bandos, curvas e couraça.", custo = 350,
                nivel1 = bomba1,
                nivel2 = M("Barril Maior", "Explosão maior (raio 1,4).", 220, bomba2),
                nivel3 = M("Lama Grudenta", "Atingidos ficam 40% mais lentos por 1,5 s.", 300, bomba3),
                especializacaoA = M("Chuva de Lama", "Atira o dobro (recarga 0,7 s).", 850, Mais(bomba3, a => a.recarga = 0.7f)),
                especializacaoB = M("Barril de Estrume", "Dano 7, raio 1,8 e empurra os pássaros para trás.", 800,
                    Mais(bomba3, a => { a.dano = 7f; a.raioArea = 1.8f; a.empurrao = 1f; })),
            },
            new DadosPorco
            {
                especie = EspeciePorco.Tamboreiro, nome = "Tamboreiro", papel = "Não ataca: os porcos no círculo atiram mais rápido.", custo = 300,
                nivel1 = tambor1,
                nivel2 = M("Batuque", "Aura +35% e raio 3,0.", 220, tambor2),
                nivel3 = M("Grito de Guerra", "Vizinhos com +20% de alcance.", 320, tambor3),
                especializacaoA = M("Fanfarra", "Aura +60% e raio 3,5.", 650,
                    Mais(tambor3, a => { a.bonusCadencia = 0.6f; a.raioAura = 3.5f; a.alcance = 3.5f; })),
                especializacaoB = M("Tambor das Penas", "Pássaros estourados na aura rendem +50% de penas.", 550,
                    Mais(tambor3, a => a.bonusPenas = 0.5f)),
            },
        };
    }

    static Atributos Mais(Atributos anterior, System.Action<Atributos> efeito)
    {
        var a = anterior.Copia();
        efeito(a);
        return a;
    }

    static Melhoria M(string nome, string descricao, int custo, Atributos atributos)
    {
        return new Melhoria { nome = nome, descricao = descricao, custo = custo, atributos = atributos };
    }

    // Curva de dificuldade: a partir da onda 11, os pássaros de cada onda têm
    // 8% a mais de vida por onda (onda 20 = ×1,8; onda 30 = ×2,6). É o número
    // que mais mexe no "difícil na medida certa": a renda do jogador cresce
    // rápido no fim, e a vida dos pássaros acompanha.
    public const float CrescimentoVidaPorOnda = 0.08f;
    public const int OndaInicioCrescimento = 10;

    public static List<DadosOnda> Ondas()
    {
        var ondas = OndasBase();
        for (int i = 0; i < ondas.Count; i++)
        {
            int numero = i + 1;
            if (numero <= OndaInicioCrescimento) continue;
            foreach (var g in ondas[i].grupos) g.multiplicadorVida *= 1f + CrescimentoVidaPorOnda * (numero - OndaInicioCrescimento);
        }
        return ondas;
    }

    static List<DadosOnda> OndasBase()
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
            // 15–19: urubus (couraça); a 18 é uma revoada densa. As ondas 16 e 19
            // são a virada do jogo: quem só tem pedrinha (meia força na couraça)
            // ou não investiu em penas costuma cair aqui ou logo depois.
            O(G(Urubu, 3, 4f), G(Sabia, 20, 0.5f, 1f)),
            O(G(Urubu, 8, 2.5f), G(BeijaFlor, 25, 0.45f, 2f)),
            O(G(Pata, 12, 1.2f), G(Arara, 12, 1f, 3f)),
            O(G(Pardal, 80, 0.12f), G(BeijaFlor, 20, 0.35f, 4f)),
            O(G(Urubu, 13, 1.8f), G(Pata, 10, 1.4f, 3f)),
            // 20: chefe 2
            O(G(Urubu, 4, 3f), G(Gaviao, 1, 1f, 3f, 2.2f)),
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
                // Mais barato que o ponto de partida do design (250 … 2500): com os
                // preços antigos o Ninho não se pagava nunca e a escolha não existia.
                new NivelNinho { multiplicador = 1.25f, custo = 120 },
                new NivelNinho { multiplicador = 1.5f, custo = 250 },
                new NivelNinho { multiplicador = 1.75f, custo = 450 },
                new NivelNinho { multiplicador = 2f, custo = 750 },
                new NivelNinho { multiplicador = 2.5f, custo = 1200 },
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
