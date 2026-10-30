import math
from PIL import Image, ImageDraw, ImageFont, ImageFilter

SPRITES_DIR = r"c:\Documents\UNITY 2026\Star Vanguard\Assets\Sprites"

def create_star_vanguard_logo(path):
    W, H = 512, 240
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Background subtle hexagonal glow
    center_x, center_y = W // 2, H // 2
    for r in range(110, 0, -5):
        alpha = int(45 * (1.0 - (r / 110.0)))
        draw.ellipse([center_x - r * 2, center_y - r - 10, center_x + r * 2, center_y + r - 10], 
                     fill=(0, 229, 255, alpha))

    # Decorative tech lines & wings
    wing_color = (0, 229, 255, 200)
    # Left wing chevron
    draw.polygon([(40, center_y - 20), (100, center_y - 50), (140, center_y - 50), (60, center_y - 20)], fill=wing_color)
    draw.polygon([(50, center_y - 10), (90, center_y - 30), (120, center_y - 30), (65, center_y - 10)], fill=(255, 0, 128, 180))
    # Right wing chevron
    draw.polygon([(W - 40, center_y - 20), (W - 100, center_y - 50), (W - 140, center_y - 50), (W - 60, center_y - 20)], fill=wing_color)
    draw.polygon([(W - 50, center_y - 10), (W - 90, center_y - 30), (W - 120, center_y - 30), (W - 65, center_y - 10)], fill=(255, 0, 128, 180))

    # Outer decorative frame
    draw.line([(80, 30), (W - 80, 30)], fill=(0, 229, 255, 120), width=2)
    draw.line([(60, H - 40), (W - 60, H - 40)], fill=(0, 229, 255, 120), width=2)
    draw.line([(80, 30), (50, 60)], fill=(0, 229, 255, 120), width=2)
    draw.line([(W - 80, 30), (W - 50, 60)], fill=(0, 229, 255, 120), width=2)

    # Text rendering using fallback default or clean vector polygons
    # Main Title: STAR VANGUARD
    # Drop shadow
    draw.text((center_x - 180, center_y - 58), "STAR VANGUARD", fill=(0, 20, 40, 255), font=None)
    # We will use large stylized text
    try:
        font_large = ImageFont.truetype("arialbd.ttf", 46)
        font_sub = ImageFont.truetype("arialbd.ttf", 18)
    except:
        font_large = ImageFont.load_default()
        font_sub = ImageFont.load_default()

    # Draw glow layers for "STAR VANGUARD"
    title = "STAR VANGUARD"
    bbox = draw.textbbox((0, 0), title, font=font_large)
    tw, th = bbox[2] - bbox[0], bbox[3] - bbox[1]
    tx, ty = center_x - tw // 2, center_y - 45

    for offset in [(0, 3), (0, -2), (2, 0), (-2, 0), (2, 2), (-2, -2)]:
        draw.text((tx + offset[0], ty + offset[1]), title, fill=(0, 150, 255, 160), font=font_large)

    draw.text((tx, ty), title, fill=(240, 255, 255, 255), font=font_large)

    # Subtitle: ALIEN ONSLAUGHT
    sub_title = "— ALIEN ONSLAUGHT —"
    bbox_sub = draw.textbbox((0, 0), sub_title, font=font_sub)
    sw, sh = bbox_sub[2] - bbox_sub[0], bbox_sub[3] - bbox_sub[1]
    sx, sy = center_x - sw // 2, center_y + 15
    draw.text((sx + 1, sy + 1), sub_title, fill=(255, 0, 90, 220), font=font_sub)
    draw.text((sx, sy), sub_title, fill=(255, 80, 160, 255), font=font_sub)

    # Tech dots
    for i in range(-5, 6):
        dot_x = center_x + i * 28
        draw.ellipse([dot_x - 2, H - 42, dot_x + 2, H - 38], fill=(0, 229, 255, 200))

    img.save(path, "PNG")
    print(f"Created Logo: {path}")

def create_alien_scout_drone(path):
    # Dimensions matching BlueChicken (545, 454)
    W, H = 545, 454
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    cx, cy = W // 2, H // 2

    # Draw glowing aura
    for r in range(180, 40, -15):
        alpha = int(35 * (1.0 - (r / 180.0)))
        draw.ellipse([cx - r * 1.2, cy - r, cx + r * 1.2, cy + r], fill=(160, 32, 240, alpha))

    # Alien Drone Chassis (Facing Downward!)
    # Top is rear, Bottom is cockpit / nose
    # Main outer wings (Swept forward bio-mechanical wings)
    outer_wings = [
        (cx, cy - 140),          # Rear tail apex
        (cx - 90, cy - 100),     # Upper left hull
        (cx - 230, cy - 20),     # Left wingtip outer
        (cx - 210, cy + 80),     # Left wing weapon pod
        (cx - 130, cy + 60),     # Left inner intake
        (cx - 60, cy + 150),     # Left nose flank
        (cx, cy + 190),          # Menacing front nose point (downward)
        (cx + 60, cy + 150),     # Right nose flank
        (cx + 130, cy + 60),     # Right inner intake
        (cx + 210, cy + 80),     # Right wing weapon pod
        (cx + 230, cy - 20),     # Right wingtip outer
        (cx + 90, cy - 100),     # Upper right hull
    ]
    # Dark alien armor plate
    draw.polygon(outer_wings, fill=(25, 20, 38, 255), outline=(0, 229, 255, 220))

    # Inner armor layering
    inner_wings = [
        (cx, cy - 110),
        (cx - 70, cy - 80),
        (cx - 180, cy - 10),
        (cx - 160, cy + 60),
        (cx - 50, cy + 130),
        (cx, cy + 160),
        (cx + 50, cy + 130),
        (cx + 160, cy + 60),
        (cx + 180, cy - 10),
        (cx + 70, cy - 80),
    ]
    draw.polygon(inner_wings, fill=(45, 35, 65, 255), outline=(180, 0, 255, 200))

    # Glowing plasma vents & conduits
    draw.line([(cx - 140, cy), (cx - 40, cy + 90)], fill=(0, 255, 230, 220), width=4)
    draw.line([(cx + 140, cy), (cx + 40, cy + 90)], fill=(0, 255, 230, 220), width=4)
    draw.line([(cx - 170, cy + 40), (cx - 70, cy + 100)], fill=(255, 0, 128, 200), width=3)
    draw.line([(cx + 170, cy + 40), (cx + 70, cy + 100)], fill=(255, 0, 128, 200), width=3)

    # Dual Wing Energy Cannons
    draw.rectangle([cx - 215, cy + 20, cx - 200, cy + 110], fill=(15, 10, 25, 255), outline=(0, 229, 255, 255))
    draw.rectangle([cx + 200, cy + 20, cx + 215, cy + 110], fill=(15, 10, 25, 255), outline=(0, 229, 255, 255))
    draw.ellipse([cx - 212, cy + 105, cx - 203, cy + 115], fill=(0, 255, 255, 255))
    draw.ellipse([cx + 203, cy + 105, cx + 212, cy + 115], fill=(0, 255, 255, 255))

    # Central Menacing Red Optical Core / Alien Eye
    draw.ellipse([cx - 36, cy - 20, cx + 36, cy + 50], fill=(15, 5, 20, 255), outline=(255, 0, 80, 255))
    for r in range(25, 0, -4):
        col = (255, 20, 70, int(255 * (r / 25.0)))
        draw.ellipse([cx - r, cy + 15 - r, cx + r, cy + 15 + r], fill=col)
    draw.ellipse([cx - 6, cy + 10, cx + 6, cy + 20], fill=(255, 240, 240, 255)) # highlight

    # Dual Rear Engine Thrusters (Top)
    draw.rectangle([cx - 50, cy - 148, cx - 25, cy - 138], fill=(0, 180, 255, 255))
    draw.rectangle([cx + 25, cy - 148, cx + 50, cy - 138], fill=(0, 180, 255, 255))
    draw.polygon([(cx - 45, cy - 148), (cx - 30, cy - 148), (cx - 37, cy - 170)], fill=(0, 220, 255, 200))
    draw.polygon([(cx + 30, cy - 148), (cx + 45, cy - 148), (cx + 37, cy - 170)], fill=(0, 220, 255, 200))

    img.save(path, "PNG")
    print(f"Created Alien Scout Drone: {path}")

def create_alien_dreadnought_boss(path):
    # Dimensions matching ChickenBoss (274, 250) or higher quality (512, 480) downscaled to (274, 250)
    W, H = 274, 250
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    cx, cy = W // 2, H // 2

    # Menacing Crimson Aura
    for r in range(120, 30, -10):
        alpha = int(40 * (1.0 - (r / 120.0)))
        draw.ellipse([cx - r * 1.1, cy - r, cx + r * 1.1, cy + r], fill=(255, 0, 60, alpha))

    # Heavy Dreadnought Hull (Facing Downwards)
    boss_chassis = [
        (cx, cy - 100),          # Rear heavy armor
        (cx - 60, cy - 90),
        (cx - 125, cy - 40),     # Left broadside horn
        (cx - 110, cy + 20),     # Left weapon array
        (cx - 75, cy + 80),      # Left frontal ram
        (cx - 30, cy + 105),     # Nose port
        (cx, cy + 118),          # Frontal heavy prow (Downward)
        (cx + 30, cy + 105),     # Nose starboard
        (cx + 75, cy + 80),      # Right frontal ram
        (cx + 110, cy + 20),     # Right weapon array
        (cx + 125, cy - 40),     # Right broadside horn
        (cx + 60, cy - 90),
    ]
    draw.polygon(boss_chassis, fill=(28, 18, 30, 255), outline=(255, 40, 40, 240))

    # Inner Armor Plating
    inner_plates = [
        (cx, cy - 75),
        (cx - 45, cy - 65),
        (cx - 90, cy - 25),
        (cx - 80, cy + 25),
        (cx - 50, cy + 65),
        (cx, cy + 90),
        (cx + 50, cy + 65),
        (cx + 80, cy + 25),
        (cx + 90, cy - 25),
        (cx + 45, cy - 65),
    ]
    draw.polygon(inner_plates, fill=(48, 28, 50, 255), outline=(255, 120, 0, 220))

    # Heavy Weapon Battery Pods (Left & Right)
    draw.rectangle([cx - 115, cy - 10, cx - 100, cy + 50], fill=(15, 10, 20, 255), outline=(255, 60, 0, 255))
    draw.rectangle([cx + 100, cy - 10, cx + 115, cy + 50], fill=(15, 10, 20, 255), outline=(255, 60, 0, 255))
    draw.ellipse([cx - 112, cy + 45, cx - 103, cy + 56], fill=(255, 80, 0, 255))
    draw.ellipse([cx + 103, cy + 45, cx + 112, cy + 56], fill=(255, 80, 0, 255))

    # Plasma conduits
    draw.line([(cx - 75, cy - 40), (cx - 20, cy + 40)], fill=(0, 240, 255, 200), width=3)
    draw.line([(cx + 75, cy - 40), (cx + 20, cy + 40)], fill=(0, 240, 255, 200), width=3)

    # Central Core Reactor (Pulsing Anti-matter eye)
    draw.ellipse([cx - 32, cy - 20, cx + 32, cy + 44], fill=(10, 5, 15, 255), outline=(255, 0, 80, 255))
    for r in range(22, 0, -3):
        col = (255, int(180 * (1 - r / 22)), 30, int(255 * (r / 22.0)))
        draw.ellipse([cx - r, cy + 12 - r, cx + r, cy + 12 + r], fill=col)
    draw.ellipse([cx - 5, cy + 7, cx + 5, cy + 17], fill=(255, 255, 255, 255))

    # Triple Rear Engine Plasma Exhausts (Top)
    for off_x in [-40, 0, 40]:
        draw.rectangle([cx + off_x - 8, cy - 108, cx + off_x + 8, cy - 98], fill=(255, 60, 0, 255))
        draw.polygon([(cx + off_x - 6, cy - 108), (cx + off_x + 6, cy - 108), (cx + off_x, cy - 124)], 
                     fill=(255, 150, 0, 200))

    img.save(path, "PNG")
    print(f"Created Alien Dreadnought Boss: {path}")

def create_plasma_orb(path):
    # Dimensions matching Egg (152, 196)
    W, H = 152, 196
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    cx, cy = W // 2, H // 2

    # Energy Corona Falloff
    for r in range(65, 15, -4):
        alpha = int(75 * (1.0 - (r / 65.0)))
        draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=(255, 0, 128, alpha))
        draw.ellipse([cx - r * 0.85, cy - r * 0.85, cx + r * 0.85, cy + r * 0.85], fill=(160, 0, 255, alpha))

    # Dense Plasma Core
    for r in range(28, 0, -3):
        col = (255, 80 + int(175 * (1 - r / 28)), 220, 255)
        draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=col)

    # Core white hot center
    draw.ellipse([cx - 8, cy - 8, cx + 8, cy + 8], fill=(255, 255, 255, 255))

    # Orbital electric sparks
    draw.arc([cx - 42, cy - 42, cx + 42, cy + 42], start=30, end=140, fill=(0, 229, 255, 240), width=2)
    draw.arc([cx - 42, cy - 42, cx + 42, cy + 42], start=210, end=320, fill=(0, 229, 255, 240), width=2)

    img.save(path, "PNG")
    print(f"Created Plasma Orb: {path}")

def create_broken_plasma_residue(path):
    # Dimensions matching BrokenEgg (107, 70)
    W, H = 107, 70
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    cx, cy = W // 2, H // 2
    # Expanding ring fragment
    for r in range(30, 5, -5):
        alpha = int(60 * (1.0 - r / 30.0))
        draw.ellipse([cx - r * 1.5, cy - r, cx + r * 1.5, cy + r], fill=(255, 0, 150, alpha))

    # Scatter particles
    import random
    random.seed(42)
    for _ in range(16):
        px = cx + random.randint(-40, 40)
        py = cy + random.randint(-20, 20)
        pr = random.randint(2, 5)
        draw.ellipse([px - pr, py - pr, px + pr, py + pr], fill=(0, 240, 255, 220))

    img.save(path, "PNG")
    print(f"Created Plasma Impact: {path}")

def create_energy_crystal_sprite(path):
    # Dimensions matching Food (42, 70)
    W, H = 42, 70
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    cx, cy = W // 2, H // 2

    # Glow
    for r in range(20, 5, -3):
        alpha = int(65 * (1.0 - r / 20.0))
        draw.ellipse([cx - r, cy - r * 1.5, cx + r, cy + r * 1.5], fill=(0, 229, 255, alpha))

    # Octagonal Crystal Gem Facets
    top_y = cy - 28
    bottom_y = cy + 28
    mid_upper_y = cy - 10
    mid_lower_y = cy + 10
    left_x = cx - 16
    right_x = cx + 16

    # Front Main Facet
    main_facet = [(cx, top_y), (right_x, mid_upper_y), (cx, bottom_y), (left_x, mid_upper_y)]
    draw.polygon(main_facet, fill=(0, 200, 255, 255), outline=(220, 255, 255, 255))

    # Side Shading
    left_facet = [(cx, top_y), (left_x, mid_upper_y), (cx - 8, mid_lower_y), (cx, bottom_y)]
    draw.polygon(left_facet, fill=(0, 140, 210, 255))

    right_facet = [(cx, top_y), (right_x, mid_upper_y), (cx + 8, mid_lower_y), (cx, bottom_y)]
    draw.polygon(right_facet, fill=(0, 240, 255, 255))

    # Specular white reflection line
    draw.line([(cx - 2, top_y + 4), (left_x + 4, mid_upper_y)], fill=(255, 255, 255, 255), width=2)

    img.save(path, "PNG")
    print(f"Created Energy Crystal: {path}")

if __name__ == "__main__":
    create_star_vanguard_logo(rf"{SPRITES_DIR}\ChickenInvadersLogo.png")
    create_alien_scout_drone(rf"{SPRITES_DIR}\BlueChicken_Sprite.png")
    create_alien_dreadnought_boss(rf"{SPRITES_DIR}\ChickenBoss.png")
    create_plasma_orb(rf"{SPRITES_DIR}\Egg.png")
    create_broken_plasma_residue(rf"{SPRITES_DIR}\BrokenEgg.png")
    create_energy_crystal_sprite(rf"{SPRITES_DIR}\Food.png")
    print("All sci-fi sprites successfully generated and updated!")
