using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace BookStore.Admin.Converters;

/// <summary>Dao nguoc gia tri bool, dung de tat nut khi dang xu ly.</summary>
public class NghichDaoBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => value is bool b && !b;

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => value is bool b && !b;
}

/// <summary>Chuyen bool thanh Visibility. ConverterParameter="nguoc" de dao nguoc.</summary>
public class BoolSangVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var hien = value is bool b && b;
        if (parameter as string == "nguoc") hien = !hien;
        return hien ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => value is Visibility v && v == Visibility.Visible;
}

/// <summary>Tra ve true khi chuoi co noi dung, dung de bat tat InfoBar.</summary>
public class ChuoiCoNoiDungConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => !string.IsNullOrWhiteSpace(value as string);

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}

/// <summary>NumberBox chi lam viec voi double nen can chuyen qua lai voi decimal.</summary>
public class DecimalSangDoubleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => value is decimal d ? (double)d : 0d;

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => value is double d && !double.IsNaN(d) ? (decimal)d : 0m;
}

/// <summary>NumberBox chi lam viec voi double nen can chuyen qua lai voi int.</summary>
public class IntSangDoubleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => value is int i ? (double)i : 0d;

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => value is double d && !double.IsNaN(d) ? (int)d : 0;
}

/// <summary>Chon mau nhan theo trang thai don hang.</summary>
public class TrangThaiDonSangMauConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var ma = value is byte b ? b : (byte)255;
        var mau = ma switch
        {
            0 => Windows.UI.Color.FromArgb(255, 245, 166, 35),   // Cho thanh toan
            1 => Windows.UI.Color.FromArgb(255, 25, 118, 210),   // Cho xac nhan
            2 => Windows.UI.Color.FromArgb(255, 56, 142, 60),    // Da xac nhan
            3 => Windows.UI.Color.FromArgb(255, 123, 31, 162),   // Dang giao
            4 => Windows.UI.Color.FromArgb(255, 27, 94, 32),     // Hoan thanh
            5 => Windows.UI.Color.FromArgb(255, 198, 40, 40),    // Da huy
            _ => Windows.UI.Color.FromArgb(255, 120, 120, 120)
        };
        return new Microsoft.UI.Xaml.Media.SolidColorBrush(mau);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}
