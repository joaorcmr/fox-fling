#!/usr/bin/env python3
"""Gera os efeitos sonoros do Fox Fling em WAV (Assets/FoxFling/Audio).

Uso:  python3 Tools/gerar_sons.py

Síntese simples com Python puro: senos com envelope, ruído filtrado e
varreduras de frequência. 22 050 Hz, mono, 16 bits.
"""
import math
import os
import random
import struct
import wave

RAIZ = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SAIDA = os.path.join(RAIZ, "Assets", "FoxFling", "Audio")
TAXA = 22050


def amostras(duracao):
    return int(TAXA * duracao)


def silencio(duracao):
    return [0.0] * amostras(duracao)


def misturar(*faixas):
    n = max(len(f) for f in faixas)
    out = [0.0] * n
    for f in faixas:
        for i, v in enumerate(f):
            out[i] += v
    return out


def atrasar(faixa, segundos):
    return silencio(segundos) + faixa


def envelope(faixa, ataque=0.005, queda=None):
    """Ataque linear e queda exponencial (queda = tempo para cair ~95%)."""
    n = len(faixa)
    a = max(1, amostras(ataque))
    out = []
    for i, v in enumerate(faixa):
        g = min(1.0, i / a)
        if queda:
            g *= math.exp(-3.0 * (i / TAXA) / queda)
        else:
            g *= max(0.0, 1 - i / n)
        out.append(v * g)
    return out


def seno(freq_ini, duracao, freq_fim=None, vol=1.0, harmonicos=(1.0,)):
    freq_fim = freq_ini if freq_fim is None else freq_fim
    n = amostras(duracao)
    fase = 0.0
    out = []
    for i in range(n):
        t = i / max(1, n - 1)
        f = freq_ini * (freq_fim / freq_ini) ** t  # varredura exponencial soa mais natural
        fase += 2 * math.pi * f / TAXA
        v = sum(a * math.sin(fase * (k + 1)) for k, a in enumerate(harmonicos))
        out.append(v * vol)
    return out


def quadrada(freq, duracao, vol=1.0):
    n = amostras(duracao)
    return [vol * (1.0 if math.sin(2 * math.pi * freq * i / TAXA) >= 0 else -1.0) for i in range(n)]


def ruido(duracao, vol=1.0, corte=1.0, semente=0):
    """Ruído branco passado por um filtro passa-baixa de um polo (corte 0..1)."""
    rnd = random.Random(semente)
    y = 0.0
    out = []
    for _ in range(amostras(duracao)):
        y += corte * (rnd.uniform(-1, 1) - y)
        out.append(y * vol)
    return out


def ruido_varrido(duracao, corte_ini, corte_fim, vol=1.0, semente=0):
    rnd = random.Random(semente)
    n = amostras(duracao)
    y = 0.0
    out = []
    for i in range(n):
        c = corte_ini + (corte_fim - corte_ini) * i / max(1, n - 1)
        y += c * (rnd.uniform(-1, 1) - y)
        out.append(y * vol)
    return out


def normalizar(faixa, pico=0.85):
    m = max(1e-9, max(abs(v) for v in faixa))
    return [v * pico / m for v in faixa]


def salvar(nome, faixa, pico=0.85):
    faixa = normalizar(faixa, pico)
    caminho = os.path.join(SAIDA, nome + ".wav")
    with wave.open(caminho, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(TAXA)
        w.writeframes(b"".join(struct.pack("<h", int(max(-1, min(1, v)) * 32767)) for v in faixa))
    return caminho, len(faixa) / TAXA


def sons():
    s = {}
    s["lancar"] = misturar(
        envelope(ruido_varrido(0.4, 0.05, 0.4, vol=0.6, semente=1), ataque=0.06, queda=0.25),
        envelope(seno(220, 0.25, 110, vol=0.7, harmonicos=(1, 0.5, 0.25)), queda=0.1),
    )
    s["esticar"] = envelope(seno(110, 0.3, 190, vol=0.5, harmonicos=(1, 0.6, 0.4, 0.3)), ataque=0.05, queda=0.4)
    s["pulo"] = envelope(seno(300, 0.18, 620, vol=0.8), queda=0.12)
    s["impacto_madeira"] = misturar(
        envelope(seno(190, 0.2, 150, harmonicos=(1, 0.5, 0.3)), queda=0.06),
        envelope(ruido(0.08, vol=0.5, corte=0.3, semente=2), queda=0.03),
    )
    quebra_madeira = [envelope(ruido(0.12, vol=0.9, corte=0.5, semente=10 + k), queda=0.05) for k in range(4)]
    s["quebra_madeira"] = misturar(
        envelope(seno(150, 0.3, 90, harmonicos=(1, 0.4)), queda=0.12),
        *[atrasar(q, 0.04 + 0.07 * k) for k, q in enumerate(quebra_madeira)],
    )
    s["impacto_vidro"] = misturar(*[envelope(seno(f, 0.3, vol=v), queda=0.1) for f, v in ((2300, 0.6), (3450, 0.4), (4200, 0.3))])
    rnd = random.Random(5)
    s["quebra_vidro"] = misturar(
        envelope(ruido(0.5, vol=0.5, corte=0.9, semente=3), queda=0.2),
        *[atrasar(envelope(seno(rnd.uniform(2500, 5200), 0.15, vol=0.5), queda=0.05), rnd.uniform(0, 0.45)) for _ in range(14)],
    )
    s["impacto_rocha"] = misturar(
        envelope(seno(95, 0.22, 70, harmonicos=(1, 0.3)), queda=0.08),
        envelope(ruido(0.1, vol=0.6, corte=0.15, semente=4), queda=0.04),
    )
    s["quebra_rocha"] = misturar(
        envelope(ruido(0.7, vol=1.0, corte=0.08, semente=6), ataque=0.01, queda=0.35),
        *[atrasar(envelope(seno(rnd.uniform(70, 130), 0.15, harmonicos=(1, 0.4)), queda=0.06), 0.08 * k) for k in range(5)],
    )
    s["inimigo_morre"] = misturar(
        envelope(seno(900, 0.25, 180, vol=0.8, harmonicos=(1, 0.3)), queda=0.15),
        atrasar(envelope(ruido(0.25, vol=0.5, corte=0.2, semente=7), ataque=0.02, queda=0.12), 0.05),
    )
    s["explosao"] = misturar(
        envelope(ruido(1.4, vol=1.0, corte=0.06, semente=8), ataque=0.003, queda=0.7),
        envelope(ruido(0.25, vol=0.6, corte=0.6, semente=9), ataque=0.001, queda=0.08),
        envelope(seno(70, 0.8, 35), queda=0.4),
    )
    s["arrancada"] = misturar(
        envelope(ruido_varrido(0.35, 0.05, 0.7, vol=0.7, semente=12), ataque=0.02, queda=0.25),
        envelope(seno(300, 0.3, 1300, vol=0.5), ataque=0.01, queda=0.2),
    )
    notas = (523.25, 659.25, 783.99, 1046.5)
    s["vitoria"] = misturar(
        *[atrasar(envelope(seno(f, 0.5, harmonicos=(1, 0.3, 0.1)), queda=0.35), 0.12 * k) for k, f in enumerate(notas)],
        atrasar(envelope(seno(1046.5, 0.9, harmonicos=(1, 0.3, 0.1)), queda=0.7), 0.5),
        atrasar(envelope(seno(1318.5, 0.9, harmonicos=(1, 0.3)), queda=0.7), 0.5),
    )
    s["derrota"] = misturar(
        *[atrasar(envelope(seno(f, 0.4 if k < 3 else 0.9, f * 0.97 if k < 3 else f * 0.85, harmonicos=(1, 0.5, 0.3)), queda=0.3 if k < 3 else 0.8), 0.3 * k)
          for k, f in enumerate((392.0, 369.99, 349.23, 329.63))],
    )
    s["clique"] = envelope(seno(1500, 0.05, 900, vol=0.6), queda=0.02)
    s["estrela"] = misturar(
        envelope(seno(1318.5, 0.6, vol=0.7), queda=0.3),
        envelope(seno(1975.5, 0.6, vol=0.4), queda=0.25),
        atrasar(envelope(seno(2637, 0.3, vol=0.25), queda=0.15), 0.05),
    )
    return s


def main():
    os.makedirs(SAIDA, exist_ok=True)
    for nome, faixa in sons().items():
        caminho, dur = salvar(nome, faixa)
        print("%-16s %.2fs  %6d bytes" % (nome, dur, os.path.getsize(caminho)))


if __name__ == "__main__":
    main()
