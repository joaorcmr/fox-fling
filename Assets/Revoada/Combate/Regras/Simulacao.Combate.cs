using System;
using System.Collections.Generic;
using UnityEngine;

// O combate: porcos miram e atiram (passo 4), tiros voam e acertam (passo 5).
public partial class Simulacao
{
    readonly List<Projetil> projeteis = new List<Projetil>();
    public IReadOnlyList<Projetil> Projeteis => projeteis;

    public event Action<Porco, Projetil> TiroDisparado;
    public event Action<Projetil> ProjetilSumiu;
    public event Action<Vector2, float> BombaExplodiu; // centro, raio

    int proximoIdProjetil = 1;

    partial void PassoPorcos()
    {
        for (int i = 0; i < porcos.Count; i++)
        {
            var porco = porcos[i];
            if (!porco.Ataca) continue;
            porco.Recarga -= Dt * porco.MultCadencia;
            if (porco.Recarga > 0f) continue;

            var alvo = Alvos.Escolher(porco.Posicao, porco.Alcance, porco.Modo, passaros);
            if (alvo == null)
            {
                porco.Recarga = 0f; // fica pronto, esperando alguém entrar no alcance
                continue;
            }
            Disparar(porco, alvo);
            porco.Recarga += porco.Atributos.recarga; // += mantém o ritmo exato entre os tiros
        }
    }

    void Disparar(Porco porco, Passaro alvo)
    {
        var a = porco.Atributos.Copia();
        Vector2 ponto = Alvos.Antecipar(porco.Posicao, a.velocidadeProjetil, alvo, Trilha);
        Vector2 direcao = ponto - porco.Posicao;
        direcao = direcao.sqrMagnitude > 1e-6f ? direcao.normalized : Vector2.right;
        porco.Mira = direcao;

        int quantidade = Mathf.Max(1, a.tiros);
        for (int i = 0; i < quantidade; i++)
        {
            // Leque: os tiros se abrem em volta da direção central.
            float graus = (i - (quantidade - 1) / 2f) * a.abertura;
            Vector2 dir = Girar(direcao, graus);
            // A bomba voa só até o ponto mirado; pedrinha e arpão seguem voando.
            float distancia = a.tiro == TipoTiro.Bomba ? Vector2.Distance(porco.Posicao, ponto) : porco.Alcance * a.alcanceVoo;
            var tiro = new Projetil(proximoIdProjetil++, porco, a, porco.Posicao, dir, distancia);
            projeteis.Add(tiro);
            TiroDisparado?.Invoke(porco, tiro);
        }
    }

    partial void PassoTiros()
    {
        for (int i = 0; i < projeteis.Count; i++)
        {
            var t = projeteis[i];
            if (!t.Ativo) continue;
            t.PosicaoAnterior = t.Posicao;
            float passo = t.A.velocidadeProjetil * Dt;

            if (t.Tipo == TipoTiro.Bomba)
            {
                if (passo >= t.DistanciaRestante)
                {
                    t.Posicao = t.Destino;
                    t.DistanciaRestante = 0f;
                    Explodir(t);
                    Sumir(t);
                }
                else
                {
                    t.Posicao += t.Velocidade * Dt;
                    t.DistanciaRestante -= passo;
                }
                continue;
            }

            t.Posicao += t.Velocidade * Dt;
            t.DistanciaRestante -= passo;

            // Só os pássaros que existiam no começo do passo: filhos que nascerem
            // agora (a pata) entram na conta a partir do próximo.
            int quantos = passaros.Count;
            for (int j = 0; j < quantos && t.PerfuracaoRestante > 0; j++)
            {
                var p = passaros[j];
                if (!p.Vivo || t.Atingidos.Contains(p)) continue;
                float r = p.Raio + t.A.raioProjetil;
                if ((p.Posicao - t.Posicao).sqrMagnitude > r * r) continue;
                t.Atingidos.Add(p);
                t.PerfuracaoRestante--;
                Atingir(p, t.A.dano, t.A.ignoraCouraca);
                EfeitosDoGolpe(p, t.A);
            }

            if (t.PerfuracaoRestante <= 0 || t.DistanciaRestante <= 0f) Sumir(t);
        }
        projeteis.RemoveAll(t => !t.Ativo);
    }

    // A bomba atinge todos dentro do raio, com dano cheio mesmo na couraça.
    void Explodir(Projetil t)
    {
        Vector2 centro = t.Destino;
        float raio = t.A.raioArea;
        int quantos = passaros.Count;
        for (int j = 0; j < quantos; j++)
        {
            var p = passaros[j];
            if (!p.Vivo) continue;
            float r = raio + p.Raio;
            if ((p.Posicao - centro).sqrMagnitude > r * r) continue;
            Atingir(p, t.A.dano, true);
            EfeitosDoGolpe(p, t.A);
        }
        BombaExplodiu?.Invoke(centro, raio);
    }

    // Lama Grudenta (lentidão) e Barril de Estrume (empurrão), em quem sobreviveu.
    void EfeitosDoGolpe(Passaro p, Atributos a)
    {
        if (!p.Vivo) return;
        if (a.lentidao > 0f) AplicarLentidao(p, a.lentidao, a.duracaoLentidao);
        if (a.empurrao > 0f) Empurrar(p, a.empurrao);
    }

    // Lentidões não se acumulam: vale a mais forte e a mais longa. O chefe fica
    // no máximo 30% mais lento.
    public void AplicarLentidao(Passaro p, float lentidao, float duracao)
    {
        if (p == null || !p.Vivo || lentidao <= 0f) return;
        if (p.Dados.chefe) lentidao = Mathf.Min(lentidao, 0.3f);
        p.Lentidao = Mathf.Max(p.Lentidao, lentidao);
        p.TempoLentidao = Mathf.Max(p.TempoLentidao, duracao);
    }

    // Empurra o pássaro para trás na trilha (nunca para antes do começo). O chefe
    // é pesado demais para ser empurrado.
    public void Empurrar(Passaro p, float distancia)
    {
        if (p == null || !p.Vivo || p.Dados.chefe || distancia <= 0f) return;
        p.Distancia = Mathf.Max(0f, p.Distancia - distancia);
        p.Posicao = Trilha.Posicao(p.Distancia);
    }

    void Sumir(Projetil t)
    {
        if (!t.Ativo) return;
        t.Ativo = false;
        ProjetilSumiu?.Invoke(t);
    }

    static Vector2 Girar(Vector2 v, float graus)
    {
        if (graus == 0f) return v;
        float a = graus * Mathf.Deg2Rad, c = Mathf.Cos(a), s = Mathf.Sin(a);
        return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
    }
}
