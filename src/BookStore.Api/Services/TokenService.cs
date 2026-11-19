using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BookStore.Services.Dtos;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BookStore.Api.Services;

public class CauHinhJwt
{
    public string KhoaBiMat { get; set; } = string.Empty;
    public string Issuer { get; set; } = "BookStore.Api";
    public string Audience { get; set; } = "BookStore.Admin";
    public int SoPhutHieuLuc { get; set; } = 480;
}

public interface ITokenService
{
    (string Token, DateTime HetHan) TaoToken(NguoiDungDto nguoiDung);
}

/// <summary>
/// Sinh token JWT cho ung dung Desktop. Token chua ma tai khoan, ten dang nhap,
/// vai tro va ma nhan vien de API phan quyen theo Role.
/// </summary>
public class TokenService : ITokenService
{
    public const string ClaimMaQuanLy = "ma_quan_ly";

    private readonly CauHinhJwt _cauHinh;

    public TokenService(IOptions<CauHinhJwt> cauHinh) => _cauHinh = cauHinh.Value;

    public (string Token, DateTime HetHan) TaoToken(NguoiDungDto nguoiDung)
    {
        var hetHan = DateTime.UtcNow.AddMinutes(_cauHinh.SoPhutHieuLuc);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, nguoiDung.MaTaiKhoan.ToString()),
            new(ClaimTypes.NameIdentifier, nguoiDung.MaTaiKhoan.ToString()),
            new(ClaimTypes.Name, nguoiDung.TenDangNhap),
            new(ClaimTypes.Role, nguoiDung.VaiTro),
            new("ho_ten", nguoiDung.HoTen)
        };

        if (nguoiDung.MaQuanLy.HasValue)
            claims.Add(new Claim(ClaimMaQuanLy, nguoiDung.MaQuanLy.Value.ToString()));

        var khoa = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cauHinh.KhoaBiMat));
        var token = new JwtSecurityToken(
            issuer: _cauHinh.Issuer,
            audience: _cauHinh.Audience,
            claims: claims,
            expires: hetHan,
            signingCredentials: new SigningCredentials(khoa, SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), hetHan);
    }

    private static class JwtRegisteredClaimNames
    {
        public const string Sub = "sub";
    }
}
