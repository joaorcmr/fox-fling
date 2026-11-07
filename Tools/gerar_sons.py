#!/usr/bin/env python3
"""Gera os efeitos sonoros do Revoada em WAV (Assets/Revoada/Audio).

Uso:  python3 Tools/gerar_sons.py

Síntese simples com Python puro: senos com envelope, ruído filtrado e
varreduras de frequência. 22 050 Hz, mono, 16 bits. Imprime a duração e o
pico de cada som (o pico fica abaixo de 0,95 para não distorcer).
"""
import math
import os
import random
import struct
import wave

RAIZ = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SAIDA = os.path.join(RAIZ, "Assets", "Revoada", "Audio")
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


def seno(freq_ini, duracao, freq_fim=None, vol=1.0, harmonicos=(1.0,), vibrato=0.0, vel_vibrato=6.0):
    freq_fim = freq_ini if freq_fim is None else freq_fim
    n = amostras(duracao)
    fase = 0.0
    out = []
    for i in range(n):
        t = i / max(1, n - 1)
        f = freq_ini * (freq_fim / freq_ini) ** t  # varredura exponencial soa mais natural
        if vibrato:
            f *= 1 + vibrato * math.sin(2 * math.pi * vel_vibrato * i / TAXA)
        fase += 2 * math.pi * f / TAXA
        out.append(vol * sum(a * math.sin(fase * (k + 1)) for k, a in enumerate(harmonicos)))
    return out


def quadrada(freq, duracao, vol=1.0, brilho=0.35):
    """Onda quadrada suavizada (soma de harmônicos ímpares), menos estridente."""
    return seno(freq, duracao, vol=vol, harmonicos=(1.0, 0, brilho, 0, brilho * 0.5, 0, brilho * 0.25))


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
    return len(faixa) / TAXA, max(abs(v) for v in faixa)


def nota(freq, dur, vol=1.0, queda=None, harmonicos=(1, 0.4, 0.15)):
    return envelope(seno(freq, dur, vol=vol, harmonicos=harmonicos), ataque=0.006, queda=queda or dur * 0.7)


def sons():
    s = {}
    # Estouro: um "plop" de bolha que cai de tom e um sopro de penas.
    for k, (f0, f1) in enumerate(((900, 260), (1050, 300), (780, 220))):
        s["estouro_%d" % (k + 1)] = misturar(
            envelope(seno(f0, 0.12, f1, vol=0.9, harmonicos=(1, 0.25)), ataque=0.002, queda=0.06),
            atrasar(envelope(ruido_varrido(0.22, 0.5, 0.08, vol=0.35, semente=40 + k), ataque=0.01, queda=0.12), 0.015),
        )
    # Tiros.
    s["tiro_estilingue"] = misturar(
        envelope(seno(380, 0.09, 820, vol=0.6), ataque=0.003, queda=0.05),
        envelope(ruido(0.05, vol=0.4, corte=0.6, semente=1), queda=0.02),
    )
    s["tiro_arpao"] = misturar(
        envelope(ruido_varrido(0.28, 0.08, 0.55, vol=0.55, semente=2), ataque=0.02, queda=0.18),
        envelope(seno(520, 0.16, 240, vol=0.5, harmonicos=(1, 0.5, 0.3)), ataque=0.002, queda=0.07),
    )
    s["tiro_bomba"] = misturar(
        envelope(seno(140, 0.22, 60, vol=1.0, harmonicos=(1, 0.3)), ataque=0.002, queda=0.12),
        envelope(ruido(0.12, vol=0.5, corte=0.12, semente=3), queda=0.05),
    )
    # Explosão de lama: estrondo abafado e um "splash" molhado.
    s["explosao"] = misturar(
        envelope(ruido(0.6, vol=0.9, corte=0.07, semente=4), ataque=0.003, queda=0.28),
        envelope(seno(220, 0.3, 70, vol=0.6, vibrato=0.08, vel_vibrato=28), ataque=0.004, queda=0.16),
        atrasar(envelope(ruido_varrido(0.25, 0.4, 0.05, vol=0.4, semente=5), queda=0.12), 0.04),
    )
    # Loja.
    s["compra"] = misturar(nota(1318.5, 0.12, 0.7, 0.08), atrasar(nota(1975.5, 0.22, 0.6, 0.16), 0.07))
    s["upgrade"] = misturar(*[atrasar(nota(f, 0.2, 0.7, 0.14), 0.065 * k)
                              for k, f in enumerate((523.25, 659.25, 783.99, 1046.5))])
    s["venda"] = misturar(nota(1046.5, 0.12, 0.6, 0.08), atrasar(nota(783.99, 0.2, 0.6, 0.14), 0.08))
    s["erro"] = misturar(envelope(quadrada(196, 0.12, vol=0.5), queda=0.1),
                         atrasar(envelope(quadrada(147, 0.18, vol=0.5), queda=0.14), 0.12))
    # Corneta de início de onda.
    corneta = (1, 0.7, 0.5, 0.35, 0.2)
    s["onda"] = misturar(
        envelope(seno(392, 0.22, vol=0.6, harmonicos=corneta, vibrato=0.01), ataque=0.03, queda=0.3),
        atrasar(envelope(seno(523.25, 0.5, vol=0.6, harmonicos=corneta, vibrato=0.012), ataque=0.03, queda=0.45), 0.2),
    )
    # Chefe: notas graves descendo, com tremor, e um tambor.
    s["chefe"] = misturar(
        *[atrasar(envelope(seno(f, 0.45, vol=0.6, harmonicos=corneta, vibrato=0.02, vel_vibrato=9), ataque=0.02,
                           queda=0.5), 0.32 * k) for k, f in enumerate((196.0, 185.0, 146.83))],
        *[atrasar(envelope(seno(90, 0.15, 50, vol=0.9), queda=0.08), 0.32 * k) for k in range(3)],
    )
    # Torre apanhando: pancada e um grunhido de porco.
    s["torre"] = misturar(
        envelope(seno(110, 0.18, 70, vol=0.8, harmonicos=(1, 0.4)), queda=0.08),
        envelope(ruido(0.1, vol=0.5, corte=0.2, semente=6), queda=0.04),
        atrasar(envelope(seno(560, 0.24, 1150, vol=0.45, harmonicos=(1, 0.6, 0.4), vibrato=0.05, vel_vibrato=32),
                         ataque=0.01, queda=0.2), 0.05),
    )
    # Fim de jogo.
    s["vitoria"] = misturar(
        *[atrasar(nota(f, 0.5, 0.7, 0.35), 0.12 * k) for k, f in enumerate((523.25, 659.25, 783.99, 1046.5))],
        atrasar(nota(1046.5, 1.0, 0.6, 0.8), 0.5),
        atrasar(nota(1318.5, 1.0, 0.5, 0.8), 0.5),
    )
    s["derrota"] = misturar(
        *[atrasar(envelope(seno(f, 0.4 if k < 3 else 1.0, f * 0.97 if k < 3 else f * 0.85, harmonicos=(1, 0.5, 0.3)),
                           queda=0.3 if k < 3 else 0.8), 0.3 * k)
          for k, f in enumerate((392.0, 369.99, 349.23, 329.63))],
    )
    s["clique"] = envelope(seno(1500, 0.05, 900, vol=0.6), queda=0.02)
    return s


def main():
    os.makedirs(SAIDA, exist_ok=True)
    for nome, faixa in sons().items():
        dur, pico = salvar(nome, faixa)
        print("%-16s %.2fs  pico %.2f" % (nome, dur, pico))


if __name__ == "__main__":
    main()
