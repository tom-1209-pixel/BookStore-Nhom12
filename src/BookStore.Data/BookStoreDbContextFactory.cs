using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BookStore.Data;

/// <summary>
/// Factory danh cho cong cu dong lenh "dotnet ef". Khi chay migration, EF Core
/// khong khoi dong ung dung nen can mot cach tao DbContext rieng.
/// </summary>
public class BookStoreDbContextFactory : IDesignTimeDbContextFactory<BookStoreDbContext>
{
    public const string ChuoiKetNoiMacDinh =
        "Server=localhost;Database=QuanLyBanSachTrucTuyen;Trusted_Connection=True;" +
        "TrustServerCertificate=True;MultipleActiveResultSets=True";

    public BookStoreDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<BookStoreDbContext>()
            .UseSqlServer(ChuoiKetNoiMacDinh)
            .Options;
        return new BookStoreDbContext(options);
    }
}
