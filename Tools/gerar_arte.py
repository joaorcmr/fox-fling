#!/usr/bin/env python3
"""Gera toda a arte do Fox Fling em PNG (Assets/FoxFling/Art).

Uso:  python3 Tools/gerar_arte.py [pasta_contato]

Escala: 80 pixels = 1 unidade do Unity (PPU 80). A câmera mostra 15×12 unidades,
então o fundo tem 1200×960. Personagens são desenhados 4× maiores e reduzidos
(antialias); blocos são desenhados pixel a pixel porque precisam repetir sem
emenda no modo "Tiled" do SpriteRenderer. A raposa normal e o inimigo (tubarão)
vêm da arte em Art/Fox e Art/Enemy, reduzida para esse mesmo tamanho.
"""
import math
import os
import random
import sys

from PIL import Image, ImageChops, ImageDraw, ImageFilter, ImageFont

RAIZ = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SAIDA = os.path.join(RAIZ, "Assets", "FoxFling", "Art")
S = 4  # fator de supersampling

CONTORNO = (43, 29, 20, 255)
BRANCO = (255, 255, 255, 255)


def hexc(h, a=255):
    h = h.lstrip("#")
    return (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16), a)


def salvar(img, nome):
    img.save(os.path.join(SAIDA, nome + ".png"))
    return img


def tela(w, h):
    img = Image.new("RGBA", (w * S, h * S), (0, 0, 0, 0))
    return img, ImageDraw.Draw(img)


def reduzir(img):
    return img.resize((img.width // S, img.height // S), Image.LANCZOS)


def sc(*pts):
    """Converte coordenadas 1× para a tela 4×."""
    return [p * S for p in pts]


def elipse(d, cx, cy, rx, ry, cor):
    d.ellipse(sc(cx - rx, cy - ry, cx + rx, cy + ry), fill=cor)


def mascara_vazia(w, h):
    return Image.new("L", (w * S, h * S), 0)


def dilatar(mascara, raio_1x):
    tamanho = int(raio_1x * S) * 2 + 1
    return mascara.filter(ImageFilter.MaxFilter(tamanho))


def pintar(base, mascara, cor):
    camada = Image.new("RGBA", base.size, cor)
    base.alpha_composite(Image.composite(camada, Image.new("RGBA", base.size, (0, 0, 0, 0)), mascara))


def colar_com_mascara(base, camada, mascara):
    vazio = Image.new("RGBA", base.size, (0, 0, 0, 0))
    base.alpha_composite(Image.composite(camada, vazio, mascara))


def gradiente_vertical(w, h, topo, baixo):
    img = Image.new("RGBA", (w, h))
    px = img.load()
    for y in range(h):
        t = y / max(1, h - 1)
        c = tuple(int(topo[i] + (baixo[i] - topo[i]) * t) for i in range(4))
        for x in range(w):
            px[x, y] = c
    return img


# --------------------------------------------------------------------------
# Raposas (64×64, bola com raio 26 e centro em (32, 36) → pivô (0.5, 0.4375))
# --------------------------------------------------------------------------
def raposa(tipo):
    W = H = 64
    cx, cy, r = 32, 36, 26
    if tipo == "bomba":
        pelo, sombra, luz, barriga, orelha_in = hexc("4d5160"), hexc("373a46"), hexc("6e7385"), hexc("d5d9e2"), hexc("8a8fa0")
    else:
        pelo, sombra, luz, barriga, orelha_in = hexc("f08a24"), hexc("c8620f"), hexc("ffb35c"), hexc("fff3e0"), hexc("ffd0a0")

    img, d = tela(W, H)

    # Silhueta (cauda + orelhas + bola) → contorno por dilatação.
    sil = mascara_vazia(W, H)
    ds = ImageDraw.Draw(sil)
    ds.ellipse(sc(cx - r, cy - r, cx + r, cy + r), fill=255)
    orelhas = [
        [(cx - 5, cy - 21), (cx - 23, cy - 13), (cx - 17, cy - 32)],
        [(cx + 5, cy - 21), (cx + 23, cy - 13), (cx + 17, cy - 32)],
    ]
    for o in orelhas:
        ds.polygon([tuple(sc(*p)) for p in o], fill=255)
    cauda = (cx - 29, cy + 4, cx - 9, cy + 22)
    ds.ellipse(sc(*cauda), fill=255)
    pintar(img, dilatar(sil, 2.2), CONTORNO)
    pintar(img, sil, pelo)

    # Orelhas por dentro e ponta da cauda.
    for o in orelhas:
        cx_o = sum(p[0] for p in o) / 3
        cy_o = sum(p[1] for p in o) / 3
        menor = [(cx_o + (p[0] - cx_o) * 0.55, cy_o + (p[1] - cy_o) * 0.55 + 1) for p in o]
        d.polygon([tuple(sc(*p)) for p in menor], fill=orelha_in)
    ponta = mascara_vazia(W, H)
    ImageDraw.Draw(ponta).ellipse(sc(cx - 29, cy + 6, cx - 21, cy + 20), fill=255)
    pintar(img, ImageChops.multiply(ponta, sil), barriga)

    # Detalhes recortados pela bola.
    bola = mascara_vazia(W, H)
    ImageDraw.Draw(bola).ellipse(sc(cx - r, cy - r, cx + r, cy + r), fill=255)
    det, dd = tela(W, H)
    dd.ellipse(sc(cx - r, cy - r, cx + r, cy + r), fill=sombra)
    dd.ellipse(sc(cx - r - 3, cy - r - 3, cx + r - 4, cy + r - 5), fill=pelo)
    elipse(dd, cx + 2, cy + 20, 17, 10, barriga)          # peito
    elipse(dd, cx + 16, cy + 5, 12, 8, barriga)           # focinho
    elipse(dd, cx - 13, cy - 13, 7, 4, luz)               # brilho
    if tipo == "ninja":
        dd.rectangle(sc(cx - r, cy - 14, cx + r, cy - 2), fill=hexc("262a40"))
        dd.rectangle(sc(cx - r, cy - 14, cx + r, cy - 12.5), fill=hexc("3d4466"))
    colar_com_mascara(img, det, bola)

    # Rosto olhando para a direita (para onde ela voa).
    if tipo == "ninja":
        # Faixa amarrada atrás da cabeça.
        for (a, b) in [((cx - 22, cy - 10), (cx - 28, cy - 18)), ((cx - 22, cy - 6), (cx - 28, cy + 1))]:
            d.line(sc(*a, *b), fill=CONTORNO, width=int(6 * S))
            d.line(sc(*a, *b), fill=hexc("262a40"), width=int(3.4 * S))
        for ex in (cx + 6, cx + 17):
            elipse(d, ex, cy - 8, 5.2, 3.2, CONTORNO)
            elipse(d, ex, cy - 8, 4.2, 2.3, BRANCO)
            elipse(d, ex + 1.6, cy - 8, 1.8, 1.8, CONTORNO)
    else:
        for ex in (cx + 6, cx + 17):
            elipse(d, ex, cy - 7, 5.6, 5.6, CONTORNO)
            elipse(d, ex, cy - 7, 4.4, 4.4, BRANCO)
            elipse(d, ex + 1.7, cy - 6.5, 2.2, 2.2, CONTORNO)
            elipse(d, ex + 0.9, cy - 7.8, 0.8, 0.8, BRANCO)
        sobr = (cx + 1, cy - 15, cx + 10, cy - 12.5), (cx + 13, cy - 12.5, cx + 22, cy - 15)
        for x1, y1, x2, y2 in sobr:
            d.line(sc(x1, y1, x2, y2), fill=CONTORNO, width=int(2.4 * S))
    elipse(d, cx + 24, cy + 1.5, 3.2, 2.6, CONTORNO)        # nariz
    d.arc(sc(cx + 11, cy + 2, cx + 21, cy + 10), 20, 160, fill=CONTORNO, width=int(1.6 * S))

    if tipo == "bomba":
        # Pavio aceso entre as orelhas.
        d.line(sc(cx + 1, cy - 25, cx + 4, cy - 30), fill=CONTORNO, width=int(3.6 * S))
        d.line(sc(cx + 1, cy - 25, cx + 4, cy - 30), fill=hexc("c9a46a"), width=int(2 * S))
        for raio_f, cor in ((4.2, hexc("ff7a1a")), (2.6, hexc("ffd84a")), (1.2, BRANCO)):
            elipse(d, cx + 5, cy - 31.5, raio_f, raio_f, cor)
        # Bochechas vermelhas de raiva.
        elipse(d, cx + 4, cy + 3, 3, 2, hexc("e0524a", 200))

    return reduzir(img)


# --------------------------------------------------------------------------
# Arte desenhada à parte (Art/Fox e Art/Enemy), reduzida para o tamanho do jogo
# --------------------------------------------------------------------------
ARTE_RAPOSA = os.path.join(SAIDA, "Fox", "raposinha_principal.png")
ARTE_TUBARAO = os.path.join(SAIDA, "Enemy", "Tubarãozinho pixelado fofo e destemido.png")


def abrir_arte(caminho):
    """Abre a arte, limpa o alfa (o tubarão vem quase opaco, 250–254, e com ruído
    em volta) e recorta as bordas vazias."""
    img = Image.open(caminho).convert("RGBA")
    alfa = img.getchannel("A").point(lambda v: 0 if v < 32 else 255 if v > 200 else v)
    img.putalpha(alfa)
    return img.crop(alfa.getbbox())


def encaixar(arte, W, H, largura, cx, base):
    """Tela 4× com a arte reduzida para `largura` px, centrada em cx e com o pé em y = base."""
    img, d = tela(W, H)
    w = round(largura * S)
    h = round(arte.height * w / arte.width)
    img.alpha_composite(arte.resize((w, h), Image.LANCZOS), (round(cx * S - w / 2), round(base * S - h)))
    return img, d


def raposa_principal():
    # 60 px de largura com o pé em y = 62 (fundo da bola): o corpo cai dentro do
    # colisor (raio 26, centro (32, 36)), igual à raposa desenhada antes.
    img, _ = encaixar(abrir_arte(ARTE_RAPOSA), 64, 64, 60, 32, 62)
    return reduzir(img)


# --------------------------------------------------------------------------
# Inimigo tubarão (72×64; 64 px de largura com o pé em y = 60 → cabe na mesma
# caixa de 0.8 × 0.7 unidades do antigo triângulo, que é o espaço das fases)
# --------------------------------------------------------------------------
def faixa(d, cx, cy, comp, larg, angulo, cor):
    c, s = math.cos(math.radians(angulo)), math.sin(math.radians(angulo))
    cantos = [(-comp / 2, -larg / 2), (comp / 2, -larg / 2), (comp / 2, larg / 2), (-comp / 2, larg / 2)]
    pts = [(cx + x * c - y * s, cy + x * s + y * c) for x, y in cantos]
    d.polygon([tuple(sc(*p)) for p in pts], fill=cor, outline=CONTORNO, width=int(1.1 * S))


def tubarao(ferido):
    W, H = 72, 64
    img, d = encaixar(abrir_arte(ARTE_TUBARAO), W, H, 64, 36, 60)
    if ferido:
        # Pálpebras caídas: a metade de cima de cada olho vira pele azul.
        for ex, ey, rx, ry in ((16.3, 27.6, 5.0, 5.0), (33.6, 28.0, 5.0, 5.4)):
            olho = mascara_vazia(W, H)
            ImageDraw.Draw(olho).ellipse(sc(ex - rx, ey - ry, ex + rx, ey + ry), fill=255)
            metade = mascara_vazia(W, H)
            ImageDraw.Draw(metade).rectangle(sc(0, 0, W, ey + 0.3), fill=255)
            pintar(img, ImageChops.multiply(olho, metade), hexc("5b8cc3"))
            d.line(sc(ex - rx - 0.6, ey + 0.6, ex + rx + 0.6, ey - 0.2), fill=CONTORNO, width=int(1.8 * S))
        # Curativo em X no alto da cabeça.
        for angulo in (35, -35):
            faixa(d, 25, 17.5, 10, 3.4, angulo, hexc("f3c99a"))
        elipse(d, 25, 17.5, 1.4, 1.4, hexc("d99a6c"))
    return reduzir(img)


# --------------------------------------------------------------------------
# Blocos 48×48 com borda de 8 px (modo Tiled: o miolo 32×32 se repete)
# --------------------------------------------------------------------------
BORDA = 8
TB = 48


def bloco(material, vertical=False, rachado=False):
    rnd = random.Random({"madeira": 1, "vidro": 2, "rocha": 3}[material])
    img = Image.new("RGBA", (TB, TB), (0, 0, 0, 0))
    px = img.load()

    if material == "madeira":
        base, veio, luz, sombra, borda = hexc("c98a43"), hexc("a96b2c"), hexc("e3aa62"), hexc("8f5626"), hexc("5c3517")
    elif material == "vidro":
        base, veio, luz, sombra, borda = hexc("a8def2", 150), None, hexc("eefbff", 235), hexc("6fb4cf", 220), hexc("3d7f9c")
    else:
        base, veio, luz, sombra, borda = hexc("8f949c"), None, hexc("b9bec6"), hexc("5f646b"), hexc("3b3e44")

    for y in range(TB):
        for x in range(TB):
            c = base
            if material == "madeira":
                # Veios com período de 32 px para repetir sem emenda.
                u, v = (y, x) if vertical else (x, y)
                onda = math.sin(2 * math.pi * (u - BORDA) / 32.0) * 0.9
                if int(v + onda) % 6 == 0:
                    c = veio
            elif material == "rocha":
                rr = rnd.random()
                if rr < 0.08:
                    c = hexc("7c8189")
                elif rr < 0.14:
                    c = hexc("a4a9b1")
            px[x, y] = c

    if material == "vidro":
        d = ImageDraw.Draw(img)
        for o in (-20, 12, 44):
            for dx in (-32, 0, 32):
                d.line((o + dx, TB, o + dx + TB, 0), fill=(255, 255, 255, 90), width=3)

    # Chanfro: claro em cima/esquerda, escuro embaixo/direita, contorno por fora.
    for i in range(TB):
        for t in (2, 3):
            px[i, t] = luz
            px[t, i] = luz
            px[i, TB - 1 - t] = sombra
            px[TB - 1 - t, i] = sombra
        for t in (0, 1):
            px[i, t] = borda
            px[t, i] = borda
            px[i, TB - 1 - t] = borda
            px[TB - 1 - t, i] = borda
    for (x, y) in ((0, 0), (1, 0), (0, 1), (TB - 1, 0), (TB - 2, 0), (TB - 1, 1),
                   (0, TB - 1), (1, TB - 1), (0, TB - 2), (TB - 1, TB - 1), (TB - 2, TB - 1), (TB - 1, TB - 2)):
        px[x, y] = (0, 0, 0, 0)

    d = ImageDraw.Draw(img)
    if material == "madeira":
        for (x, y) in ((5, 5), (42, 5), (5, 42), (42, 42)):
            d.rectangle((x - 1, y - 1, x, y), fill=borda)
    if material == "rocha":
        for (x, y) in ((14, 20), (30, 12), (25, 32), (38, 26)):
            d.ellipse((x - 3, y - 2, x + 3, y + 2), fill=hexc("80858d"))
            d.point((x - 1, y - 1), fill=luz)

    if rachado:
        cor = {"madeira": hexc("3a2210"), "vidro": hexc("ffffff", 240), "rocha": hexc("2b2d31")}[material]
        trincas = [[(6, 4), (14, 15), (12, 23), (22, 30), (30, 44)],
                   [(14, 15), (24, 13), (33, 6)],
                   [(22, 30), (34, 28), (44, 34)],
                   [(40, 8), (36, 18), (41, 24)]]
        for t in trincas:
            d.line(t, fill=cor, width=1)
    return img


# --------------------------------------------------------------------------
# Cenário
# --------------------------------------------------------------------------
def nuvem(d, x, y, esc):
    for (dx, dy, rx, ry) in ((0, 0, 70, 38), (-60, 14, 50, 30), (62, 12, 52, 30), (20, -24, 48, 34), (-28, -14, 40, 28)):
        d.ellipse((x + (dx - rx) * esc, y + (dy - ry + 8) * esc, x + (dx + rx) * esc, y + (dy + ry + 8) * esc), fill=(196, 226, 242, 255))
    for (dx, dy, rx, ry) in ((0, 0, 70, 38), (-60, 14, 50, 30), (62, 12, 52, 30), (20, -24, 48, 34), (-28, -14, 40, 28)):
        d.ellipse((x + (dx - rx) * esc, y + (dy - ry) * esc, x + (dx + rx) * esc, y + (dy + ry) * esc), fill=(255, 255, 255, 255))


def morro(d, w, base_y, amplitude, periodo, fase, cor):
    pts = [(0, 960)]
    for x in range(0, w + 1, 8):
        y = base_y - amplitude * (0.6 * math.sin(x / periodo + fase) + 0.4 * math.sin(x / (periodo * 0.47) + fase * 2.1))
        pts.append((x, y))
    pts.append((w, 960))
    d.polygon(pts, fill=cor)


def fundo():
    W, H = 1200, 960
    img = gradiente_vertical(W, H, hexc("5ebbe8"), hexc("d4f0fa"))
    d = ImageDraw.Draw(img)
    brilho = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    db = ImageDraw.Draw(brilho)
    for r, a in ((150, 40), (110, 60), (80, 90)):
        db.ellipse((1020 - r, 150 - r, 1020 + r, 150 + r), fill=(255, 250, 210, a))
    img.alpha_composite(brilho.filter(ImageFilter.GaussianBlur(18)))
    d.ellipse((960, 90, 1080, 210), fill=hexc("fff2b0"))
    for (x, y, e) in ((190, 170, 1.0), (560, 110, 0.8), (820, 250, 0.9), (330, 360, 0.6), (1110, 330, 0.7), (680, 420, 0.5)):
        nuvem(d, x, y, e)
    morro(d, W, 690, 60, 120, 0.4, hexc("a9d9c4"))
    morro(d, W, 770, 46, 90, 2.0, hexc("86c77f"))
    # Arbustos nos morros da frente.
    rnd = random.Random(7)
    for x in range(30, W, 140):
        y = 800 + rnd.randint(-10, 10)
        for (dx, rr) in ((-18, 20), (0, 28), (20, 22)):
            d.ellipse((x + dx - rr, y - rr, x + dx + rr, y + rr), fill=hexc("6db462"))
    return img


def chao():
    W, H = 1200, 120
    img = Image.new("RGBA", (W, H), hexc("9c6b38"))
    d = ImageDraw.Draw(img)
    rnd = random.Random(11)
    for y in range(28, H, 22):
        d.line((0, y, W, y + rnd.randint(-2, 2)), fill=hexc("8d5f30"), width=3)
    for _ in range(140):
        x, y = rnd.randint(0, W), rnd.randint(26, H)
        r = rnd.randint(2, 6)
        d.ellipse((x - r, y - r * 0.7, x + r, y + r * 0.7), fill=rnd.choice([hexc("7f552a"), hexc("b5834c"), hexc("aa7a45")]))
    # Grama com borda ondulada.
    pts = [(0, 0), (W, 0)]
    for x in range(W, -1, -10):
        pts.append((x, 20 + 4 * math.sin(x / 13.0) + 2 * math.sin(x / 5.0)))
    d.polygon(pts, fill=hexc("62b23a"))
    pts2 = [(0, 0), (W, 0)]
    for x in range(W, -1, -10):
        pts2.append((x, 8 + 2 * math.sin(x / 9.0)))
    d.polygon(pts2, fill=hexc("78c84a"))
    d.rectangle((0, 0, W, 2), fill=hexc("a5e070"))
    return img


def estilingue():
    W, H = 80, 176
    img, d = tela(W, H)
    pau, luz, faixa = hexc("8b5a2b"), hexc("b37c43"), hexc("5b3a1e")
    segs = [((40, 176), (40, 100), 14), ((40, 108), (16, 18), 11), ((40, 108), (64, 18), 11)]
    for (a, b, w) in segs:
        d.line(sc(*a, *b), fill=CONTORNO, width=int((w + 4) * S))
    for (a, b, w) in segs:
        elipse(d, b[0], b[1], (w + 4) / 2, (w + 4) / 2, CONTORNO)
    for (a, b, w) in segs:
        d.line(sc(*a, *b), fill=pau, width=int(w * S))
        elipse(d, b[0], b[1], w / 2, w / 2, pau)
    d.line(sc(36, 170, 36, 112), fill=luz, width=int(3 * S))
    d.line(sc(34, 104, 15, 26), fill=luz, width=int(2.4 * S))
    d.line(sc(43, 104, 61, 30), fill=luz, width=int(2 * S))
    d.rectangle(sc(32, 124, 48, 140), fill=faixa)
    for (x, y) in ((17, 26), (63, 26)):
        d.line(sc(x - 6, y - 3, x + 6, y + 3), fill=faixa, width=int(5 * S))
    return reduzir(img)


# --------------------------------------------------------------------------
# Interface e efeitos
# --------------------------------------------------------------------------
def circulo(tam, suave=False):
    img, d = tela(tam, tam)
    d.ellipse((S, S, tam * S - S, tam * S - S), fill=BRANCO)
    img = reduzir(img)
    if suave:
        img = Image.new("RGBA", (tam, tam), (0, 0, 0, 0))
        px = img.load()
        c = (tam - 1) / 2
        for y in range(tam):
            for x in range(tam):
                t = max(0.0, 1 - math.hypot(x - c, y - c) / c)
                px[x, y] = (255, 255, 255, int(255 * t * t))
    return img


def estilhaco():
    img, d = tela(16, 16)
    d.polygon([tuple(sc(*p)) for p in ((2, 5), (11, 1), (15, 9), (6, 15))], fill=BRANCO)
    return reduzir(img)


def anel():
    img, d = tela(128, 128)
    d.ellipse(sc(4, 4, 124, 124), outline=BRANCO, width=int(9 * S))
    return reduzir(img).filter(ImageFilter.GaussianBlur(1))


def estrela(cheia):
    img, d = tela(64, 64)
    pts = []
    for i in range(10):
        ang = -math.pi / 2 + i * math.pi / 5
        r = 29 if i % 2 == 0 else 13
        pts.append((32 + r * math.cos(ang), 33 + r * math.sin(ang)))
    d.polygon([tuple(sc(*p)) for p in pts], fill=CONTORNO)
    pts2 = [(32 + (x - 32) * 0.82, 33 + (y - 33) * 0.82) for (x, y) in pts]
    if cheia:
        d.polygon([tuple(sc(*p)) for p in pts2], fill=hexc("ffc93c"))
        pts3 = [(31 + (x - 32) * 0.5, 30 + (y - 33) * 0.5) for (x, y) in pts]
        d.polygon([tuple(sc(*p)) for p in pts3], fill=hexc("ffe48a"))
    else:
        d.polygon([tuple(sc(*p)) for p in pts2], fill=hexc("9aa3ad"))
    return reduzir(img)


def cadeado():
    img, d = tela(48, 48)
    d.arc(sc(13, 5, 35, 29), 180, 360, fill=CONTORNO, width=int(7 * S))
    d.arc(sc(13, 5, 35, 29), 180, 360, fill=hexc("b8bec6"), width=int(3.5 * S))
    d.rounded_rectangle(sc(8, 19, 40, 44), radius=5 * S, fill=hexc("f2b33d"), outline=CONTORNO, width=int(2.5 * S))
    elipse(d, 24, 29, 3.5, 3.5, CONTORNO)
    d.rectangle(sc(22.6, 30, 25.4, 37), fill=CONTORNO)
    return reduzir(img)


def caixa(tam, raio, cor, cor_baixo, borda_cor, espessura):
    img, d = tela(tam, tam)
    d.rounded_rectangle(sc(1, 1, tam - 1, tam - 1), radius=raio * S, fill=borda_cor)
    d.rounded_rectangle(sc(1 + espessura, 1 + espessura, tam - 1 - espessura, tam - 1 - espessura),
                        radius=(raio - espessura) * S, fill=cor_baixo)
    d.rounded_rectangle(sc(1 + espessura, 1 + espessura, tam - 1 - espessura, tam - 1 - espessura - 4),
                        radius=(raio - espessura) * S, fill=cor)
    return reduzir(img)


def icone(tipo):
    img, d = tela(48, 48)
    if tipo == "menu":
        for y in (14, 24, 34):
            d.line(sc(11, y, 37, y), fill=CONTORNO, width=int(8 * S))
            elipse(d, 11, y, 4, 4, CONTORNO)
            elipse(d, 37, y, 4, 4, CONTORNO)
        for y in (14, 24, 34):
            d.line(sc(11, y, 37, y), fill=BRANCO, width=int(4 * S))
            elipse(d, 11, y, 2, 2, BRANCO)
            elipse(d, 37, y, 2, 2, BRANCO)
    elif tipo == "reiniciar":
        for cor, w in ((CONTORNO, 9), (BRANCO, 5)):
            d.arc(sc(10, 10, 38, 38), 40, 330, fill=cor, width=int(w * S))
        seta = [(29, 6), (41, 13), (30, 21)]
        d.polygon([tuple(sc(*p)) for p in seta], fill=BRANCO, outline=CONTORNO, width=int(2 * S))
    else:
        d.polygon([tuple(sc(*p)) for p in ((15, 9), (39, 24), (15, 39))], fill=BRANCO, outline=CONTORNO, width=int(2.5 * S))
    return reduzir(img)


def logo(raposa_arte):
    W, H = 540, 130
    fonte_caminho = "/System/Library/Fonts/Supplemental/Arial Rounded Bold.ttf"
    try:
        fonte = ImageFont.truetype(fonte_caminho, 86 * S)
    except OSError:
        fonte = ImageFont.load_default()
    texto = "Fox Fling"
    img = Image.new("RGBA", (W * S, H * S), (0, 0, 0, 0))
    pos = (118 * S, 16 * S)
    m_borda = Image.new("L", img.size, 0)
    ImageDraw.Draw(m_borda).text(pos, texto, font=fonte, fill=255, stroke_width=7 * S, stroke_fill=255)
    m_texto = Image.new("L", img.size, 0)
    ImageDraw.Draw(m_texto).text(pos, texto, font=fonte, fill=255)
    sombra = ImageChops.offset(m_borda, 0, 5 * S)
    pintar(img, sombra, (43, 29, 20, 110))
    pintar(img, m_borda, CONTORNO)
    colar_com_mascara(img, gradiente_vertical(W * S, H * S, hexc("ffd166"), hexc("f06d1f")), m_texto)
    img = reduzir(img)
    # A arte grande (não o sprite de 64 px), num quadrado de 104 px, com o pé em y = 115.
    rap = raposa_arte.copy()
    rap.thumbnail((104, 104), Image.LANCZOS)
    img.alpha_composite(rap, (8 + (104 - rap.width) // 2, 115 - rap.height))
    return img


def main():
    os.makedirs(SAIDA, exist_ok=True)
    feitos = {}
    feitos["raposa_normal"] = salvar(raposa_principal(), "raposa_normal")
    for t in ("ninja", "bomba"):
        feitos["raposa_" + t] = salvar(raposa(t), "raposa_" + t)
    feitos["inimigo"] = salvar(tubarao(False), "inimigo")
    feitos["inimigo_ferido"] = salvar(tubarao(True), "inimigo_ferido")
    for m in ("madeira", "vidro", "rocha"):
        nome_r = {"madeira": "madeira_rachada", "vidro": "vidro_rachado", "rocha": "rocha_rachada"}[m]
        feitos[m] = salvar(bloco(m), m)
        feitos[nome_r] = salvar(bloco(m, rachado=True), nome_r)
    feitos["madeira_v"] = salvar(bloco("madeira", vertical=True), "madeira_v")
    feitos["madeira_v_rachada"] = salvar(bloco("madeira", vertical=True, rachado=True), "madeira_v_rachada")
    feitos["fundo"] = salvar(fundo(), "fundo")
    feitos["chao"] = salvar(chao(), "chao")
    feitos["estilingue"] = salvar(estilingue(), "estilingue")
    feitos["ponto"] = salvar(circulo(16), "ponto")
    feitos["particula"] = salvar(circulo(32, suave=True), "particula")
    feitos["estilhaco"] = salvar(estilhaco(), "estilhaco")
    feitos["anel"] = salvar(anel(), "anel")
    feitos["estrela"] = salvar(estrela(True), "estrela")
    feitos["estrela_vazia"] = salvar(estrela(False), "estrela_vazia")
    feitos["cadeado"] = salvar(cadeado(), "cadeado")
    feitos["botao"] = salvar(caixa(48, 12, hexc("f7a43b"), hexc("c9701c"), CONTORNO, 2.5), "botao")
    feitos["painel"] = salvar(caixa(64, 18, hexc("fff4df"), hexc("e9d3ad"), CONTORNO, 3), "painel")
    for t in ("menu", "reiniciar", "proxima"):
        feitos["icone_" + t] = salvar(icone(t), "icone_" + t)
    feitos["logo"] = salvar(logo(abrir_arte(ARTE_RAPOSA)), "logo")

    if len(sys.argv) > 1:
        contato(feitos, sys.argv[1])
    print("geradas %d imagens em %s" % (len(feitos), SAIDA))


def contato(feitos, pasta):
    """Folha-contato para conferir a arte (não vai para o projeto)."""
    os.makedirs(pasta, exist_ok=True)
    folha = Image.new("RGBA", (1200, 960), hexc("cfe8f2"))
    folha.alpha_composite(feitos["fundo"])
    folha.alpha_composite(feitos["chao"], (0, 840))
    folha.alpha_composite(feitos["estilingue"], (120, 840 - 176))
    folha.alpha_composite(feitos["raposa_normal"], (128, 840 - 176 - 14))
    x = 20
    for n in ("raposa_normal", "raposa_ninja", "raposa_bomba", "inimigo", "inimigo_ferido"):
        im = feitos[n].resize((feitos[n].width * 2, feitos[n].height * 2), Image.LANCZOS)
        folha.alpha_composite(im, (x, 20))
        x += im.width + 16
    x = 20
    for n in ("madeira", "madeira_rachada", "madeira_v", "vidro", "vidro_rachado", "rocha", "rocha_rachada"):
        # repete o bloco 3×2 como o modo Tiled faria (aproximação)
        b = feitos[n]
        folha.alpha_composite(b.resize((96, 96), Image.NEAREST), (x, 200))
        x += 110
    x = 20
    for n in ("estrela", "estrela_vazia", "cadeado", "botao", "painel", "icone_menu", "icone_reiniciar", "icone_proxima", "ponto", "estilhaco", "anel"):
        folha.alpha_composite(feitos[n], (x, 330))
        x += feitos[n].width + 12
    folha.alpha_composite(feitos["logo"], (600, 420))
    folha.save(os.path.join(pasta, "contato.png"))


if __name__ == "__main__":
    main()
