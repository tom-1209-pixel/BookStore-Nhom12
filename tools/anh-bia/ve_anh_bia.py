"""Ve anh bia minh hoa cho 12 sach mau (khong dung anh bia that de tranh van de ban quyen).

Chay:  python ve_anh_bia.py <thu_muc_xuat>
Mac dinh xuat vao src/AnhBia, ten tep la ISBN cua sach (khop voi SeedData.cs).
"""
import math
import random
import sys
from pathlib import Path

from PIL import Image, ImageDraw, ImageFilter, ImageFont

RONG, CAO = 600, 900
FONT_DAM = "C:/Windows/Fonts/segoeuib.ttf"
FONT_THUONG = "C:/Windows/Fonts/segoeui.ttf"
FONT_SERIF = "C:/Windows/Fonts/timesbd.ttf"

# ISBN, ten hien thi, tac gia, nha xuat ban, mau nen 1, mau nen 2, kieu trang tri
SACH = [
    ("9786046742011", "Lập trình C#\ntừ cơ bản\nđến nâng cao", "Phạm Hữu Khang", "NXB Thông tin và Truyền thông", "#1d3557", "#457b9d", "ma"),
    ("9780132350884", "Clean Code\nMã nguồn sạch", "Robert C. Martin", "NXB Thông tin và Truyền thông", "#264653", "#2a9d8f", "luoi"),
    ("9786046743012", "Cơ sở dữ liệu\nSQL Server\ntừ A đến Z", "Phạm Hữu Khang", "NXB Thông tin và Truyền thông", "#3a0ca3", "#4361ee", "bang"),
    ("9786041081338", "Mắt biếc", "Nguyễn Nhật Ánh", "NXB Trẻ", "#2d6a4f", "#95d5b2", "la"),
    ("9786041093256", "Cho tôi xin\nmột vé đi\ntuổi thơ", "Nguyễn Nhật Ánh", "NXB Trẻ", "#f4a261", "#e9c46a", "tron"),
    ("9786041125478", "Đất rừng\nphương Nam", "Tô Hoài", "NXB Trẻ", "#386641", "#a7c957", "la"),
    ("9786045868751", "Nhà giả kim", "Paulo Coelho", "NXB Trẻ", "#7f5539", "#ddb892", "sao"),
    ("9786047736333", "Đắc nhân tâm", "Dale Carnegie", "NXB Lao động", "#9d0208", "#dc2f02", "song"),
    ("9786049558412", "Quản trị học\ncăn bản", "", "NXB Lao động", "#14213d", "#fca311", "cot"),
    ("9786049558419", "Marketing\ncăn bản", "", "NXB Lao động", "#5f0f40", "#e36414", "tron"),
    ("9786047742215", "Tôi tài giỏi,\nbạn cũng thế!", "Adam Khoo", "NXB Lao động", "#0077b6", "#90e0ef", "song"),
    ("9786042108881", "Doraemon\ntập 1", "Fujiko F. Fujio", "NXB Kim Đồng", "#0096c7", "#ffd60a", "tron"),
]


def mau(hex_):
    hex_ = hex_.lstrip("#")
    return tuple(int(hex_[i:i + 2], 16) for i in (0, 2, 4))


def nen_chuyen_mau(c1, c2):
    anh = Image.new("RGB", (RONG, CAO))
    ve = ImageDraw.Draw(anh)
    for y in range(CAO):
        t = y / CAO
        ve.line([(0, y), (RONG, y)], fill=tuple(int(c1[i] + (c2[i] - c1[i]) * t) for i in range(3)))
    return anh


def trang_tri(anh, kieu, rnd):
    lop = Image.new("RGBA", (RONG, CAO), (0, 0, 0, 0))
    ve = ImageDraw.Draw(lop)
    trang = (255, 255, 255)
    if kieu == "ma":
        f = ImageFont.truetype("C:/Windows/Fonts/consola.ttf", 26)
        dong = ["class Sach {", "  string Ten;", "  decimal Gia;", "}", "var ds = db.Sach", "  .Where(s => s.Ton > 0)", "  .ToList();", "async Task Main()", "{ await Run(); }"]
        for i in range(30):
            ve.text((rnd.randint(-40, RONG - 120), rnd.randint(0, CAO)), rnd.choice(dong), font=f, fill=trang + (28,))
    elif kieu == "luoi":
        for x in range(0, RONG, 40):
            ve.line([(x, 0), (x, CAO)], fill=trang + (22,), width=1)
        for y in range(0, CAO, 40):
            ve.line([(0, y), (RONG, y)], fill=trang + (22,), width=1)
        ve.rectangle([360, 520, 560, 720], outline=trang + (90,), width=6)
    elif kieu == "bang":
        for i in range(5):
            x, y = 70 + i * 95, 560 + (i % 2) * 60
            ve.rounded_rectangle([x, y, x + 80, y + 150], 10, outline=trang + (80,), width=4)
            ve.line([(x, y + 35), (x + 80, y + 35)], fill=trang + (80,), width=4)
    elif kieu == "la":
        for _ in range(16):
            cx, cy, r = rnd.randint(0, RONG), rnd.randint(450, CAO), rnd.randint(40, 120)
            ve.ellipse([cx - r, cy - r // 2, cx + r, cy + r // 2], fill=trang + (rnd.randint(20, 50),))
    elif kieu == "tron":
        for _ in range(9):
            cx, cy, r = rnd.randint(0, RONG), rnd.randint(0, CAO), rnd.randint(30, 160)
            ve.ellipse([cx - r, cy - r, cx + r, cy + r], fill=trang + (rnd.randint(18, 45),))
    elif kieu == "sao":
        for _ in range(90):
            x, y, r = rnd.randint(0, RONG), rnd.randint(0, CAO // 2), rnd.choice([1, 2, 2, 3])
            ve.ellipse([x - r, y - r, x + r, y + r], fill=trang + (rnd.randint(90, 200),))
        ve.polygon([(0, CAO), (180, 600), (330, 740), (460, 560), (RONG, 780), (RONG, CAO)], fill=(0, 0, 0, 60))
    elif kieu == "song":
        for k in range(7):
            diem = [(x, 620 + k * 40 + 26 * math.sin(x / 70 + k)) for x in range(0, RONG + 10, 10)]
            ve.line(diem, fill=trang + (45,), width=5)
    elif kieu == "cot":
        for i, h in enumerate([120, 200, 160, 260, 320]):
            x = 90 + i * 90
            ve.rectangle([x, 760 - h, x + 55, 760], fill=trang + (70,))
    anh.paste(lop, (0, 0), lop)


def can_giua(ve, y, chu, font, mau_chu, gian=8):
    for dong in chu.split("\n"):
        hop = ve.textbbox((0, 0), dong, font=font)
        ve.text(((RONG - (hop[2] - hop[0])) / 2, y), dong, font=font, fill=mau_chu)
        y += (hop[3] - hop[1]) + gian
    return y


def ve_bia(isbn, ten, tac_gia, nxb, c1, c2, kieu, thu_muc):
    rnd = random.Random(isbn)
    anh = nen_chuyen_mau(mau(c1), mau(c2))
    trang_tri(anh, kieu, rnd)
    ve = ImageDraw.Draw(anh)

    # Khung va gay sach
    ve.rectangle([0, 0, 22, CAO], fill=(0, 0, 0))
    anh.paste(Image.new("RGB", (22, CAO), (0, 0, 0)).filter(ImageFilter.GaussianBlur(4)), (0, 0))
    ve.rectangle([40, 40, RONG - 40, CAO - 40], outline=(255, 255, 255), width=2)

    so_dong = ten.count("\n") + 1
    co_chu = 74 if so_dong <= 2 else 62
    font_ten = ImageFont.truetype(FONT_SERIF, co_chu)
    y = can_giua(ve, 150, ten, font_ten, (255, 255, 255), gian=26)
    ve.line([(RONG / 2 - 60, y + 24), (RONG / 2 + 60, y + 24)], fill=(255, 255, 255), width=3)
    if tac_gia:
        can_giua(ve, y + 50, tac_gia.upper(), ImageFont.truetype(FONT_DAM, 30), (255, 255, 255))

    # Nha xuat ban o chan bia
    dai = ImageFont.truetype(FONT_THUONG, 22)
    hop = ve.textbbox((0, 0), nxb, font=dai)
    ve.text(((RONG - (hop[2] - hop[0])) / 2, CAO - 95), nxb, font=dai, fill=(255, 255, 255))

    anh = anh.resize((400, 600), Image.LANCZOS)
    anh.save(thu_muc / f"{isbn}.png", optimize=True)


def main():
    thu_muc = Path(sys.argv[1]) if len(sys.argv) > 1 else Path(__file__).resolve().parents[2] / "src" / "AnhBia"
    thu_muc.mkdir(parents=True, exist_ok=True)
    for s in SACH:
        ve_bia(*s, thu_muc)
    print(f"Da ve {len(SACH)} anh bia vao {thu_muc}")


if __name__ == "__main__":
    main()
