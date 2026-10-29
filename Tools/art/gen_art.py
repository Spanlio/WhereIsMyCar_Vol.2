"""Generates the Hanoi / menu artwork for WhereIsMyCar Vol.2.

Writes into Assets/Sprites/hanojas tornis/: the board with a transparent sky
(from towerOfHanoi.png, which is left untouched), the floor strip, the
day/sunset/night skies, the cherry trees, the menu preview and the star icon;
and Assets/Sprites/menu_background.png. Output is the same on every run.

Setup (once, from the project folder):
    python3 -m venv Tools/art/.venv
    Tools/art/.venv/bin/pip install -r Tools/art/requirements.txt

Run (from the project folder), then Tools > Hub > Build All in Unity:
    Tools/art/.venv/bin/python Tools/art/gen_art.py Assets
"""
import math
import random
import sys
from collections import deque

import numpy as np
from PIL import Image, ImageDraw, ImageFilter

ASSETS = sys.argv[1]
HANOI = f"{ASSETS}/Sprites/hanojas tornis"
FLOOR_Y = 553
random.seed(7)


def lerp(a, b, t):
    return tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(len(a)))


def vertical_gradient(w, h, stops):
    """stops: list of (t, rgb)."""
    img = Image.new("RGB", (w, h))
    px = img.load()
    col = []
    for y in range(h):
        t = y / (h - 1)
        for i in range(len(stops) - 1):
            t0, c0 = stops[i]
            t1, c1 = stops[i + 1]
            if t0 <= t <= t1:
                col.append(lerp(c0, c1, (t - t0) / (t1 - t0)))
                break
    for y in range(h):
        for x in range(w):
            px[x, y] = col[y]
    return img.convert("RGBA")


# ---------------------------------------------------------------- board
def make_board():
    src = Image.open(f"{HANOI}/towerOfHanoi.png").convert("RGBA")
    a = np.array(src).astype(np.int32)
    h, w = a.shape[:2]
    sky = np.array([239, 239, 239])
    dist = np.abs(a[:, :, :3] - sky).sum(axis=2)

    # Flood fill the light grey sky from the top edge and the side edges
    # above the floor, so white walls inside the buildings stay opaque.
    mask = np.zeros((h, w), bool)
    q = deque()
    seeds = [(0, x) for x in range(w)] + [(y, 0) for y in range(FLOOR_Y)] + [(y, w - 1) for y in range(FLOOR_Y)]
    for y, x in seeds:
        if dist[y, x] < 24 and not mask[y, x]:
            mask[y, x] = True
            q.append((y, x))
    while q:
        y, x = q.popleft()
        for dy, dx in ((1, 0), (-1, 0), (0, 1), (0, -1)):
            ny, nx = y + dy, x + dx
            if 0 <= ny < FLOOR_Y and 0 <= nx < w and not mask[ny, nx] and dist[ny, nx] < 24:
                mask[ny, nx] = True
                q.append((ny, nx))

    # Drop small opaque specks left floating in the sky (noise in the source).
    seen = mask.copy()
    for sy in range(FLOOR_Y):
        for sx in range(w):
            if seen[sy, sx]:
                continue
            comp = [(sy, sx)]
            seen[sy, sx] = True
            i = 0
            while i < len(comp):
                y, x = comp[i]
                i += 1
                for dy, dx in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                    ny, nx = y + dy, x + dx
                    if 0 <= ny < FLOOR_Y and 0 <= nx < w and not seen[ny, nx]:
                        seen[ny, nx] = True
                        comp.append((ny, nx))
            if len(comp) < 300:
                for y, x in comp:
                    mask[y, x] = True

    alpha = np.where(mask, 0, 255).astype(np.float32)
    # Soften the 1px anti-aliased fringe next to the removed sky.
    m = Image.fromarray((mask * 255).astype(np.uint8)).filter(ImageFilter.MaxFilter(3))
    fringe = (np.array(m) > 0) & ~mask
    alpha[fringe] = np.clip((dist[fringe] - 20) * 8, 0, 255)

    alpha[:4, :] = 0  # stray light pixels on the top edge
    a[:, :, 3] = alpha.astype(np.int32)
    out = Image.fromarray(a.astype(np.uint8), "RGBA")
    out.save(f"{HANOI}/towerOfHanoi_board.png")
    return src, out


def make_floor_strip(src):
    """A 4096px wide bamboo floor tiled from a clean stretch of the board."""
    seg = src.crop((300, FLOOR_Y, 620, 600))
    strip = Image.new("RGBA", (4096, seg.height))
    x = 0
    while x < strip.width:
        strip.paste(seg, (x, 0))
        x += seg.width
    strip.save(f"{HANOI}/hanoi_floor.png")


# ---------------------------------------------------------------- sky
def mountain_layer(w, h, base_y, amp, color, seed, peaks):
    rnd = random.Random(seed)
    phases = [rnd.uniform(0, math.tau) for _ in range(4)]
    freqs = [peaks * f for f in (1, 2.3, 4.7, 9.1)]
    amps = [amp, amp * 0.45, amp * 0.18, amp * 0.07]
    pts = [(0, h)]
    for x in range(0, w + 1, 4):
        t = x / w
        y = base_y - sum(a * (0.5 + 0.5 * math.sin(f * math.tau * t + p)) for a, f, p in zip(amps, freqs, phases))
        pts.append((x, y))
    pts.append((w, h))
    layer = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    ImageDraw.Draw(layer).polygon(pts, fill=color)
    return layer


def cloud(draw, cx, cy, s, fill, shade):
    blobs = [(-1.0, 0.15, 0.55), (-0.45, -0.2, 0.7), (0.2, -0.3, 0.8), (0.85, 0.05, 0.55), (0.0, 0.2, 0.6)]
    for dx, dy, r in blobs:
        x, y, rr = cx + dx * s, cy + dy * s + s * 0.08, r * s
        draw.ellipse((x - rr, y - rr, x + rr, y + rr), fill=shade)
    for dx, dy, r in blobs:
        x, y, rr = cx + dx * s, cy + dy * s, r * s
        draw.ellipse((x - rr, y - rr, x + rr, y + rr), fill=fill)


SKY_THEMES = {
    "day": dict(
        gradient=[(0.0, (110, 182, 232)), (0.45, (166, 214, 240)), (0.72, (253, 226, 196)), (1.0, (250, 205, 170))],
        sun=(0.78, 0.30, 95), glow=(255, 236, 190), disc=((255, 214, 120), (255, 228, 150)),
        cloud=((255, 255, 255, 235), (220, 230, 242, 235)),
        mountains=[(150, 178, 196), (112, 152, 170), (86, 132, 128)], mist=((255, 240, 225), 70),
        ground=(96, 140, 70), birds=(70, 80, 100), stars=0),
    "sunset": dict(
        gradient=[(0.0, (84, 70, 142)), (0.38, (226, 120, 128)), (0.62, (255, 178, 112)), (1.0, (255, 160, 100))],
        sun=(0.74, 0.52, 120), glow=(255, 190, 120), disc=((255, 140, 70), (255, 176, 96)),
        cloud=((255, 196, 186, 230), (214, 132, 150, 230)),
        mountains=[(170, 120, 150), (128, 92, 136), (92, 72, 112)], mist=((255, 186, 160), 80),
        ground=(98, 92, 70), birds=(70, 50, 70), stars=0),
    "night": dict(
        gradient=[(0.0, (12, 18, 48)), (0.5, (30, 46, 96)), (1.0, (64, 76, 128))],
        sun=(0.78, 0.24, 70), glow=(180, 200, 255), disc=((226, 230, 214), (246, 246, 232)),
        cloud=((86, 98, 140, 200), (58, 68, 110, 200)),
        mountains=[(56, 70, 112), (42, 56, 92), (30, 46, 70)], mist=((100, 112, 160), 60),
        ground=(28, 48, 40), birds=None, stars=160),
}


def make_sky(theme, w=2560, h=1080):
    t = SKY_THEMES[theme]
    S = 2  # supersample for smooth edges
    W, H = w * S, h * S
    img = vertical_gradient(W // 8, H // 8, t["gradient"]).resize((W, H), Image.BICUBIC)

    if t["stars"]:
        rnd = random.Random(11)
        sd = ImageDraw.Draw(img)
        for _ in range(t["stars"]):
            x, y = rnd.uniform(0, W), rnd.uniform(0, H * 0.6) ** 1.0
            r = rnd.choice((1.5, 2, 2, 3, 4)) * S / 2
            sd.ellipse((x - r, y - r, x + r, y + r), fill=(255, 255, 230, rnd.randint(150, 255)))

    # Sun (or moon) with glow
    glow = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    sx, sy, sr = int(W * t["sun"][0]), int(H * t["sun"][1]), t["sun"][2]
    gd = ImageDraw.Draw(glow)
    for r, a in ((sr * 2.7, 40), (sr * 2.1, 60), (sr * 1.6, 90)):
        gd.ellipse((sx - r * S, sy - r * S, sx + r * S, sy + r * S), fill=t["glow"] + (a,))
    glow = glow.filter(ImageFilter.GaussianBlur(40 * S))
    img.alpha_composite(glow)
    outer, inner = t["disc"]
    ImageDraw.Draw(img).ellipse((sx - sr * S, sy - sr * S, sx + sr * S, sy + sr * S), fill=outer + (255,))
    ri = sr * 0.84
    ImageDraw.Draw(img).ellipse((sx - ri * S, sy - ri * S, sx + ri * S, sy + ri * S), fill=inner + (255,))
    if theme == "night":  # moon craters
        for cx, cy, cr in ((-0.3, -0.2, 0.18), (0.25, 0.15, 0.24), (-0.1, 0.4, 0.12)):
            x, y, r = sx + cx * sr * S, sy + cy * sr * S, cr * sr * S
            ImageDraw.Draw(img).ellipse((x - r, y - r, x + r, y + r), fill=(214, 216, 200, 255))

    # Clouds
    cl = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    cd = ImageDraw.Draw(cl)
    for cx, cy, s in ((0.12, 0.16, 70), (0.36, 0.09, 55), (0.55, 0.22, 80), (0.93, 0.12, 60), (0.70, 0.07, 45), (0.25, 0.33, 50)):
        cloud(cd, cx * W, cy * H, s * S, *t["cloud"])
    img.alpha_composite(cl)

    # Misty mountain layers, far to near
    layers = [(0.66, 230, 1, 1.6), (0.74, 190, 2, 2.2), (0.82, 140, 3, 3.0)]
    mist_color, mist_alpha = t["mist"]
    for i, (base, amp, seed, peaks) in enumerate(layers):
        layer = mountain_layer(W, H, base * H, amp * S, t["mountains"][i] + (255,), seed, peaks)
        img.alpha_composite(layer)
        # mist band above each nearer layer
        if i < len(layers) - 1:
            mist = Image.new("RGBA", (W, H), mist_color + (0,))
            m_alpha = Image.new("L", (W, H), 0)
            md = ImageDraw.Draw(m_alpha)
            top = int((layers[i + 1][0] - 0.09) * H)
            for y in range(top, int(layers[i + 1][0] * H)):
                f = (y - top) / max(1, int(layers[i + 1][0] * H) - top)
                md.line((0, y, W, y), fill=int(mist_alpha * f))
            mist.putalpha(m_alpha)
            img.alpha_composite(mist)

    # Ground: hills at the very bottom
    ground = mountain_layer(W, H, 0.92 * H, 40 * S, t["ground"] + (255,), 9, 2.0)
    img.alpha_composite(ground)

    # Flying birds
    if t["birds"]:
        bd = ImageDraw.Draw(img)
        for bx, by, bs in ((0.30, 0.25, 14), (0.33, 0.22, 10), (0.36, 0.26, 12), (0.62, 0.36, 9)):
            x, y, s = bx * W, by * H, bs * S
            bd.line((x - s, y - s * 0.4, x, y, x + s, y - s * 0.4), fill=t["birds"] + (255,), width=3 * S, joint="curve")

    img = img.resize((w, h), Image.LANCZOS)
    name = "hanoi_sky.png" if theme == "day" else f"hanoi_sky_{theme}.png"
    img.save(f"{HANOI}/{name}")
    return img


# ---------------------------------------------------------------- trees
# Cherry trees stand in front of the cut-off buildings at the board's edges.
# Coordinates are board pixels (towerOfHanoi.png, y down). The left tree's
# image covers x -480..180, y -80..560; the right one is drawn mirrored.
TREE_X0, TREE_Y0, TREE_W, TREE_H = -480, -80, 660, 640
TS = 2  # tree images are saved at 2x for sharpness


def blob_cluster(img, circles, base, shadow, light, outline):
    d = ImageDraw.Draw(img)

    def disc(x, y, r, c):
        d.ellipse((x - r, y - r, x + r, y + r), fill=c)

    for x, y, r in circles:
        disc(x, y, r + 5 * TS, outline)
    for x, y, r in circles:
        disc(x, y, r, shadow)
    for x, y, r in circles:
        disc(x - r * 0.1, y - r * 0.14, r * 0.86, base)
    for x, y, r in circles:
        disc(x - r * 0.3, y - r * 0.35, r * 0.34, light)


def lobe_circles(rnd, lobes):
    out = []
    for cx, cy, rx, ry, n in lobes:
        for _ in range(n):
            a = rnd.uniform(0, math.tau)
            d = math.sqrt(rnd.uniform(0, 1))
            out.append((cx + math.cos(a) * rx * d, cy + math.sin(a) * ry * d, rnd.uniform(34, 58)))
    # board px -> image px
    return [((x - TREE_X0) * TS, (y - TREE_Y0) * TS, r * TS) for x, y, r in out]


def make_tree(seed, name):
    rnd = random.Random(seed)
    img = Image.new("RGBA", (TREE_W * TS, TREE_H * TS), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)

    def P(x, y):
        return ((x - TREE_X0) * TS, (y - TREE_Y0) * TS)

    bark, bark_dark, bark_light = (122, 74, 54, 255), (66, 38, 28, 255), (156, 102, 74, 255)

    # Trunk over the cut line at x = 0, slightly curved.
    left = [P(-38, 562), P(-30, 470), P(-26, 380), P(-34, 290), P(-50, 200), P(-70, 130)]
    right = [P(-6, 130), P(4, 210), P(18, 300), P(30, 390), P(40, 480), P(52, 562)]
    trunk = left + right
    d.polygon(trunk, fill=bark_dark)
    inner = [(x + 5 * TS, y) for x, y in left] + [(x - 5 * TS, y) for x, y in right]
    d.polygon(inner, fill=bark)
    d.line([(x + 14 * TS, y) for x, y in left[:-1]], fill=bark_light, width=7 * TS, joint="curve")

    # Branches out to the blossom lobes
    branches = [((-40, 160), (-250, 60)), ((-40, 150), (60, 30)), ((-20, 260), (60, 210)),
                ((-50, 200), (-330, 230)), ((-60, 140), (-150, -10))]
    for (x0, y0), (x1, y1) in branches:
        d.line((P(x0, y0), P(x1, y1)), fill=bark_dark, width=26 * TS)
        d.line((P(x0, y0), P(x1, y1)), fill=bark, width=16 * TS)

    # Blossom canopy: the lobes at x ~ 0 hide the cut from the top down to the trunk.
    lobes = [(-220, 90, 200, 110, 26), (-60, 30, 140, 90, 16), (20, 70, 70, 70, 10),
             (20, 200, 60, 70, 7), (-330, 230, 110, 70, 9), (-120, 210, 110, 60, 8), (-10, 130, 50, 50, 5)]
    circles = lobe_circles(rnd, lobes)
    rnd.shuffle(circles)
    blob_cluster(img, circles, (248, 183, 205, 255), (226, 136, 172, 255), (255, 224, 234, 255), (160, 66, 104, 255))

    # Little flowers on the canopy for texture
    for x, y, r in circles:
        for _ in range(3):
            fx, fy = x + rnd.uniform(-0.7, 0.7) * r, y + rnd.uniform(-0.7, 0.5) * r
            fr = rnd.uniform(3, 5) * TS
            c = rnd.choice(((255, 240, 245, 255), (214, 96, 140, 255)))
            d.ellipse((fx - fr, fy - fr, fx + fr, fy + fr), fill=c)

    # Bushes at the foot of the tree
    bushes = [(-300, 520, 90, 30, 6), (-110, 530, 90, 26, 6), (60, 540, 50, 20, 3)]
    bush = lobe_circles(rnd, bushes)
    bush = [(x, min(y, (548 - TREE_Y0) * TS - r * 0.4), r * 0.85) for x, y, r in bush]
    blob_cluster(img, bush, (104, 166, 84, 255), (66, 124, 62, 255), (150, 204, 116, 255), (36, 78, 40, 255))
    for x, y, r in bush:
        for _ in range(2):
            fx, fy = x + rnd.uniform(-0.6, 0.6) * r, y + rnd.uniform(-0.6, 0.3) * r
            fr = 5 * TS
            d.ellipse((fx - fr, fy - fr, fx + fr, fy + fr), fill=(255, 196, 214, 255))

    # A few falling petals
    for _ in range(9):
        x, y = P(rnd.uniform(-420, 120), rnd.uniform(280, 470))
        petal = Image.new("RGBA", (24 * TS, 14 * TS), (0, 0, 0, 0))
        ImageDraw.Draw(petal).ellipse((0, 0, 24 * TS - 1, 14 * TS - 1), fill=(246, 170, 196, 255))
        petal = petal.rotate(rnd.uniform(0, 180), expand=True, resample=Image.BICUBIC)
        img.alpha_composite(petal, (int(x), int(y)))

    img.save(f"{HANOI}/{name}")
    return img


def board_with_trees(board, left, right):
    """The board with both trees, on a canvas 480px wider on each side (for previews)."""
    pad = -TREE_X0
    canvas = Image.new("RGBA", (1280 + 2 * pad, 600 - TREE_Y0), (0, 0, 0, 0))
    canvas.alpha_composite(board, (pad, -TREE_Y0))
    l = left.resize((TREE_W, TREE_H), Image.LANCZOS)
    r = right.resize((TREE_W, TREE_H), Image.LANCZOS)
    canvas.alpha_composite(l, (0, 0))
    canvas.alpha_composite(r, (canvas.width - TREE_W, 0))
    return canvas


def make_preview(sky, board, left, right):
    """Menu card image: sky + transparent board with its trees."""
    pw, ph = 1280, 720
    prev = sky.crop((0, 0, int(1080 * pw / ph), 1080)).resize((pw, ph), Image.LANCZOS)
    full = board_with_trees(board, left, right)
    prev.alpha_composite(full.crop((-TREE_X0, 0, -TREE_X0 + 1280, full.height)), (0, ph - full.height))
    prev.convert("RGB").save(f"{HANOI}/hanoi_preview.png")


# ---------------------------------------------------------------- menu bg
def make_menu_bg(w=2400, h=2400):
    """Neutral, square so it can cover both portrait and landscape."""
    img = vertical_gradient(w // 8, h // 8, [
        (0.0, (122, 196, 240)),
        (0.55, (186, 226, 236)),
        (1.0, (255, 236, 200)),
    ]).resize((w, h), Image.BICUBIC)
    rnd = random.Random(3)
    palette = [(255, 255, 255), (255, 214, 120), (255, 170, 150), (150, 220, 190), (190, 180, 255)]
    # Each circle is a blurred alpha mask filled with a solid colour, so the
    # blur never mixes in black from a transparent background.
    sw = w // 4
    for _ in range(34):
        r = rnd.randint(10, 48)
        x, y = rnd.randint(-25, sw + 25), rnd.randint(-25, sw + 25)
        c = rnd.choice(palette)
        mask = Image.new("L", (sw, sw), 0)
        ImageDraw.Draw(mask).ellipse((x - r, y - r, x + r, y + r), fill=rnd.randint(45, 100))
        mask = mask.filter(ImageFilter.GaussianBlur(2.5)).resize((w, h), Image.BICUBIC)
        layer = Image.new("RGBA", (w, h), c + (0,))
        layer.putalpha(mask)
        img.alpha_composite(layer)
    # soft diagonal light rays
    rays = Image.new("L", (w, h), 0)
    rd = ImageDraw.Draw(rays)
    for i in range(7):
        x0 = -w // 2 + i * w // 3
        rd.polygon([(x0, 0), (x0 + 160, 0), (x0 + 160 + h // 2, h), (x0 + h // 2, h)], fill=22)
    light = Image.new("RGBA", (w, h), (255, 255, 255, 0))
    light.putalpha(rays.filter(ImageFilter.GaussianBlur(30)))
    img.alpha_composite(light)
    img.convert("RGB").save(f"{ASSETS}/Sprites/menu_background.png")


src, board = make_board()
make_floor_strip(src)
left_tree = make_tree(21, "hanoi_tree_left.png")
right_tree = make_tree(34, "hanoi_tree_right.png").transpose(Image.FLIP_LEFT_RIGHT)
right_tree.save(f"{HANOI}/hanoi_tree_right.png")
sky = make_sky("day")
make_sky("sunset")
make_sky("night")
make_preview(sky, board, left_tree, right_tree)
make_menu_bg()
print("ok")


def make_star(size=128):
    """Small gold star for the star criteria panel."""
    S = 4
    W = size * S
    img = Image.new("RGBA", (W, W), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)

    def star(r_out, r_in, cx, cy):
        pts = []
        for i in range(10):
            r = r_out if i % 2 == 0 else r_in
            a = -math.pi / 2 + i * math.pi / 5
            pts.append((cx + r * math.cos(a), cy + r * math.sin(a)))
        return pts

    c = W / 2
    d.polygon(star(W * 0.49, W * 0.22, c, c + W * 0.02), fill=(196, 120, 20, 255))   # outline
    d.polygon(star(W * 0.43, W * 0.19, c, c + W * 0.02), fill=(255, 205, 40, 255))   # body
    d.polygon(star(W * 0.25, W * 0.11, c - W * 0.03, c - W * 0.03), fill=(255, 236, 130, 255))  # shine
    img.resize((size, size), Image.LANCZOS).save(f"{HANOI}/star.png")


make_star()
