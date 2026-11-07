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
            ninho = new List<NivelNinho>
            {
                new NivelNinho { multiplicador = 1.25f, custo = 250 },
                new NivelNinho { multiplicador = 1.5f, custo = 500 },
            },
        };
        d.passaros[EspeciePassaro.Pardal] = Passaro(EspeciePassaro.Pardal);
        return d;
    }

    public static DadosPassaro Passaro(EspeciePassaro e, float vida = 1f, float velocidade = 2f, float penas = 1f, int dano = 1)
    {
        return new DadosPassaro
        {
            especie = e,
            nome = e.ToString(),
            vida = vida,
            velocidade = velocidade,
            penas = penas,
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
}
