#!/usr/bin/env python3
"""Gera toda a arte do Fox Tower em PNG (Assets/Revoada/Compartilhado/Arte).

Uso:  python3 Tools/gerar_arte.py [--so nome1,nome2] [pasta_contato]

Com --so, gera só as imagens pedidas (ex.: --so logo,icone_vida) e não mexe
nas outras: útil para não regravar a arte inteira por causa de um ícone.

Escala: 64 pixels = 1 unidade do Unity (PPU 64). Na tela de 600×480 uma
unidade ocupa 32 px, então tudo é desenhado com o dobro da resolução em que
aparece e fica nítido mesmo com o jogo em tela cheia.

Personagens de interface (ícones, logo) são desenhados 4× maiores e reduzidos
(antialias), com o contorno marrom-escuro "inflando" a silhueta inteira
(filtro de máximo), como um adesivo. Os pássaros e os porcos (raposas) são a
arte do Iran, que entra reduzida a partir de Arte/Fontes (ver `da_arte`).
Ladrilhos (grama e terra) são desenhados pixel a pixel, para emendarem sem
costura.
"""
import math
import os
import random
import sys

from PIL import Image, ImageChops, ImageDraw, ImageFilter, ImageFont

RAIZ = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SAIDA = os.path.join(RAIZ, "Assets", "Revoada", "Compartilhado", "Arte")
FONTES = os.path.join(SAIDA, "Fontes")
S = 4  # supersampling

CONTORNO = (43, 29, 20, 255)
BRANCO = (255, 255, 255, 255)
# Arial Rounded Bold: o caminho do macOS e, depois, o do Windows.
FONTES_TITULO = (
    "/System/Library/Fonts/Supplemental/Arial Rounded Bold.ttf",
    "C:/Windows/Fonts/ARLRDBD.TTF",
    "C:/Windows/Fonts/arialbd.ttf",
)
FONTES_TEXTO = ("/System/Library/Fonts/Supplemental/Arial.ttf", "C:/Windows/Fonts/arial.ttf")


def fonte(caminhos, tamanho):
    """A primeira fonte da lista que existir neste computador."""
    for caminho in caminhos:
        try:
            return ImageFont.truetype(caminho, tamanho)
        except OSError:
            pass
    return ImageFont.load_default()


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


SO = None  # com --so, os nomes das únicas imagens que serão gravadas


def salvar(img, nome, feitos):
    if SO is not None and nome not in SO:
        return
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
# Porcos: agora raposas, a arte do Iran reduzida a partir de Arte/Fontes
# (ver gerar(); o corpo inteiro já vem com a arma, então só a tela do
# acessório de especialização continua desenhada por código).
# --------------------------------------------------------------------------

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
    elif nome == "moedas":  # leque de moedas de ouro atrás do chapéu
        centros = []
        for ang in (-38, -13, 13, 38):
            a = math.radians(ang - 90)
            centros.append((40 + math.cos(a) * 19, 33 + math.sin(a) * 19))
        for x, y in centros:
            t.elipse(x, y, 5.2, 5.2, hexc("#F2C14E"))
        t.contornar(1.6)
        for x, y in centros:
            t.elipse(x, y, 3.1, 3.1, hexc("#D9A235"))
            t.elipse(x - 0.9, y - 0.9, 1.5, 1.5, hexc("#FBE08A"))
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


VERDE = hexc("#8CCB5E")
FOCINHO = hexc("#B9E58F")


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
    elif tipo == "moeda":  # moeda de ouro com a borda mais escura e um brilho
        t.elipse(24, 25, 17, 17, hexc("#D9A235"))
        t.elipse(24, 24, 17, 16, hexc("#F2C14E"))
        t.elipse(24, 24, 11.5, 11, hexc("#D9A235"))
        t.elipse(24, 24.8, 10, 9.6, hexc("#F7D774"))
        t.elipse(19.5, 19.5, 3.4, 2.4, hexc("#FFF3C4"))
    elif tipo == "play":
        t.poligono([(15, 9), (39, 24), (15, 39)], creme)
    elif tipo == "rapido":
        t.poligono([(8, 11), (24, 24), (8, 37)], creme)
        t.poligono([(24, 11), (40, 24), (24, 37)], creme)
    elif tipo == "menu":
        for y in (13, 23, 33):
            t.retangulo(10, y - 3, 38, y + 3, creme, raio=2)
    elif tipo == "cofrinho":  # porquinho de louça com uma moeda entrando
        rosa, rosa_escuro = hexc("#F4A6B8"), hexc("#DB7F97")
        t.elipse(22, 9, 6, 6, hexc("#F2C14E"))                      # a moeda
        for x in (14, 30):                                          # patas
            t.retangulo(x - 3, 36, x + 3, 43, rosa_escuro, raio=1.5)
        t.poligono([(12, 20), (15, 11), (21, 19)], rosa_escuro)     # orelha
        t.elipse(23, 29, 16, 12, rosa)                              # corpo
        t.elipse(39, 29, 5, 6, rosa_escuro)                         # focinho
        t.contornar(2.2)
        t.elipse(22, 9, 3.2, 3.2, hexc("#D9A235"))
        t.retangulo(16, 18.2, 28, 20.4, CONTORNO, raio=1)           # a fenda
        t.elipse(31, 25, 1.6, 1.9, CONTORNO)                        # olho
        t.elipse(38, 28, 0.9, 1.4, hexc("#8E4A5E"))
        t.elipse(40.6, 28, 0.9, 1.4, hexc("#8E4A5E"))
        t.elipse(16, 25, 4.5, 2.8, mais_claro(rosa, 0.45))
        return t.final()
    elif tipo == "reiniciar":
        t.arco(24, 25, 13, 40, 330, creme, 5)
        t.poligono([(32, 6), (41, 16), (29, 18)], creme)
    elif tipo == "reparo":  # o coração da vida com uma cruz branca: reparar a Torre
        t.elipse(17, 19, 10, 10, hexc("#E04B4B"))
        t.elipse(31, 19, 10, 10, hexc("#E04B4B"))
        t.poligono([(7.5, 22), (40.5, 22), (24, 41)], hexc("#E04B4B"))
        t.retangulo(20.5, 14, 27.5, 31, creme, raio=1.5)
        t.retangulo(13, 18.5, 35, 25.5, creme, raio=1.5)
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
    """Título "Fox Tower" com contorno grosso e um porquinho e um pardal nas pontas."""
    W, H = 512, 136
    img = Image.new("RGBA", (W * S, H * S), (0, 0, 0, 0))
    texto = "Fox Tower"
    # A maior letra que cabe entre o porquinho e o pardal.
    tamanho = 92
    while True:
        letra = fonte(FONTES_TITULO, tamanho * S)
        caixa_txt = ImageDraw.Draw(img).textbbox((0, 0), texto, font=letra)
        if caixa_txt[2] - caixa_txt[0] <= 392 * S or tamanho <= 40:
            break
        tamanho -= 2
    pos = ((W * S - (caixa_txt[2] - caixa_txt[0])) / 2 - caixa_txt[0], (H * S - (caixa_txt[3] - caixa_txt[1])) / 2 - caixa_txt[1] - 2 * S)
    borda = Image.new("L", img.size, 0)
    ImageDraw.Draw(borda).text(pos, texto, font=letra, fill=255, stroke_width=8 * S, stroke_fill=255)
    letras = Image.new("L", img.size, 0)
    ImageDraw.Draw(letras).text(pos, texto, font=letra, fill=255)
    img.paste(Image.new("RGBA", img.size, CONTORNO), mask=borda)
    grad = Image.new("RGBA", img.size)
    gd = ImageDraw.Draw(grad)
    for y in range(H * S):
        f = y / (H * S)
        cor = tuple(int(a + (b - a) * f) for a, b in zip((255, 236, 150), (240, 150, 60))) + (255,)
        gd.line([(0, y), (W * S, y)], fill=cor)
    img.paste(grad, mask=letras)
    final = img.resize((W, H), Image.LANCZOS)
    p = da_arte("raposa_estilingueiro.png", 80, 80, 74, 78).resize((80, 80), Image.LANCZOS)
    final.alpha_composite(p, (0, 50))
    b = da_arte("aranha.png", 56, 56, 50, 54).resize((64, 64), Image.LANCZOS)
    final.alpha_composite(b, (W - 64, 2))
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
    # Pássaros (issue #15: viraram os bichos do Iran, reduzidos a partir de
    # Arte/Fontes, do mesmo jeito que a arara e o urubu já eram)
    salvar(da_arte("aranha.png", 56, 56, 50, 54), "pardal", feitos)
    salvar(da_arte("touro.png", 60, 60, 54, 58), "sabia", feitos)
    salvar(da_arte("tigre.png", 62, 56, 54, 54), "beija_flor", feitos)
    salvar(da_arte("arara_azul.png", 68, 68, 62, 66), "pata", feitos)
    salvar(da_arte("arara_azul_filhote.png", 44, 44, 40, 42), "patinho", feitos)
    salvar(da_arte("tubarao_chefe.png", 124, 124, 112, 121), "gaviao", feitos)
    salvar(da_arte("jacare.png", 72, 72, 66, 70), "arara", feitos)
    salvar(da_arte("leao.png", 72, 72, 62, 71), "urubu", feitos)
    salvar(sombra(), "sombra", feitos)
    # Porcos (idem: viraram as raposas do Iran; a arte já traz a arma embutida)
    salvar(da_arte("raposa_estilingueiro.png", 80, 80, 74, 78), "porco_estilingueiro", feitos)
    salvar(da_arte("raposa_ninja.png", 80, 80, 74, 78), "porco_arpoador", feitos)
    salvar(da_arte("raposa_bombardeiro.png", 80, 80, 74, 78), "porco_bombardeiro", feitos)
    salvar(da_arte("raposa_tamboreiro.png", 80, 80, 74, 78), "porco_tamboreiro", feitos)
    for tipo in ("estilingue", "arpao", "canhao"):
        salvar(arma(tipo), "arma_" + tipo, feitos)
    salvar(tambor(), "arma_tambor", feitos)
    for nome in ("rajada", "mira", "gigante", "ligeiro", "chuva", "estrume", "fanfarra", "moedas"):
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
    for tipo in ("vida", "moeda", "play", "rapido", "menu", "cofrinho", "reiniciar", "reparo"):
        salvar(icone(tipo), "icone_" + tipo, feitos)
    salvar(logo(), "logo", feitos)
    for nome, cor in (("bronze", "#CD7F32"), ("prata", "#C3C7CF"), ("ouro", "#F2C14E")):
        salvar(medalha(cor), "medalha_" + nome, feitos)
    return feitos


def contato(feitos, pasta):
    """Folha de contato: tudo lado a lado sobre grama, com o nome embaixo."""
    os.makedirs(pasta, exist_ok=True)
    imgs = [(n, Image.open(os.path.join(SAIDA, n + ".png")).convert("RGBA")) for n in feitos]
    letra = fonte(FONTES_TEXTO, 11)
    colunas, cel = 8, 150
    linhas = math.ceil(len(imgs) / colunas)
    folha = Image.new("RGBA", (colunas * cel, linhas * cel), hexc("#79BC57"))
    d = ImageDraw.Draw(folha)
    for i, (n, img) in enumerate(imgs):
        x, y = (i % colunas) * cel, (i // colunas) * cel
        esc = min(1.0, (cel - 24) / max(img.width, img.height))
        mini = img.resize((max(1, int(img.width * esc)), max(1, int(img.height * esc))), Image.LANCZOS)
        folha.alpha_composite(mini, (x + (cel - mini.width) // 2, y + (cel - 18 - mini.height) // 2))
        d.text((x + 4, y + cel - 16), n, fill=(20, 30, 10, 255), font=letra)
    caminho = os.path.join(pasta, "contato.png")
    folha.save(caminho)
    return caminho


def main():
    global SO
    args = sys.argv[1:]
    if "--so" in args:
        i = args.index("--so")
        SO = set(args[i + 1].split(","))
        del args[i:i + 2]
    feitos = gerar()
    print("%d imagens em %s" % (len(feitos), SAIDA))
    if SO is not None and SO - set(feitos):
        print("não existem:", ", ".join(sorted(SO - set(feitos))))
    if args:
        print("folha de contato:", contato(feitos, args[0]))


if __name__ == "__main__":
    main()
