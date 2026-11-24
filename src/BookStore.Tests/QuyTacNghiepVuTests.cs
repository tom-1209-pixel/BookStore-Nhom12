using BookStore.Services.Common;

namespace BookStore.Tests;

/// <summary>Kiem thu cong thuc tinh phi van chuyen (BR07).</summary>
public class QuyTacNghiepVuTests
{
    [Theory]
    [InlineData(0, 25_000)]
    [InlineData(99_000, 25_000)]
    [InlineData(299_999, 25_000)]
    [InlineData(300_000, 0)]          // dung nguong thi duoc mien phi
    [InlineData(1_000_000, 0)]
    public void TinhPhiVanChuyen_TraVeDungTheoNguong(decimal tamTinh, decimal phiMongDoi)
    {
        var phi = QuyTacNghiepVu.TinhPhiVanChuyen(tamTinh);
        Assert.Equal(phiMongDoi, phi);
    }

    [Fact]
    public void CacHangSoNghiepVu_DungNhuMoTaTrongBaoCao()
    {
        Assert.Equal(25_000m, QuyTacNghiepVu.PhiVanChuyen);
        Assert.Equal(300_000m, QuyTacNghiepVu.NguongMienPhiVanChuyen);
        Assert.Equal(30, QuyTacNghiepVu.SoPhutHetHanThanhToan);
    }
}
