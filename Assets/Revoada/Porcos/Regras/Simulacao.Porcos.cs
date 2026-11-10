using System;
using System.Collections.Generic;
using UnityEngine;

// Os comandos de porco: comprar, vender e mudar o modo de alvo. São imediatos
// (não esperam um Passo), por isso funcionam com o jogo pausado.
public partial class Simulacao
{
    readonly List<Porco> porcos = new List<Porco>();
    public IReadOnlyList<Porco> Porcos => porcos;

    public event Action<Porco> PorcoCriado;
    public event Action<Porco> PorcoVendido;
    public event Action<Porco> PorcoMelhorado; // subiu de nível ou se especializou

    int proximoIdPorco = 1;

    public bool PodeColocar(EspeciePorco especie, Vector2 posicao)
    {
        if (Acabou || !Dados.porcos.TryGetValue(especie, out var dados)) return false;
        return Colocacao.Valida(posicao, dados.raio, Dados.mapa, Trilha, porcos);
    }

    public Resultado Comprar(EspeciePorco especie, Vector2 posicao, out Porco porco)
    {
        porco = null;
        if (Acabou || !Dados.porcos.TryGetValue(especie, out var dados)) return Resultado.Indisponivel;
        if (!Colocacao.Valida(posicao, dados.raio, Dados.mapa, Trilha, porcos)) return Resultado.LugarInvalido;
        if (!Economia.Gastar(dados.custo)) return Resultado.SemPenas;

        porco = new Porco(proximoIdPorco++, dados, posicao) { Gasto = dados.custo };
        porcos.Add(porco);
        RecalcularAuras();
        PorcoCriado?.Invoke(porco);
        return Resultado.Ok;
    }

    // ------------------------------------------------------------------
    // Upgrades: nível 1 → 2 → 3 e, no 3, uma de duas especializações (nível 4)
    // ------------------------------------------------------------------
    public Melhoria ProximaMelhoria(Porco p)
    {
        if (p == null) return null;
        if (p.Nivel == 1) return p.Dados.nivel2;
        if (p.Nivel == 2) return p.Dados.nivel3;
        return null; // no 3 vem a especialização; no 4 acabou
    }

    public Resultado Melhorar(Porco p)
    {
        if (Acabou || p == null || !porcos.Contains(p)) return Resultado.Indisponivel;
        var m = ProximaMelhoria(p);
        if (m == null) return Resultado.Indisponivel;
        if (!Economia.Gastar(m.custo)) return Resultado.SemPenas;
        p.Nivel++;
        Aplicar(p, m);
        return Resultado.Ok;
    }

    public Resultado Especializar(Porco p, int escolha)
    {
        if (Acabou || p == null || !porcos.Contains(p) || p.Nivel != 3 || (escolha != 0 && escolha != 1))
            return Resultado.Indisponivel;
        var m = escolha == 0 ? p.Dados.especializacaoA : p.Dados.especializacaoB;
        if (!Economia.Gastar(m.custo)) return Resultado.SemPenas;
        p.Nivel = 4;
        p.Especializacao = escolha;
        Aplicar(p, m);
        return Resultado.Ok;
    }

    void Aplicar(Porco p, Melhoria m)
    {
        p.Gasto += m.custo;
        p.Atributos = m.atributos.Copia();
        // Se a recarga nova é mais curta, o próximo tiro não espera a antiga.
        p.Recarga = Mathf.Min(p.Recarga, p.Atributos.recarga);
        RecalcularAuras();
        PorcoMelhorado?.Invoke(p);
    }

    // ------------------------------------------------------------------
    // Auras do Tamboreiro ("agrupar")
    // ------------------------------------------------------------------

    // Para cada porco que ataca, vale o MAIOR bônus entre os tambores que o
    // alcançam: as auras não se somam, então empilhar tambores não vira uma
    // estratégia que sempre ganha. Só muda quando os porcos mudam, então é
    // recalculado nos comandos e não a cada passo.
    void RecalcularAuras()
    {
        foreach (var p in porcos)
        {
            float cadencia = 0f, alcance = 0f;
            if (p.Ataca)
            {
                foreach (var t in porcos)
                {
                    if (t.Ataca || t.Atributos.raioAura <= 0f) continue;
                    if (Vector2.Distance(p.Posicao, t.Posicao) > t.Atributos.raioAura) continue;
                    cadencia = Mathf.Max(cadencia, t.Atributos.bonusCadencia);
                    alcance = Mathf.Max(alcance, t.Atributos.bonusAlcance);
                }
            }
            p.MultCadencia = 1f + cadencia;
            p.MultAlcance = 1f + alcance;
        }
    }

    // Quanto a mais rendem as penas de um pássaro estourado neste ponto (o
    // Tambor das Penas transforma um bom ponto de abate numa fazenda de penas).
    public float BonusPenas(Vector2 posicao)
    {
        float bonus = 0f;
        foreach (var t in porcos)
        {
            if (t.Ataca || t.Atributos.bonusPenas <= 0f) continue;
            if (Vector2.Distance(posicao, t.Posicao) <= t.Atributos.raioAura) bonus = Mathf.Max(bonus, t.Atributos.bonusPenas);
        }
        return 1f + bonus;
    }

    public int ValorVenda(Porco p) => p == null ? 0 : Economia.ValorVenda(p.Gasto);

    // Devolve as penas que voltaram (0 se o porco já não está no jogo).
    public int Vender(Porco p)
    {
        if (Acabou || p == null || !porcos.Remove(p)) return 0;
        int valor = Economia.ValorVenda(p.Gasto);
        Economia.Creditar(valor);
        RecalcularAuras();
        PorcoVendido?.Invoke(p);
        return valor;
    }

    public void MudarModo(Porco p, ModoAlvo modo)
    {
        if (Acabou || p == null) return;
        p.Modo = modo;
    }
}
