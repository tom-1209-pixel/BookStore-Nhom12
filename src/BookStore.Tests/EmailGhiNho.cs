using BookStore.Services.Common;
using BookStore.Services.Interfaces;

namespace BookStore.Tests;

/// <summary>Thay the EmailService khi kiem thu: luu lai cac thu da "gui".</summary>
public class EmailGhiNho : IEmailService
{
    public List<(string NguoiNhan, string TieuDe, string NoiDung)> Thu { get; } = new();

    public Task<KetQua> GuiAsync(string nguoiNhan, string tieuDe, string noiDungHtml)
    {
        Thu.Add((nguoiNhan, tieuDe, noiDungHtml));
        return Task.FromResult(KetQua.Dung());
    }
}
