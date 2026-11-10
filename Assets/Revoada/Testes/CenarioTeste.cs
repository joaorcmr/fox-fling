using System.Collections.Generic;
using UnityEngine;

// Cenários pequenos e previsíveis para os testes das regras. Eles NÃO usam os
// números de balanceamento do jogo (que mudam com o ajuste dos robôs), então
// um teste de regra só quebra se a regra mudar.
public static class CenarioTeste
{
    // Trilha reta no meio do mapa, de (0, 7.5) a (14, 7.5): 14 unidades.
    public static DadosJogo Basico()
    {
        var d = new DadosJogo();
        d.mapa = new DadosMapa
        {
            nome = "Teste",
            tamanho = new Vector2(14f, 15f),
            pontos = new List<Vector2> { new Vector2(0f, 7.5f), new Vector2(14f, 7.5f) },
            raioCanto = 1f,
            meiaLargura = 0.6f,
            torre = new Vector2(12f, 3f),
            raioTorre = 1.1f,
        };
        d.economia = new DadosEconomia
        {
            cofrinho = new List<NivelCofrinho>
            {
                new NivelCofrinho { multiplicador = 1.25f, custo = 250 },
                new NivelCofrinho { multiplicador = 1.5f, custo = 500 },
            },
        };
        d.passaros[EspeciePassaro.Pardal] = Passaro(EspeciePassaro.Pardal);
        return d;
    }

    public static DadosPassaro Passaro(EspeciePassaro e, float vida = 1f, float velocidade = 2f, float moedas = 1f, int dano = 1)
    {
        return new DadosPassaro
        {
            especie = e,
            nome = e.ToString(),
            vida = vida,
            velocidade = velocidade,
            moedas = moedas,
            danoTorre = dano,
            raio = 0.35f,
        };
    }

    public static DadosOnda Onda(params GrupoOnda[] grupos)
    {
        return new DadosOnda { grupos = new List<GrupoOnda>(grupos) };
    }

    public static GrupoOnda Grupo(EspeciePassaro e, int quantidade, float intervalo = 1f, float atraso = 0f)
    {
        return new GrupoOnda { especie = e, quantidade = quantidade, intervalo = intervalo, atraso = atraso };
    }

    // Porco de teste: as melhorias repetem o nível 1 (cada teste muda o que precisa).
    public static DadosPorco Porco(EspeciePorco e, int custo, Atributos nivel1)
    {
        return new DadosPorco
        {
            especie = e,
            nome = e.ToString(),
            custo = custo,
            raio = 0.45f,
            nivel1 = nivel1,
            nivel2 = new Melhoria { nome = "N2", custo = 100, atributos = nivel1.Copia() },
            nivel3 = new Melhoria { nome = "N3", custo = 180, atributos = nivel1.Copia() },
            especializacaoA = new Melhoria { nome = "A", custo = 500, atributos = nivel1.Copia() },
            especializacaoB = new Melhoria { nome = "B", custo = 550, atributos = nivel1.Copia() },
        };
    }

    public static Atributos Tiro(TipoTiro tiro, float dano = 1f, float alcance = 10f, float recarga = 10f, float velocidade = 12f, int perfura = 1)
    {
        return new Atributos { tiro = tiro, dano = dano, alcance = alcance, recarga = recarga, velocidadeProjetil = velocidade, perfura = perfura };
    }

    // Uma simulação com pássaros PARADOS (velocidade 0) nas distâncias dadas da
    // trilha, já em onda. Uma segunda onda existe para a primeira não virar vitória.
    public static Simulacao ComPassarosParados(DadosJogo d, EspeciePassaro e, params float[] distancias)
    {
        d.passaros[e].velocidade = 0f;
        d.ondas.Add(Onda(Grupo(e, distancias.Length, 0.0001f)));
        d.ondas.Add(Onda(Grupo(e, 1)));
        var s = new Simulacao(d);
        s.SoltarOnda();
        s.Passo();
        for (int i = 0; i < distancias.Length; i++)
        {
            var p = s.Passaros[i];
            p.Distancia = distancias[i];
            p.Posicao = p.PosicaoAnterior = s.Trilha.Posicao(distancias[i]);
        }
        return s;
    }

    // Avança a simulação por "segundos" de jogo (em passos fixos de 1/60 s).
    public static void Avancar(Simulacao s, float segundos)
    {
        int passos = Mathf.CeilToInt(segundos / Simulacao.Dt - 1e-3f);
        for (int i = 0; i < passos; i++) s.Passo();
    }
}
