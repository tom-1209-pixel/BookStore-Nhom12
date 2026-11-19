using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Api.Middlewares;

/// <summary>
/// Middleware xu ly loi tap trung (muc 4.5 Thiet ke thich ung).
/// Moi ngoai le chua duoc xu ly se duoc ghi log kem ma truy vet (TraceId)
/// va tra ve cho client duoi dang ProblemDetails theo RFC 7807, khong lo
/// thong tin ky thuat ra ngoai.
/// </summary>
public class XuLyLoiMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<XuLyLoiMiddleware> _log;

    public XuLyLoiMiddleware(RequestDelegate next, ILogger<XuLyLoiMiddleware> log)
    {
        _next = next;
        _log = log;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        var batDau = Stopwatch.GetTimestamp();
        try
        {
            await _next(ctx);

            var ms = Stopwatch.GetElapsedTime(batDau).TotalMilliseconds;
            if (ms > 1000)
                _log.LogWarning("Yêu cầu {Method} {Path} chạy chậm: {Ms:0} ms.",
                    ctx.Request.Method, ctx.Request.Path, ms);
        }
        catch (Exception ex)
        {
            var maTruyVet = ctx.TraceIdentifier;
            _log.LogError(ex, "Lỗi chưa xử lý tại {Method} {Path}. Mã truy vết {MaTruyVet}.",
                ctx.Request.Method, ctx.Request.Path, maTruyVet);

            if (ctx.Response.HasStarted) throw;

            ctx.Response.Clear();
            ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await ctx.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Lỗi hệ thống",
                Detail = "Đã xảy ra lỗi khi xử lý yêu cầu. Vui lòng thử lại hoặc liên hệ bộ phận kỹ thuật.",
                Instance = ctx.Request.Path,
                Extensions = { ["maTruyVet"] = maTruyVet }
            });
        }
    }
}
