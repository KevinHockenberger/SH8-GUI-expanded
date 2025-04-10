using SH8_Sample.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace SH8_Sample.ViewModels.Converters
{
  //any custom converters you want to use in your xaml
  //public class BoolToBrushColorConverter : MarkupExtension, IValueConverter
  //{
  //  private System.Windows.Media.Brush TrueValue { get; set; } = new SolidColorBrush(System.Windows.Media.Colors.Red); // could be used directly instead of defining here but for consistency it is defined
  //  private System.Windows.Media.Brush FalseValue { get; set; } = new SolidColorBrush(System.Windows.Media.Colors.Lime); // could be used directly instead of defining here but for consistency it is defined
  //  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  //  {
  //    if (value is not bool) return System.Windows.Data.Binding.DoNothing;
  //    return (bool)value ? TrueValue : FalseValue;
  //  }
  //  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  //  {
  //    if (value == TrueValue) return true;
  //    if (value == FalseValue) return false;
  //    return System.Windows.Data.Binding.DoNothing;
  //  }
  //  public override object ProvideValue(IServiceProvider serviceProvider)
  //  {
  //    return this;
  //  }
  //}
  public class DoubleToZeroDecimalsFormatter : MarkupExtension, IValueConverter
  {
    private System.Windows.Media.Brush TrueValue { get; set; } = new SolidColorBrush(System.Windows.Media.Colors.Red); // could be used directly instead of defining here but for consistency it is defined
    private System.Windows.Media.Brush FalseValue { get; set; } = new SolidColorBrush(System.Windows.Media.Colors.Lime); // could be used directly instead of defining here but for consistency it is defined
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is not double) return System.Windows.Data.Binding.DoNothing;
      return ((double)value).ToString(StaticGlobals.intformat);
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == TrueValue) return true;
      if (value == FalseValue) return false;
      return System.Windows.Data.Binding.DoNothing;
    }
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      return this;
    }
  }
  public class DoubleToThreeDecimalsFormatter :MarkupExtension, IValueConverter
  {
    private System.Windows.Media.Brush TrueValue { get; set; } = new SolidColorBrush(System.Windows.Media.Colors.Red); // could be used directly instead of defining here but for consistency it is defined
    private System.Windows.Media.Brush FalseValue { get; set; } = new SolidColorBrush(System.Windows.Media.Colors.Lime); // could be used directly instead of defining here but for consistency it is defined
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is not double) return System.Windows.Data.Binding.DoNothing;
      return ((double)value).ToString(StaticGlobals.decimalformat);
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == TrueValue) return true;
      if (value == FalseValue) return false;
      return System.Windows.Data.Binding.DoNothing;
    }
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      return this;
    }
  }

}
