# Dang nhap ung dung quan tri va chup lan luot cac man hinh de dua vao bao cao.
#
# Yeu cau: BookStore.Api dang chay va BookStore.Admin.exe dang o man hinh dang nhap.
# Cach dung:
#   powershell -ExecutionPolicy Bypass -File chup-quantri.ps1 -ThuMucAnh "<duong dan img>"
#
# Script dung UI Automation de tim dung o nhap lieu va muc menu thay vi doan toa do,
# nen khong phu thuoc vao do phan giai hay ty le hien thi cua man hinh.
# Trong luc script chay, khong thao tac chuot va ban phim tren may.
param(
    [Parameter(Mandatory = $true)][string]$ThuMucAnh,
    [string]$TenDangNhap = "quanly",
    [string]$MatKhau = "123456"
)

Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type @"
using System;
using System.Runtime.InteropServices;
public class W {
    [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
    [DllImport("user32.dll")] public static extern void SwitchToThisWindow(IntPtr h, bool alt);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int n);
    [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr h, IntPtr a, int x, int y, int cx, int cy, uint f);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out RECT r);
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left, Top, Right, Bottom; }
}
"@

[void][W]::SetProcessDPIAware()

function Lay-CuaSo {
    Get-Process -Name BookStore.Admin -ErrorAction SilentlyContinue |
        Where-Object { $_.MainWindowHandle -ne 0 } | Select-Object -First 1
}

function Dua-LenTruoc($h) {
    [void][W]::ShowWindow($h, 9)
    [W]::SwitchToThisWindow($h, $true)
    [void][W]::SetForegroundWindow($h)
    Start-Sleep -Milliseconds 900
}

function Goc($h) { [Windows.Automation.AutomationElement]::FromHandle($h) }

function Tim-TatCa($goc, $loai) {
    $dk = New-Object Windows.Automation.PropertyCondition(
        [Windows.Automation.AutomationElement]::ControlTypeProperty, $loai)
    $goc.FindAll([Windows.Automation.TreeScope]::Descendants, $dk)
}

function Go-VaoO($o, $chu) {
    $o.SetFocus()
    Start-Sleep -Milliseconds 350
    [System.Windows.Forms.SendKeys]::SendWait("^a")
    [System.Windows.Forms.SendKeys]::SendWait($chu)
    Start-Sleep -Milliseconds 350
}

function Kich-Hoat($e) {
    try {
        $p = $e.GetCurrentPattern([Windows.Automation.InvokePattern]::Pattern)
        $p.Invoke()
    } catch {
        $p = $e.GetCurrentPattern([Windows.Automation.SelectionItemPattern]::Pattern)
        $p.Select()
    }
    Start-Sleep -Milliseconds 600
}

function Chup($ten) {
    $p = Lay-CuaSo
    if (-not $p) { Write-Output "  KHONG TIM THAY CUA SO"; return }
    Dua-LenTruoc $p.MainWindowHandle
    $r = New-Object W+RECT
    [void][W]::GetWindowRect($p.MainWindowHandle, [ref]$r)
    $w = $r.Right - $r.Left; $ht = $r.Bottom - $r.Top
    $bmp = New-Object System.Drawing.Bitmap $w, $ht
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.CopyFromScreen($r.Left, $r.Top, 0, 0, $bmp.Size)
    $bmp.Save((Join-Path $ThuMucAnh ($ten + ".png")), [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $bmp.Dispose()
    Write-Output "  $ten.png ($w x $ht)"
}

$p = Lay-CuaSo
if (-not $p) { Write-Output "Chua mo BookStore.Admin.exe"; exit 1 }
$h = $p.MainWindowHandle
$theCu = $h
Dua-LenTruoc $h

# ----- Dang nhap -----
$o = Tim-TatCa (Goc $h) ([Windows.Automation.ControlType]::Edit)
Write-Output "So o nhap lieu tim thay: $($o.Count)"
if ($o.Count -lt 2) { Write-Output "Khong tim du o nhap lieu"; exit 1 }

Go-VaoO $o[0] $TenDangNhap
Go-VaoO $o[1] $MatKhau
Chup "kq_d01_dangnhap"

[System.Windows.Forms.SendKeys]::SendWait("{ENTER}")
Write-Output "Da gui thong tin dang nhap, cho cua so chinh..."

$doi = $false
for ($i = 0; $i -lt 25; $i++) {
    Start-Sleep -Milliseconds 700
    $p = Lay-CuaSo
    if ($p -and $p.MainWindowHandle -ne $theCu) { $doi = $true; break }
}
if (-not $doi) {
    Write-Output "KHONG MO DUOC CUA SO CHINH - kiem tra Web API da chay chua"
    Chup "loi_dangnhap"
    exit 1
}

$h = (Lay-CuaSo).MainWindowHandle
# Kich thuoc cua so phai nam gon trong man hinh, neu khong phan nam ngoai
# man hinh se khong duoc ve va anh chup bi cat.
$mh = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
$rong = [Math]::Min(1480, $mh.Width - 40)
$cao = [Math]::Min(900, $mh.Height - 40)
[void][W]::SetWindowPos($h, [IntPtr](-1), 10, 10, $rong, $cao, 0x40)
Start-Sleep -Seconds 3
Chup "kq_d02_tongquan"

# ----- Chuyen qua cac muc menu ben trai -----
$muc = Tim-TatCa (Goc $h) ([Windows.Automation.ControlType]::ListItem)
Write-Output "So muc menu tim thay: $($muc.Count)"

if ($muc.Count -ge 2) {
    Kich-Hoat $muc[1]
    Start-Sleep -Seconds 3
    # Chon don hang dau tien de panel chi tiet co noi dung.
    # Cac muc dau trong danh sach la muc menu dieu huong ($muc.Count muc), sau do moi den dong don hang.
    $dong = Tim-TatCa (Goc $h) ([Windows.Automation.ControlType]::ListItem)
    if ($dong.Count -gt $muc.Count) { Kich-Hoat $dong[$muc.Count]; Start-Sleep -Seconds 2 }
    Chup "kq_d03_donhang"
}
if ($muc.Count -ge 3) { Kich-Hoat $muc[2]; Start-Sleep -Seconds 3; Chup "kq_d04_sach" }

# Cac man hinh bo sung: danh muc, kho va nhap hang, khach hang, bao cao
$tenAnh = @("kq_d05_danhmuc", "kq_d06_kho", "kq_d07_khachhang", "kq_d08_baocao")
for ($k = 0; $k -lt $tenAnh.Count; $k++) {
    $chiSo = 3 + $k
    if ($muc.Count -gt $chiSo) {
        Kich-Hoat $muc[$chiSo]
        Start-Sleep -Seconds 3
        # Chon dong dau tien cua danh sach (neu co) de khung chi tiet ben phai co noi dung
        if ($k -lt 3) {
            $dong = Tim-TatCa (Goc $h) ([Windows.Automation.ControlType]::ListItem)
            if ($dong.Count -gt $muc.Count) { Kich-Hoat $dong[$muc.Count]; Start-Sleep -Seconds 2 }
        }
        Chup $tenAnh[$k]
    }
}

[void][W]::SetWindowPos($h, [IntPtr](-2), 0, 0, 0, 0, 0x3)
Write-Output "Xong."
