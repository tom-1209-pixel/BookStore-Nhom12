# Chup anh cua so ung dung Desktop (WinUI 3) de dua vao bao cao.
#
# WinUI 3 ve giao dien bang swap chain cua he thong hop thanh nen PrintWindow
# chi lay duoc khung cua so, phan noi dung bi trang. Vi vay script dua cua so
# len truoc, dat ve goc man hinh roi chup lai vung man hinh do.
#
# Cach dung: powershell -File chup-man-hinh.ps1 -DuongDanLuu anh.png
param(
    [string]$TenTienTrinh = "BookStore.Admin",
    [Parameter(Mandatory = $true)][string]$DuongDanLuu,
    [int]$ChoGiay = 2,
    [int]$X = 40,
    [int]$Y = 40,
    [int]$Rong = 0,
    [int]$Cao = 0
)

Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms
Add-Type @"
using System;
using System.Runtime.InteropServices;
public class Win32 {
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
    [DllImport("user32.dll")] public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr hWnd, IntPtr after, int x, int y, int cx, int cy, uint flags);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left, Top, Right, Bottom; }
}
"@

# Bat che do nhan biet DPI, neu khong GetWindowRect tra ve toa do da thu nho
# theo ty le man hinh va anh chup se bi cat mat mot phan cua so.
[void][Win32]::SetProcessDPIAware()

$p = Get-Process -Name $TenTienTrinh -ErrorAction SilentlyContinue |
     Where-Object { $_.MainWindowHandle -ne 0 } | Select-Object -First 1

if (-not $p) { Write-Output "KHONG TIM THAY CUA SO"; exit 1 }
$h = $p.MainWindowHandle

[void][Win32]::ShowWindow($h, 9)

$r0 = New-Object Win32+RECT
[void][Win32]::GetWindowRect($h, [ref]$r0)
$wCu = $r0.Right - $r0.Left
$hCu = $r0.Bottom - $r0.Top
if ($Rong -le 0) { $Rong = $wCu }
if ($Cao -le 0) { $Cao = $hCu }

# HWND_TOPMOST = -1, SWP_SHOWWINDOW = 0x40
[void][Win32]::SetWindowPos($h, [IntPtr](-1), $X, $Y, $Rong, $Cao, 0x40)
[Win32]::SwitchToThisWindow($h, $true)
[void][Win32]::SetForegroundWindow($h)
Start-Sleep -Seconds $ChoGiay

$r = New-Object Win32+RECT
[void][Win32]::GetWindowRect($h, [ref]$r)
$w = $r.Right - $r.Left
$ht = $r.Bottom - $r.Top

$bmp = New-Object System.Drawing.Bitmap $w, $ht
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.CopyFromScreen($r.Left, $r.Top, 0, 0, $bmp.Size)
$bmp.Save($DuongDanLuu, [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()

# HWND_NOTOPMOST = -2, SWP_NOMOVE|SWP_NOSIZE = 0x3
[void][Win32]::SetWindowPos($h, [IntPtr](-2), 0, 0, 0, 0, 0x3)

Write-Output "DA LUU $DuongDanLuu ($w x $ht) tai ($($r.Left),$($r.Top))"
