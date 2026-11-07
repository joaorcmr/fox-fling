#!/usr/bin/env python3
"""Gera toda a arte do Revoada em PNG (Assets/Revoada/Arte).

Uso:  python3 Tools/gerar_arte.py [pasta_contato]

Escala: 64 pixels = 1 unidade do Unity (PPU 64). Na tela de 600×480 uma
unidade ocupa 32 px, então tudo é desenhado com o dobro da resolução em que
aparece e fica nítido mesmo com o jogo em tela cheia.

Personagens são desenhados 4× maiores e reduzidos (antialias). O contorno
marrom-escuro é feito "inflando" a silhueta inteira (filtro de máximo), como
um adesivo: é o que dá o ar dos bichos do Iran (urubu e arara), que entram
reduzidos a partir de Arte/Fontes. Ladrilhos (grama e terra) são desenhados
pixel a pixel, para emendarem sem costura.
"""
import math
import os
import random
import sys

from PIL import Image, ImageChops, ImageDraw, ImageFilter, ImageFont

RAIZ = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SAIDA = os.path.join(RAIZ, "Assets", "Revoada", "Arte")
FONTES = os.path.join(SAIDA, "Fontes")
S = 4  # supersampling

CONTORNO = (43, 29, 20, 255)
BRANCO = (255, 255, 255, 255)
FONTE = "/System/Library/Fonts/Supplemental/Arial Rounded Bold.ttf"


def hexc(h, a=255):
    h = h.lstrip("#")
    return (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16), a)


def mais_escuro(c, f=0.72):
    return (int(c[0] * f), int(c[1] * f), int(c[2] * f), c[3])


def mais_claro(c, f=0.35):
    return tuple(int(c[i] + (255 - c[i]) * f) for i in range(3)) + (c[3],)


# --------------------------------------------------------------------------
# Ferramentas de desenho (coordenadas sempre em pixels 1×)
# --------------------------------------------------------------------------
class Tela:
    """Uma imagem 4× com atalhos de desenho em coordenadas 1×."""

    def __init__(self, w, h):
        self.w, self.h = w, h
        self.img = Image.new("RGBA", (w * S, h * S), (0, 0, 0, 0))
        self.d = ImageDraw.Draw(self.img)

    def elipse(self, cx, cy, rx, ry, cor):
        self.d.ellipse([(cx - rx) * S, (cy - ry) * S, (cx + rx) * S, (cy + ry) * S], fill=cor)

    def poligono(self, pts, cor):
        self.d.polygon([(x * S, y * S) for x, y in pts], fill=cor)

    def linha(self, pts, cor, esp):
        self.d.line([(x * S, y * S) for x, y in pts], fill=cor, width=max(1, int(esp * S)), joint="curve")
        for x, y in (pts[0], pts[-1]):  # pontas redondas
            self.elipse(x, y, esp / 2, esp / 2, cor)

    def elipse_girada(self, cx, cy, rx, ry, graus, cor):
        a = math.radians(graus)
        pts = []
        for k in range(36):
            t = k * math.tau / 36
            x, y = math.cos(t) * rx, math.sin(t) * ry
            pts.append((cx + x * math.cos(a) - y * math.sin(a), cy + x * math.sin(a) + y * math.cos(a)))
        self.poligono(pts, cor)

    def retangulo(self, x0, y0, x1, y1, cor, raio=0):
        self.d.rounded_rectangle([x0 * S, y0 * S, x1 * S, y1 * S], radius=raio * S, fill=cor)

    def arco(self, cx, cy, r, ini, fim, cor, esp):
        self.d.arc([(cx - r) * S, (cy - r) * S, (cx + r) * S, (cy + r) * S], ini, fim, fill=cor, width=int(esp * S))

    def contornar(self, esp=2.4, cor=CONTORNO):
        """Põe um contorno em volta de TUDO o que já foi desenhado."""
        alfa = self.img.getchannel("A").point(lambda v: 255 if v > 40 else 0)
        tam = int(esp * S) * 2 + 1
        inflado = alfa.filter(ImageFilter.MaxFilter(tam))
        fundo = Image.new("RGBA", self.img.size, (0, 0, 0, 0))
        fundo.paste(Image.new("RGBA", self.img.size, cor), mask=inflado)
        fundo.alpha_composite(self.img)
        self.img = fundo
        self.d = ImageDraw.Draw(self.img)

    def camada(self):
        return Tela(self.w, self.h)

    def colar(self, outra, mascara=None):
        if mascara is None:
            self.img.alpha_composite(outra.img)
        else:
            vazio = Image.new("RGBA", self.img.size, (0, 0, 0, 0))
            self.img.alpha_composite(Image.composite(outra.img, vazio, mascara))
        self.d = ImageDraw.Draw(self.img)

    def mascara_elipse(self, cx, cy, rx, ry):
        m = Image.new("L", self.img.size, 0)
        ImageDraw.Draw(m).ellipse([(cx - rx) * S, (cy - ry) * S, (cx + rx) * S, (cy + ry) * S], fill=255)
        return m

    def final(self):
        return self.img.resize((self.w, self.h), Image.LANCZOS)


def salvar(img, nome, feitos):
    img.save(os.path.join(SAIDA, nome + ".png"))
    feitos.append(nome)


def bola_sombreada(t, cx, cy, rx, ry, cor, sombra=None, luz=True):
    """Corpo redondo com sombra embaixo à direita e um brilho em cima à esquerda."""
    t.elipse(cx, cy, rx, ry, cor)
    sombra = sombra or mais_escuro(cor, 0.78)
    corpo = t.mascara_elipse(cx, cy, rx, ry)
    iluminado = t.mascara_elipse(cx - rx * 0.16, cy - ry * 0.2, rx * 0.98, ry * 0.95)
    camada = t.camada()
    camada.img.paste(Image.new("RGBA", t.img.size, sombra), mask=ImageChops.subtract(corpo, iluminado))
    t.colar(camada)
    if luz:
        brilho = t.camada()
        brilho.elipse(cx - rx * 0.42, cy - ry * 0.5, rx * 0.26, ry * 0.17, mais_claro(cor, 0.55)[:3] + (150,))
        t.colar(brilho, corpo)


def olhos(t, cx, cy, sep, r, bravo=True, cor_iris=(30, 22, 18, 255), olhar=(0.15, 0.25)):
    """Dois olhos grandes com sobrancelha brava (o "emburrado" do estilo)."""
    for lado in (-1, 1):
        ox = cx + lado * sep
        t.elipse(ox, cy, r + 1.0, r * 1.08 + 1.0, CONTORNO)
        t.elipse(ox, cy, r, r * 1.08, BRANCO)
        px = ox - lado * r * olhar[0]
        py = cy + r * olhar[1]
        t.elipse(px, py, r * 0.55, r * 0.6, cor_iris)
        t.elipse(px - r * 0.18, py - r * 0.25, r * 0.2, r * 0.2, BRANCO)
        if bravo:
            # Ponta de dentro mais baixa que a de fora = cara de bravo.
            x_fora, x_dentro = ox + lado * r * 1.1, ox - lado * r * 0.85
            y_fora, y_dentro = cy - r * 1.45, cy - r * 0.78
            esp = max(1.6, r * 0.42)
            t.poligono([(x_fora, y_fora - esp * 0.5), (x_dentro, y_dentro - esp * 0.5),
                        (x_dentro, y_dentro + esp * 0.6), (x_fora, y_fora + esp * 0.4)], CONTORNO)


def bochechas(t, cx, cy, sep, r, cor=(255, 128, 140, 120)):
    for lado in (-1, 1):
        t.elipse(cx + lado * sep, cy, r, r * 0.6, cor)


def bico_frente(t, cx, cy, larg, alt, cor):
    """Bico visto de frente: um losango apontando para baixo."""
    t.poligono([(cx - larg, cy), (cx, cy - alt * 0.35), (cx + larg, cy), (cx, cy + alt)], cor)
    t.poligono([(cx - larg, cy), (cx + larg, cy), (cx, cy + alt * 0.25)], mais_escuro(cor, 0.85))


def pezinhos(t, cx, cy, sep, cor=hexc("#F0A23C")):
    for lado in (-1, 1):
        x = cx + lado * sep
        for dx in (-1.6, 0, 1.6):
            t.linha([(x, cy - 2), (x + dx, cy + 1.5)], cor, 1.4)


# --------------------------------------------------------------------------
# Pássaros
# --------------------------------------------------------------------------
def passaro_base(W, H, cx, cy, r, corpo, barriga, asa, bico, cauda=None, pes=True):
    """Pássaro redondo de frente. Devolve a Tela já contornada (detalhes do rosto
    ficam por conta de quem chama)."""
    t = Tela(W, H)
    if cauda:
        t.poligono([(cx - r * 0.35, cy + r * 0.6), (cx + r * 0.35, cy + r * 0.6), (cx + r * 0.65, cy + r * 1.15),
                    (cx - r * 0.65, cy + r * 1.15)], cauda)
    for lado in (-1, 1):  # asas aparecendo dos lados
        t.elipse(cx + lado * r * 0.86, cy + r * 0.2, r * 0.32, r * 0.5, asa)
    bola_sombreada(t, cx, cy, r, r * 0.96, corpo)
    if barriga:
        b = t.camada()
        b.elipse(cx, cy + r * 0.42, r * 0.62, r * 0.55, barriga)
        t.colar(b, t.mascara_elipse(cx, cy, r, r * 0.96))
    if pes:
        pezinhos(t, cx, cy + r * 1.02, r * 0.32)
    t.contornar(esp=max(2.0, r * 0.085))
    return t


def pardal():
    W = H = 56
    cx, cy, r = 28, 27, 21
    t = passaro_base(W, H, cx, cy, r, hexc("#B98352"), hexc("#EBDDC3"), hexc("#7A4E2C"), None, cauda=hexc("#6E4526"))
    # Boné marrom-escuro no alto da cabeça e babador preto embaixo do bico.
    topo = t.camada()
    topo.elipse(cx, cy - r * 0.95, r * 0.75, r * 0.42, hexc("#7A4E2C"))
    t.colar(topo, t.mascara_elipse(cx, cy, r, r * 0.96))
    t.elipse(cx, cy + r * 0.38, r * 0.3, r * 0.2, hexc("#3B2A20"))
    # Listras das asas.
    for lado in (-1, 1):
        for k in range(2):
            t.linha([(cx + lado * r * 0.8, cy + r * (0.05 + 0.25 * k)), (cx + lado * r * 1.05, cy + r * (0.18 + 0.25 * k))],
                    hexc("#4E321C"), 1.3)
    olhos(t, cx, cy - r * 0.15, r * 0.36, r * 0.22)
    bochechas(t, cx, cy + r * 0.12, r * 0.6, r * 0.16)
    bico_frente(t, cx, cy + r * 0.08, r * 0.16, r * 0.26, hexc("#F2B33D"))
    return t.final()


def sabia():
    W = H = 60
    cx, cy, r = 30, 29, 23
    t = passaro_base(W, H, cx, cy, r, hexc("#8E7356"), hexc("#E07B37"), hexc("#6B533B"), None, cauda=hexc("#5C4630"))
    # Garganta clara com risquinhos (o sabiá-laranjeira).
    g = t.camada()
    g.elipse(cx, cy + r * 0.05, r * 0.36, r * 0.22, hexc("#EEDFC8"))
    t.colar(g)
    for k in (-1, 0, 1):
        t.linha([(cx + k * r * 0.15, cy - r * 0.02), (cx + k * r * 0.15, cy + r * 0.12)], hexc("#8E7356"), 1.0)
    olhos(t, cx, cy - r * 0.2, r * 0.36, r * 0.22)
    for lado in (-1, 1):  # anel amarelo em volta do olho
        t.arco(cx + lado * r * 0.36, cy - r * 0.2, r * 0.3, 200 if lado < 0 else -20, 340 if lado < 0 else 160,
               hexc("#E9B949"), 1.0)
    bochechas(t, cx, cy + r * 0.12, r * 0.62, r * 0.15)
    bico_frente(t, cx, cy + r * 0.02, r * 0.15, r * 0.28, hexc("#D9B44A"))
    return t.final()


def beija_flor():
    W, H = 62, 56
    cx, cy, r = 31, 28, 19
    t = Tela(W, H)
    # Asas batendo tão rápido que viram um borrão claro, abertas para cima.
    for lado in (-1, 1):
        t.elipse_girada(cx + lado * r * 1.05, cy - r * 0.55, r * 0.75, r * 0.3, -lado * 35, (190, 235, 250, 135))
        t.elipse_girada(cx + lado * r * 1.05, cy - r * 0.55, r * 0.5, r * 0.14, -lado * 35, (240, 252, 255, 170))
    t2 = passaro_base(W, H, cx, cy, r, hexc("#3FBF7A"), None, hexc("#2E8F5B"), None, cauda=hexc("#2A6E8F"), pes=False)
    t.colar(t2)
    garganta = t.camada()
    garganta.elipse(cx, cy + r * 0.45, r * 0.55, r * 0.38, hexc("#5B4FD9"))
    t.colar(garganta, t.mascara_elipse(cx, cy, r, r * 0.96))
    olhos(t, cx, cy - r * 0.18, r * 0.36, r * 0.24)
    # Bico comprido e fino, saindo na diagonal para ficar visível de frente.
    t.linha([(cx + 1, cy + r * 0.08), (cx + r * 0.85, cy + r * 1.05)], CONTORNO, 3.6)
    t.linha([(cx + 1, cy + r * 0.08), (cx + r * 0.83, cy + r * 1.02)], hexc("#3A3A3A"), 1.8)
    bochechas(t, cx, cy + r * 0.12, r * 0.62, r * 0.15, (255, 140, 190, 120))
    return t.final()


def pata(filhote=False):
    if filhote:
        W = H = 44
        cx, cy, r = 22, 22, 16
        corpo, barriga = hexc("#F7D84A"), hexc("#FBE88A")
    else:
        W = H = 68
        cx, cy, r = 34, 33, 26
        corpo, barriga = hexc("#F4F1EA"), hexc("#FFFFFF")
    t = passaro_base(W, H, cx, cy, r, corpo, barriga, mais_escuro(corpo, 0.88), None, cauda=mais_escuro(corpo, 0.8))
    if filhote:
        # Topete de penugem.
        for dx, alt in ((-2, 6), (1, 8), (4, 5)):
            t.linha([(cx + dx, cy - r * 0.9), (cx + dx * 1.4, cy - r * 0.9 - alt)], CONTORNO, 2.4)
            t.linha([(cx + dx, cy - r * 0.9), (cx + dx * 1.4, cy - r * 0.9 - alt)], corpo, 1.2)
    else:
        # Lenço azul de mãe na cabeça, amarrado de lado.
        lenco = t.camada()
        lenco.elipse(cx, cy - r * 0.88, r * 0.85, r * 0.42, hexc("#3E78C9"))
        t.colar(lenco, t.mascara_elipse(cx, cy, r, r * 0.96))
        for k in range(3):
            t.elipse(cx - r * 0.45 + k * r * 0.45, cy - r * 0.86, r * 0.07, r * 0.07, BRANCO)
        t.poligono([(cx + r * 0.75, cy - r * 0.75), (cx + r * 1.18, cy - r * 0.95), (cx + r * 1.12, cy - r * 0.55)],
                   hexc("#3E78C9"))
    olhos(t, cx, cy - r * 0.16, r * 0.36, r * 0.22, bravo=not filhote)
    bochechas(t, cx, cy + r * 0.14, r * 0.62, r * 0.16)
    # Bico achatado de pato.
    bico = hexc("#F29A2E")
    t.retangulo(cx - r * 0.34 - 1, cy + r * 0.02 - 1, cx + r * 0.34 + 1, cy + r * 0.34 + 1, CONTORNO, raio=r * 0.18)
    t.retangulo(cx - r * 0.34, cy + r * 0.02, cx + r * 0.34, cy + r * 0.34, bico, raio=r * 0.16)
    t.linha([(cx - r * 0.28, cy + r * 0.18), (cx + r * 0.28, cy + r * 0.18)], mais_escuro(bico, 0.7), 0.9)
    return t.final()


def gaviao():
    W = H = 124
    cx, cy, r = 62, 66, 46
    t = Tela(W, H)
    # Asas grandes abertas atrás do corpo.
    for lado in (-1, 1):
        t.poligono([(cx + lado * r * 0.6, cy - r * 0.3), (cx + lado * r * 1.32, cy - r * 0.55),
                    (cx + lado * r * 1.3, cy + r * 0.35), (cx + lado * r * 0.75, cy + r * 0.6)], hexc("#4A4F57"))
        for k in range(3):
            y = cy - r * 0.3 + k * r * 0.28
            t.linha([(cx + lado * r * 0.95, y), (cx + lado * r * 1.25, y - r * 0.1)], hexc("#33373D"), 2.0)
    # Penacho duplo do gavião-real.
    for lado in (-1, 1):
        t.poligono([(cx + lado * r * 0.15, cy - r * 0.85), (cx + lado * r * 0.62, cy - r * 1.32),
                    (cx + lado * r * 0.45, cy - r * 0.72)], hexc("#3B3F45"))
    bola_sombreada(t, cx, cy, r, r * 0.95, hexc("#9AA1A8"))
    rosto = t.camada()
    rosto.elipse(cx, cy - r * 0.1, r * 0.72, r * 0.6, hexc("#D9DDE1"))
    rosto.elipse(cx, cy + r * 0.62, r * 0.7, r * 0.42, hexc("#F2F2F0"))
    rosto.retangulo(cx - r, cy + r * 0.24, cx + r, cy + r * 0.4, hexc("#3B3F45"))
    t.colar(rosto, t.mascara_elipse(cx, cy, r, r * 0.95))
    # Garras amarelas.
    for lado in (-1, 1):
        x = cx + lado * r * 0.35
        for dx in (-4, 0, 4):
            t.linha([(x, cy + r * 0.9), (x + dx, cy + r * 1.1)], hexc("#E9B949"), 3.0)
    t.contornar(esp=3.4)
    olhos(t, cx, cy - r * 0.2, r * 0.34, r * 0.2, cor_iris=hexc("#2B1D14"))
    # Bico grande e curvo, com a base amarela.
    t.elipse(cx, cy + r * 0.0, r * 0.2, r * 0.12, hexc("#E9B949"))
    t.poligono([(cx - r * 0.18, cy + r * 0.04), (cx + r * 0.18, cy + r * 0.04), (cx + r * 0.06, cy + r * 0.42),
                (cx - r * 0.02, cy + r * 0.32)], CONTORNO)
    t.poligono([(cx - r * 0.13, cy + r * 0.06), (cx + r * 0.13, cy + r * 0.06), (cx + r * 0.04, cy + r * 0.34),
                (cx - r * 0.01, cy + r * 0.27)], hexc("#3A3D42"))
    return t.final()


def abrir_arte(caminho):
    """Abre a arte do Iran, limpa o alfa (ruído em volta) e recorta as bordas vazias."""
    img = Image.open(caminho).convert("RGBA")
    alfa = img.getchannel("A").point(lambda v: 0 if v < 32 else 255 if v > 200 else v)
    img.putalpha(alfa)
    return img.crop(alfa.getbbox())


def da_arte(nome_fonte, W, H, largura, base):
    arte = abrir_arte(os.path.join(FONTES, nome_fonte))
    img = Image.new("RGBA", (W * S, H * S), (0, 0, 0, 0))
    w = round(largura * S)
    h = round(arte.height * w / arte.width)
    img.alpha_composite(arte.resize((w, h), Image.LANCZOS), (round(W * S / 2 - w / 2), round(base * S - h)))
    return img.resize((W, H), Image.LANCZOS)


def sombra():
    t = Tela(64, 24)
    t.elipse(32, 12, 26, 8, (0, 0, 0, 255))
    img = t.final()
    alfa = img.getchannel("A").filter(ImageFilter.GaussianBlur(2.2)).point(lambda v: int(v * 0.45))
    preto = Image.new("RGBA", img.size, (20, 30, 10, 255))
    preto.putalpha(alfa)
    return preto


# --------------------------------------------------------------------------
# Porcos (verdes, rabo enrolado, cada um com seu chapéu)
# --------------------------------------------------------------------------
VERDE = hexc("#8CCB5E")
VERDE_SOMBRA = hexc("#5E9E3F")
FOCINHO = hexc("#B9E58F")
PX, PY, PR = 40, 47, 25  # centro e raio do corpo dentro da tela 80×80


def porco(chapeu):
    t = Tela(80, 80)
    # Rabo enrolado aparecendo à direita (atrás do corpo).
    t.arco(PX + PR * 1.02, PY + PR * 0.25, 5, 110, 470, VERDE_SOMBRA, 2.6)
    # Orelhas.
    for lado in (-1, 1):
        t.poligono([(PX + lado * PR * 0.45, PY - PR * 0.72), (PX + lado * PR * 0.95, PY - PR * 1.12),
                    (PX + lado * PR * 0.9, PY - PR * 0.45)], VERDE_SOMBRA)
    bola_sombreada(t, PX, PY, PR, PR * 0.92, VERDE, VERDE_SOMBRA)
    for lado in (-1, 1):  # cascos
        t.elipse(PX + lado * PR * 0.42, PY + PR * 0.92, PR * 0.2, PR * 0.13, hexc("#4C3A2A"))
    chapeu("forma", t)
    t.contornar(esp=2.3)
    # Rosto.
    olhos(t, PX, PY - PR * 0.12, PR * 0.36, PR * 0.2)
    bochechas(t, PX, PY + PR * 0.22, PR * 0.66, PR * 0.15, (255, 150, 150, 90))
    t.elipse(PX, PY + PR * 0.3, PR * 0.36 + 1.2, PR * 0.25 + 1.2, CONTORNO)
    t.elipse(PX, PY + PR * 0.3, PR * 0.36, PR * 0.25, FOCINHO)
    for lado in (-1, 1):
        t.elipse(PX + lado * PR * 0.13, PY + PR * 0.3, PR * 0.06, PR * 0.1, hexc("#3D6B2A"))
    chapeu("detalhe", t)
    return t.final()


def chapeu_palha(etapa, t):
    if etapa == "forma":
        t.elipse(PX, PY - PR * 0.78, PR * 1.05, PR * 0.26, hexc("#E8C35A"))
        t.elipse(PX, PY - PR * 0.98, PR * 0.55, PR * 0.36, hexc("#E8C35A"))
    else:
        t.retangulo(PX - PR * 0.55, PY - PR * 0.86, PX + PR * 0.55, PY - PR * 0.74, hexc("#C8443A"))
        for k in range(-2, 3):
            t.linha([(PX + k * PR * 0.2, PY - PR * 1.25), (PX + k * PR * 0.22, PY - PR * 0.92)], hexc("#C9A243"), 0.8)


def gorro_marinheiro(etapa, t):
    if etapa == "forma":
        t.elipse(PX, PY - PR * 0.78, PR * 0.74, PR * 0.48, hexc("#2F4F8F"))
        t.elipse(PX, PY - PR * 1.28, PR * 0.2, PR * 0.2, BRANCO)
    else:
        t.retangulo(PX - PR * 0.74, PY - PR * 0.64, PX + PR * 0.74, PY - PR * 0.5, BRANCO, raio=1)
        t.retangulo(PX - PR * 0.6, PY - PR * 1.0, PX + PR * 0.6, PY - PR * 0.9, hexc("#5A7FC2"))


def capacete(etapa, t):
    if etapa == "forma":
        t.elipse(PX, PY - PR * 0.68, PR * 0.82, PR * 0.56, hexc("#6B7A5A"))
        t.retangulo(PX - PR * 0.92, PY - PR * 0.66, PX + PR * 0.92, PY - PR * 0.52, hexc("#55623F"), raio=2)
    else:
        for lado in (-1, 1):  # óculos de proteção no capacete
            t.elipse(PX + lado * PR * 0.3, PY - PR * 0.88, PR * 0.2 + 1, PR * 0.15 + 1, CONTORNO)
            t.elipse(PX + lado * PR * 0.3, PY - PR * 0.88, PR * 0.2, PR * 0.15, hexc("#C9E7F5"))
        t.linha([(PX - PR * 0.1, PY - PR * 0.88), (PX + PR * 0.1, PY - PR * 0.88)], CONTORNO, 1.2)


def chapeu_banda(etapa, t):
    if etapa == "forma":
        t.retangulo(PX - PR * 0.55, PY - PR * 1.42, PX + PR * 0.55, PY - PR * 0.62, hexc("#C8443A"), raio=3)
        t.elipse(PX, PY - PR * 0.62, PR * 0.7, PR * 0.14, hexc("#2B2B2B"))
        t.elipse(PX + PR * 0.2, PY - PR * 1.62, PR * 0.18, PR * 0.32, hexc("#F4F1EA"))  # pluma
    else:
        t.retangulo(PX - PR * 0.55, PY - PR * 1.42, PX + PR * 0.55, PY - PR * 1.3, hexc("#E8C35A"))
        t.linha([(PX - PR * 0.4, PY - PR * 1.12), (PX, PY - PR * 0.86), (PX + PR * 0.4, PY - PR * 1.12)],
                hexc("#E8C35A"), 1.4)


# Armas: desenhadas apontando para a direita. O pivô (onde o porco segura)
# fica em (12, 20) numa tela 64×40; o ConstrutorAssets usa esse ponto.
def arma(tipo):
    t = Tela(64, 40)
    if tipo == "estilingue":
        madeira = hexc("#9B6A3C")
        t.linha([(10, 20), (32, 20)], madeira, 5)
        t.linha([(32, 20), (46, 9)], madeira, 4.4)
        t.linha([(32, 20), (46, 31)], madeira, 4.4)
        t.contornar(2.0)
        t.linha([(46, 9), (41, 20), (46, 31)], hexc("#C8443A"), 1.6)
        t.elipse(41, 20, 3, 3, hexc("#8E8E86"))
    elif tipo == "arpao":
        t.retangulo(8, 16, 40, 24, hexc("#8A5A33"), raio=3)
        t.retangulo(36, 14, 44, 26, hexc("#5A5F66"), raio=2)
        t.linha([(40, 20), (56, 20)], hexc("#B8BEC6"), 2.4)
        t.poligono([(56, 15), (63, 20), (56, 25)], hexc("#D7DCE2"))
        t.contornar(2.0)
    elif tipo == "canhao":
        t.retangulo(10, 13, 46, 27, hexc("#5A5F66"), raio=4)
        t.retangulo(42, 10, 52, 30, hexc("#474C52"), raio=3)
        t.elipse(18, 28, 6, 6, hexc("#7A4E2C"))
        t.contornar(2.0)
        t.elipse(47, 20, 3, 6, hexc("#2B2B2B"))
        t.linha([(14, 16), (40, 16)], mais_claro(hexc("#5A5F66"), 0.3), 1.4)
    return t.final()


def tambor():
    t = Tela(64, 48)
    t.retangulo(14, 14, 50, 38, hexc("#C8443A"), raio=4)
    t.elipse(32, 14, 18, 6, hexc("#F4F1EA"))
    for lado in (-1, 1):  # baquetas
        t.linha([(32 + lado * 6, 10), (32 + lado * 20, 2)], hexc("#9B6A3C"), 2.4)
        t.elipse(32 + lado * 21, 2, 3, 3, hexc("#F4F1EA"))
    t.contornar(2.0)
    for k in range(4):  # cordas em zigue-zague
        x = 17 + k * 9
        t.linha([(x, 18), (x + 4.5, 34), (x + 9, 18)], hexc("#E8C35A"), 1.2)
    return t.final()


# Acessórios das especializações: tela 80×80 alinhada com o corpo do porco.
def acessorio(nome):
    t = Tela(80, 80)
    if nome == "rajada":  # bolsinha de pedras na cintura
        t.retangulo(52, 52, 68, 66, hexc("#8A5A33"), raio=3)
        for k in range(3):
            t.elipse(55 + k * 5, 51, 3, 3, hexc("#9A9A92"))
        t.contornar(1.8)
    elif nome == "mira":  # luneta no olho direito
        t.elipse(49, 44, 6, 6, hexc("#5A5F66"))
        t.retangulo(53, 40, 66, 46, hexc("#474C52"), raio=2)
        t.contornar(1.8)
        t.elipse(49, 44, 3.6, 3.6, hexc("#9BD7F2"))
    elif nome == "gigante":  # âncora dourada no gorro
        t.linha([(40, 22), (40, 34)], hexc("#E8C35A"), 2.6)
        t.arco(40, 30, 6, 20, 160, hexc("#E8C35A"), 2.4)
        t.elipse(40, 21, 2.4, 2.4, hexc("#E8C35A"))
        t.contornar(1.6)
    elif nome == "ligeiro":  # raio amarelo de velocidade
        t.poligono([(58, 22), (50, 36), (56, 36), (50, 50), (64, 32), (58, 32), (64, 22)], hexc("#F7D84A"))
        t.contornar(1.6)
    elif nome == "chuva":  # cinturão de bolas de lama
        t.linha([(20, 52), (60, 62)], hexc("#6B4A2E"), 3)
        for k in range(4):
            t.elipse(24 + k * 10, 53 + k * 2.5, 3.6, 3.6, hexc("#7A5230"))
        t.contornar(1.6)
    elif nome == "estrume":  # prendedor no focinho e linhas de fedor
        t.retangulo(36, 50, 44, 58, hexc("#E8C35A"), raio=1.5)
        t.contornar(1.4)
        for k in range(3):
            x = 26 + k * 14
            t.linha([(x, 16), (x + 3, 11), (x - 1, 6)], hexc("#8DBF3A"), 1.6)
    elif nome == "fanfarra":  # medalha no peito
        t.poligono([(36, 50), (44, 50), (42, 58), (38, 58)], hexc("#3E78C9"))
        t.elipse(40, 61, 5, 5, hexc("#F2C14E"))
        t.contornar(1.6)
        t.elipse(40, 61, 2.4, 2.4, mais_claro(hexc("#F2C14E"), 0.5))
    elif nome == "penas":  # coroa de penas atrás do chapéu
        for ang, cor in ((-35, "#C8443A"), (-12, "#F7D84A"), (12, "#3FBF7A"), (35, "#3E78C9")):
            a = math.radians(ang - 90)
            x, y = 40 + math.cos(a) * 20, 34 + math.sin(a) * 20
            t.linha([(40, 30), (x, y)], hexc(cor), 4.2)
        t.contornar(1.6)
    return t.final()


# --------------------------------------------------------------------------
# Tiros e efeitos
# --------------------------------------------------------------------------
def pedrinha():
    t = Tela(18, 18)
    bola_sombreada(t, 9, 9, 6, 5.4, hexc("#9A9A92"))
    t.contornar(1.4)
    return t.final()


def arpao():
    t = Tela(48, 14)
    t.linha([(3, 7), (36, 7)], hexc("#8A5A33"), 2.4)
    t.poligono([(34, 2), (46, 7), (34, 12), (37, 7)], hexc("#C9CED4"))
    t.poligono([(3, 3), (9, 7), (3, 11)], hexc("#C8443A"))
    t.contornar(1.3)
    return t.final()


def bomba():
    t = Tela(26, 26)
    bola_sombreada(t, 13, 13, 9, 8.6, hexc("#7A5230"))
    t.elipse(9, 11, 2, 1.6, hexc("#9B7048"))
    t.elipse(16, 16, 2.4, 1.8, hexc("#5E3E22"))
    t.contornar(1.6)
    return t.final()


def mancha_lama():
    t = Tela(96, 96)
    rnd = random.Random(7)
    cor = hexc("#6E4A2A", 215)
    t.elipse(48, 48, 30, 26, cor)
    for _ in range(11):
        a = rnd.uniform(0, math.tau)
        dist = rnd.uniform(24, 40)
        r = rnd.uniform(4, 9)
        t.elipse(48 + math.cos(a) * dist, 48 + math.sin(a) * dist * 0.9, r, r * 0.85, cor)
    for _ in range(6):
        t.elipse(48 + rnd.uniform(-18, 18), 48 + rnd.uniform(-14, 14), rnd.uniform(3, 6), rnd.uniform(2, 4),
                 hexc("#8A6038", 200))
    return t.final()


def pena():
    """Pena branca: o jogo pinta com a cor de cada pássaro."""
    t = Tela(32, 14)
    t.poligono([(2, 7), (10, 2), (26, 3), (31, 7), (26, 11), (10, 12)], (255, 255, 255, 255))
    t.linha([(2, 7), (29, 7)], (200, 200, 200, 255), 1.0)
    for k in range(4):
        x = 9 + k * 5
        t.linha([(x, 7), (x + 3, 3.5)], (225, 225, 225, 255), 0.7)
        t.linha([(x, 7), (x + 3, 10.5)], (225, 225, 225, 255), 0.7)
    return t.final()


def poeira():
    img = Image.new("RGBA", (32, 32), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.ellipse([4, 4, 28, 28], fill=(240, 232, 214, 255))
    alfa = img.getchannel("A").filter(ImageFilter.GaussianBlur(3.5))
    img.putalpha(alfa)
    return img


def brilho():
    t = Tela(32, 32)
    pts = []
    for k in range(8):
        a = math.radians(k * 45 - 90)
        r = 15 if k % 2 == 0 else 5
        pts.append((16 + math.cos(a) * r, 16 + math.sin(a) * r))
    t.poligono(pts, (255, 248, 200, 255))
    t.elipse(16, 16, 4, 4, BRANCO)
    return t.final()


def nota():
    t = Tela(24, 30)
    t.elipse(8, 22, 6, 4.6, hexc("#F7D84A"))
    t.linha([(13, 22), (13, 5)], hexc("#F7D84A"), 2.4)
    t.poligono([(12, 4), (21, 8), (21, 13), (12, 10)], hexc("#F7D84A"))
    t.contornar(1.6)
    return t.final()


def anel():
    img = Image.new("RGBA", (256 * 2, 256 * 2), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.ellipse([6, 6, 506, 506], outline=(255, 255, 255, 255), width=8)
    d.ellipse([14, 14, 498, 498], fill=(255, 255, 255, 40))
    return img.resize((256, 256), Image.LANCZOS)


def ponto_nivel(cheio):
    t = Tela(16, 16)
    t.elipse(8, 8, 6, 6, CONTORNO)
    t.elipse(8, 8, 4.4, 4.4, hexc("#F2C14E") if cheio else hexc("#5C4630"))
    if cheio:
        t.elipse(6.6, 6.6, 1.5, 1.5, (255, 250, 220, 255))
    return t.final()


def barra():
    return Image.new("RGBA", (8, 8), (255, 255, 255, 255))


# --------------------------------------------------------------------------
# Cenário
# --------------------------------------------------------------------------
def ladrilho_ruido(w, h, base, variacao, semente, manchas=()):
    """Ladrilho que emenda sem costura: ruído por pixel + manchas desenhadas com
    "volta" (o que sai por uma borda entra pela outra)."""
    rnd = random.Random(semente)
    img = Image.new("RGBA", (w, h))
    px = img.load()
    for y in range(h):
        for x in range(w):
            v = rnd.uniform(-variacao, variacao)
            px[x, y] = tuple(max(0, min(255, int(c + v))) for c in base[:3]) + (255,)
    for cor, quantidade, tam in manchas:
        for _ in range(quantidade):
            cx, cy = rnd.randrange(w), rnd.randrange(h)
            rx, ry = rnd.uniform(*tam), rnd.uniform(*tam)
            for oy in (-h, 0, h):
                for ox in (-w, 0, w):
                    ImageDraw.Draw(img).ellipse([cx + ox - rx, cy + oy - ry, cx + ox + rx, cy + oy + ry], fill=cor)
    return img


def grama():
    img = ladrilho_ruido(128, 128, hexc("#79BC57"), 6, 11,
                         manchas=((hexc("#72B451"), 14, (6, 14)), (hexc("#83C561"), 10, (4, 9))))
    rnd = random.Random(12)
    d = ImageDraw.Draw(img)
    for _ in range(90):  # folhinhas
        x, y = rnd.randrange(128), rnd.randrange(128)
        cor = hexc("#5E9E3F") if rnd.random() < 0.6 else hexc("#9BD36F")
        for ox in (-128, 0, 128):
            for oy in (-128, 0, 128):
                d.line([(x + ox, y + oy), (x + ox + rnd.choice((-1, 0, 1)), y + oy - 3)], fill=cor, width=1)
    return img


def terra():
    return ladrilho_ruido(64, 64, hexc("#CDA46E"), 7, 21,
                          manchas=((hexc("#BF955F"), 9, (2, 5)), (hexc("#DDB884"), 7, (1.5, 3)),
                                   (hexc("#A88454"), 6, (1, 2))))


def terra_borda():
    return ladrilho_ruido(64, 64, hexc("#7E5B36"), 6, 31, manchas=((hexc("#6E4E2E"), 8, (2, 5)),))


def arvore():
    t = Tela(96, 104)
    t.retangulo(42, 66, 54, 96, hexc("#7A4E2C"), raio=3)
    for cx, cy, r in ((48, 42, 30), (28, 54, 20), (68, 54, 20), (48, 62, 22)):
        t.elipse(cx, cy, r, r * 0.92, hexc("#4E9A3F"))
    copa = t.camada()
    for cx, cy, r in ((40, 34, 16), (60, 40, 12), (30, 50, 9)):
        copa.elipse(cx, cy, r, r * 0.8, hexc("#6DBB4F"))
    t.colar(copa)
    sombra_copa = t.camada()
    sombra_copa.elipse(56, 70, 26, 10, hexc("#3A7A2F"))
    t.colar(sombra_copa)
    t.contornar(2.4)
    for cx, cy in ((36, 40), (60, 48), (46, 60)):  # frutinhas
        t.elipse(cx, cy, 2.6, 2.6, hexc("#E05A4A"))
    return t.final()


def pedra():
    t = Tela(56, 44)
    t.poligono([(6, 34), (12, 14), (26, 6), (42, 9), (51, 24), (48, 38), (20, 40)], hexc("#9AA0A6"))
    luz = t.camada()
    luz.poligono([(14, 16), (26, 9), (38, 11), (30, 20), (16, 24)], hexc("#C4C9CE"))
    t.colar(luz)
    t.poligono([(48, 38), (51, 24), (42, 30), (20, 40)], hexc("#7A8086"))
    t.contornar(2.2)
    return t.final()


def lago():
    t = Tela(144, 112)
    t.elipse(72, 56, 66, 50, hexc("#8FA36A"))  # margem de capim
    t.elipse(72, 56, 58, 43, hexc("#5DADE2"))
    agua = t.camada()
    agua.elipse(66, 48, 40, 26, hexc("#7CC3EC"))
    t.colar(agua, t.mascara_elipse(72, 56, 58, 43))
    for x, y, w in ((50, 46, 14), (82, 64, 18), (70, 38, 10)):  # marolinhas
        t.arco(x, y, w, 200, 340, (235, 248, 255, 255), 1.4)
    t.elipse(98, 50, 12, 9, hexc("#4E9A3F"))  # vitória-régia
    t.poligono([(98, 50), (110, 46), (110, 52)], hexc("#5DADE2"))
    t.elipse(95, 47, 3, 3, hexc("#F29BC2"))
    t.contornar(2.4)
    return t.final()


def flor(tipo):
    t = Tela(20, 20)
    if tipo == 1:
        for k in range(5):
            a = math.radians(k * 72)
            t.elipse(10 + math.cos(a) * 4.5, 10 + math.sin(a) * 4.5, 3, 3, BRANCO)
        t.elipse(10, 10, 2.6, 2.6, hexc("#F2C14E"))
    else:
        for x, y in ((7, 8), (13, 9), (10, 13)):
            t.elipse(x, y, 3, 3, hexc("#F29BC2"))
            t.elipse(x, y, 1.1, 1.1, hexc("#F7D84A"))
    return t.final()


def torre():
    """A Torre dos Porcos: base de pedra, torre de madeira, telhado vermelho e
    bandeira com um porco. Pivô embaixo (o ConstrutorAssets usa)."""
    W, H = 168, 192
    t = Tela(W, H)
    t.retangulo(30, 128, 138, 186, hexc("#9A9A92"), raio=6)  # base de pedra
    t.retangulo(40, 62, 128, 134, hexc("#B07A45"), raio=4)  # torre de madeira
    t.poligono([(26, 70), (84, 18), (142, 70)], hexc("#C8443A"))  # telhado
    t.linha([(84, 22), (84, 2)], hexc("#5C4630"), 2.4)  # mastro
    t.poligono([(85, 3), (110, 9), (85, 16)], hexc("#8CCB5E"))  # bandeira
    t.contornar(2.6)
    for y in range(136, 186, 12):  # pedras da base
        off = 0 if (y // 12) % 2 == 0 else 9
        for x in range(34 + off, 134, 18):
            t.retangulo(x, y, x + 15, y + 9, hexc("#B0B0A8"), raio=2)
    for x in range(48, 128, 11):  # tábuas
        t.linha([(x, 70), (x, 130)], hexc("#93633A"), 1.2)
    t.linha([(32, 70), (136, 70)], hexc("#9C3A31"), 3)
    t.retangulo(64, 82, 104, 114, CONTORNO, raio=4)  # janela com um porco espiando
    t.retangulo(67, 85, 101, 111, hexc("#3B2A20"), raio=3)
    t.elipse(84, 102, 12, 10, VERDE)
    olhos(t, 84, 99, 4.4, 2.6)
    t.elipse(84, 106, 4.4, 3, FOCINHO)
    t.retangulo(70, 150, 98, 186, hexc("#6B4A2E"), raio=4)  # porta
    t.elipse(92, 168, 2, 2, hexc("#E8C35A"))
    t.elipse(96, 9, 3.4, 3, hexc("#5E9E3F"))  # carinha na bandeira
    return t.final()


# --------------------------------------------------------------------------
# Interface
# --------------------------------------------------------------------------
def caixa(w, h, raio, cor_cima, cor_baixo, borda, esp):
    t = Tela(w, h)
    t.retangulo(0, 0, w, h, borda, raio=raio)
    t.retangulo(esp, esp, w - esp, h - esp, cor_baixo, raio=max(1, raio - esp))
    t.retangulo(esp, esp, w - esp, h * 0.55, cor_cima, raio=max(1, raio - esp))
    return t.final()


def icone(tipo):
    t = Tela(48, 48)
    creme = hexc("#FFF6E0")
    if tipo == "vida":
        t.elipse(17, 19, 10, 10, hexc("#E04B4B"))
        t.elipse(31, 19, 10, 10, hexc("#E04B4B"))
        t.poligono([(7.5, 22), (40.5, 22), (24, 41)], hexc("#E04B4B"))
        t.elipse(15, 16, 3.4, 2.6, (255, 200, 200, 255))
    elif tipo == "pena":
        t.poligono([(10, 40), (14, 22), (26, 8), (38, 6), (36, 18), (26, 32)], hexc("#F7E7B4"))
        t.linha([(9, 41), (34, 10)], hexc("#C9A243"), 1.6)
    elif tipo == "play":
        t.poligono([(15, 9), (39, 24), (15, 39)], creme)
    elif tipo == "pausa":
        t.retangulo(13, 10, 21, 38, creme, raio=2)
        t.retangulo(27, 10, 35, 38, creme, raio=2)
    elif tipo == "rapido":
        t.poligono([(8, 11), (24, 24), (8, 37)], creme)
        t.poligono([(24, 11), (40, 24), (24, 37)], creme)
    elif tipo == "menu":
        for y in (13, 23, 33):
            t.retangulo(10, y - 3, 38, y + 3, creme, raio=2)
    elif tipo == "ninho":
        t.elipse(24, 32, 18, 9, hexc("#9B6A3C"))
        t.elipse(18, 25, 6, 7, hexc("#F4F1EA"))
        t.elipse(29, 24, 6, 7, hexc("#BFE3F2"))
        t.elipse(24, 34, 16, 6, hexc("#B9834F"))
        for x in range(10, 40, 6):
            t.linha([(x, 31), (x + 5, 37)], hexc("#7A4E2C"), 1.2)
    elif tipo == "reiniciar":
        t.arco(24, 25, 13, 40, 330, creme, 5)
        t.poligono([(32, 6), (41, 16), (29, 18)], creme)
    t.contornar(2.2)
    return t.final()


def medalha(cor):
    t = Tela(64, 80)
    t.poligono([(18, 2), (30, 2), (36, 34), (24, 34)], hexc("#C8443A"))
    t.poligono([(34, 2), (46, 2), (40, 34), (28, 34)], hexc("#3E78C9"))
    bola_sombreada(t, 32, 52, 22, 22, hexc(cor))
    t.contornar(2.2)
    t.elipse(32, 52, 14, 14, mais_claro(hexc(cor), 0.25))
    t.poligono([(26, 60), (30, 46), (37, 40), (40, 44), (34, 54)], mais_escuro(hexc(cor), 0.7))  # uma pena
    return t.final()


def logo():
    """Título "Revoada" com contorno grosso e um porquinho e um pardal nas pontas."""
    W, H = 512, 136
    img = Image.new("RGBA", (W * S, H * S), (0, 0, 0, 0))
    try:
        fonte = ImageFont.truetype(FONTE, 92 * S)
    except OSError:
        fonte = ImageFont.load_default()
    texto = "Revoada"
    caixa_txt = ImageDraw.Draw(img).textbbox((0, 0), texto, font=fonte)
    pos = ((W * S - (caixa_txt[2] - caixa_txt[0])) / 2 - caixa_txt[0], (H * S - (caixa_txt[3] - caixa_txt[1])) / 2 - caixa_txt[1] - 6 * S)
    borda = Image.new("L", img.size, 0)
    ImageDraw.Draw(borda).text(pos, texto, font=fonte, fill=255, stroke_width=9 * S, stroke_fill=255)
    letras = Image.new("L", img.size, 0)
    ImageDraw.Draw(letras).text(pos, texto, font=fonte, fill=255)
    img.paste(Image.new("RGBA", img.size, CONTORNO), mask=borda)
    grad = Image.new("RGBA", img.size)
    gd = ImageDraw.Draw(grad)
    for y in range(H * S):
        f = y / (H * S)
        cor = tuple(int(a + (b - a) * f) for a, b in zip((255, 236, 150), (240, 150, 60))) + (255,)
        gd.line([(0, y), (W * S, y)], fill=cor)
    img.paste(grad, mask=letras)
    final = img.resize((W, H), Image.LANCZOS)
    p = porco(chapeu_palha).resize((88, 88), Image.LANCZOS)
    final.alpha_composite(p, (0, 44))
    b = pardal().resize((70, 70), Image.LANCZOS)
    final.alpha_composite(b, (W - 72, 4))
    return final


# --------------------------------------------------------------------------
def gerar():
    os.makedirs(SAIDA, exist_ok=True)
    feitos = []
    # Cenário
    salvar(grama(), "grama", feitos)
    salvar(terra(), "terra", feitos)
    salvar(terra_borda(), "terra_borda", feitos)
    salvar(arvore(), "arvore", feitos)
    salvar(pedra(), "pedra", feitos)
    salvar(lago(), "lago", feitos)
    salvar(flor(1), "flor_1", feitos)
    salvar(flor(2), "flor_2", feitos)
    salvar(torre(), "torre", feitos)
    # Pássaros
    salvar(pardal(), "pardal", feitos)
    salvar(sabia(), "sabia", feitos)
    salvar(beija_flor(), "beija_flor", feitos)
    salvar(pata(), "pata", feitos)
    salvar(pata(filhote=True), "patinho", feitos)
    salvar(gaviao(), "gaviao", feitos)
    salvar(da_arte("arara_azul.png", 72, 72, 66, 70), "arara", feitos)
    salvar(da_arte("urubu.png", 72, 72, 62, 71), "urubu", feitos)
    salvar(sombra(), "sombra", feitos)
    # Porcos
    salvar(porco(chapeu_palha), "porco_estilingueiro", feitos)
    salvar(porco(gorro_marinheiro), "porco_arpoador", feitos)
    salvar(porco(capacete), "porco_bombardeiro", feitos)
    salvar(porco(chapeu_banda), "porco_tamboreiro", feitos)
    for tipo in ("estilingue", "arpao", "canhao"):
        salvar(arma(tipo), "arma_" + tipo, feitos)
    salvar(tambor(), "arma_tambor", feitos)
    for nome in ("rajada", "mira", "gigante", "ligeiro", "chuva", "estrume", "fanfarra", "penas"):
        salvar(acessorio(nome), "acc_" + nome, feitos)
    # Tiros e efeitos
    salvar(pedrinha(), "pedrinha", feitos)
    salvar(arpao(), "arpao", feitos)
    salvar(bomba(), "bomba", feitos)
    salvar(mancha_lama(), "mancha_lama", feitos)
    salvar(pena(), "pena", feitos)
    salvar(poeira(), "poeira", feitos)
    salvar(brilho(), "brilho", feitos)
    salvar(nota(), "nota", feitos)
    salvar(anel(), "anel", feitos)
    salvar(ponto_nivel(True), "ponto_nivel", feitos)
    salvar(ponto_nivel(False), "ponto_nivel_vazio", feitos)
    salvar(barra(), "barra", feitos)
    # Interface
    salvar(caixa(96, 96, 18, hexc("#FFF6E0"), hexc("#F6E7C4"), hexc("#6B4A2E"), 4), "painel", feitos)
    salvar(caixa(64, 64, 14, hexc("#9BD36F"), hexc("#6FAE4A"), hexc("#3D5E28"), 3), "botao", feitos)
    for tipo in ("vida", "pena", "play", "pausa", "rapido", "menu", "ninho", "reiniciar"):
        salvar(icone(tipo), "icone_" + tipo, feitos)
    salvar(logo(), "logo", feitos)
    for nome, cor in (("bronze", "#CD7F32"), ("prata", "#C3C7CF"), ("ouro", "#F2C14E")):
        salvar(medalha(cor), "medalha_" + nome, feitos)
    return feitos


def contato(feitos, pasta):
    """Folha de contato: tudo lado a lado sobre grama, com o nome embaixo."""
    os.makedirs(pasta, exist_ok=True)
    imgs = [(n, Image.open(os.path.join(SAIDA, n + ".png")).convert("RGBA")) for n in feitos]
    try:
        fonte = ImageFont.truetype("/System/Library/Fonts/Supplemental/Arial.ttf", 11)
    except OSError:
        fonte = ImageFont.load_default()
    colunas, cel = 8, 150
    linhas = math.ceil(len(imgs) / colunas)
    folha = Image.new("RGBA", (colunas * cel, linhas * cel), hexc("#79BC57"))
    d = ImageDraw.Draw(folha)
    for i, (n, img) in enumerate(imgs):
        x, y = (i % colunas) * cel, (i // colunas) * cel
        esc = min(1.0, (cel - 24) / max(img.width, img.height))
        mini = img.resize((max(1, int(img.width * esc)), max(1, int(img.height * esc))), Image.LANCZOS)
        folha.alpha_composite(mini, (x + (cel - mini.width) // 2, y + (cel - 18 - mini.height) // 2))
        d.text((x + 4, y + cel - 16), n, fill=(20, 30, 10, 255), font=fonte)
    caminho = os.path.join(pasta, "contato.png")
    folha.save(caminho)
    return caminho


def main():
    feitos = gerar()
    print("%d imagens em %s" % (len(feitos), SAIDA))
    if len(sys.argv) > 1:
        print("folha de contato:", contato(feitos, sys.argv[1]))


if __name__ == "__main__":
    main()
