using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SH8_Sample.Models
{
  public class Totals : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler? PropertyChanged;
    private DateTime? lastPpmCalculationTime = null;
    private double? lastPpmCalculationOverall = null;
    private double? lastPpm = null;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    private double? overall = null;
    public double? Overall
    {
      get => overall;
      set { if (overall != value) { overall = value; OnPropertyChanged(); OnPropertyChanged(nameof(FailPercentage)); OnPropertyChanged(nameof(PassPercentage)); OnPropertyChanged(nameof(PPM)); } }
    }
    private double? pass = null;
    public double? Pass { get => pass; set { if (pass != value) { pass = value; OnPropertyChanged(); OnPropertyChanged(nameof(PassPercentage)); } } }
    private double? fail = null;
    public double? Fail { get => fail; set { if (fail != value) { fail = value; OnPropertyChanged(); OnPropertyChanged(nameof(FailPercentage)); } } }
    public double? FailPercentage { get => ((fail / overall) ?? 0) * 100; }
    public double? PassPercentage { get => ((pass / overall) ?? 0) * 100; }
    public double? PPM
    {
      get
      {
        var newTime = DateTime.Now;
        var elapsed = newTime - lastPpmCalculationTime;
        if (elapsed is null)
        {
          lastPpmCalculationTime = newTime;
          lastPpmCalculationOverall = overall;
          return 0;
        }
        if ((lastPpm ?? 0) > 0 && elapsed.Value.TotalMinutes < .25) { return lastPpm; }
        lastPpm = ((overall - lastPpmCalculationOverall) / elapsed.Value.TotalMinutes) ?? 0;
        lastPpmCalculationTime = newTime;
        lastPpmCalculationOverall = overall;
        return lastPpm;
      }
    }
  }

  public class SherlockReading : INotifyPropertyChanged
  {
    public SherlockReading(string name) { this.VariableName = name; }
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    private string variableName = string.Empty;
    public string VariableName { get => variableName; set { if (variableName != value) { variableName = value; OnPropertyChanged(); } } }
    private dynamic? val;
    public dynamic? Value { get => val; set { if (val != value) { val = value; OnPropertyChanged(); } } }
  }




  public class SherlockAttribute : INotifyPropertyChanged
  {
    public SherlockAttribute(string name) { this.Name = name; }

    private void TotalsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
      switch (e.PropertyName)
      {
        case "Overall":
          OnPropertyChanged(nameof(Percentage));
          break;
      }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    private int seq = 0;
    public int Sequence { get => seq; set { if (seq != value) { seq = value; OnPropertyChanged(); } } }
    private string name = string.Empty;
    public string Name { get => name; set { if (name != value) { name = value; OnPropertyChanged(); } } }
    private string format = "#,##0.00";
    public string Format { get => format; set { if (format != value) { format = value; OnPropertyChanged(); } } }
    private Totals totals = new();
    public Totals Totals { get => totals; set { if (totals != value) { totals = value; OnPropertyChanged(); totals.PropertyChanged += TotalsPropertyChanged; } } }


    private SherlockReading? max;
    public SherlockReading? Max
    {
      get => max;
      set { if (max != value) { max = value; if (max is not null) { max.PropertyChanged += MaxValueChanged; } OnPropertyChanged(); } }
    }
    private void MaxValueChanged(object? sender, PropertyChangedEventArgs e) { if (!isFormatterSet) { SetFormatter(); } OnPropertyChanged(nameof(MaxFormatted)); }
    public string MaxFormatted { get => formatMethodDelegate?.Invoke(max?.Value) ?? (max?.Value is null ? string.Empty : max.Value.ToString()); }


    private SherlockReading? min;
    public SherlockReading? Min
    {
      get => min;
      set { if (min != value) { min = value; if (min is not null) { min.PropertyChanged += MinValueChanged; } OnPropertyChanged(); } }
    }
    private void MinValueChanged(object? sender, PropertyChangedEventArgs e) { if (!isFormatterSet) { SetFormatter(); } OnPropertyChanged(nameof(MinFormatted)); }
    public string MinFormatted { get => formatMethodDelegate?.Invoke(min?.Value) ?? (min?.Value is null ? string.Empty : min.Value.ToString()); }


    private SherlockReading? cur;
    public SherlockReading? Cur
    {
      get => cur;
      set { if (cur != value) { cur = value; if (cur is not null) { cur.PropertyChanged += CurValueChanged; } OnPropertyChanged(); } }
    }
    private void CurValueChanged(object? sender, PropertyChangedEventArgs e) { if (!isFormatterSet) { SetFormatter(); } OnPropertyChanged(nameof(CurFormatted)); }

    private delegate string FormatMethod(object? val);
    private FormatMethod? formatMethodDelegate;
    private FormatMethod? formatMethodFailDelegate;
    private bool isFormatterSet = false;
    public void SetFormatter()
    {
      isFormatterSet = false;
      if (cur?.Value is null) { return; }

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8605 // Unboxing a possibly null value.
      if (cur.Value.GetType() == typeof(string))
      {
        formatMethodDelegate = p => (string)p;
        formatMethodFailDelegate = p => (string)p;
      }
      else if (cur.Value.GetType() == typeof(double))
      {
        formatMethodDelegate = p => { try { return ((double)p).ToString(format); } catch (Exception) { isFormatterSet = false; return null; } };
        formatMethodFailDelegate = p => { try { return ((double)p).ToString("#,##0"); } catch (Exception) { isFormatterSet = false; return null; } };
      }
#pragma warning restore CS8605 // Unboxing a possibly null value.
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

      isFormatterSet = true;
    }

    public string CurFormatted
    {
      get => formatMethodDelegate?.Invoke(cur?.Value) ?? (cur?.Value is null ? string.Empty : cur.Value.ToString());
    }
    private SherlockReading? fail;
    public SherlockReading? Fail
    {
      get => fail;
      set { if (fail != value) { fail = value; if (fail is not null) { fail.PropertyChanged += FailValueChanged; } OnPropertyChanged(); } }
    }
    private void FailValueChanged(object? sender, PropertyChangedEventArgs e)
    {
      OnPropertyChanged(nameof(FailFormatted));
      OnPropertyChanged(nameof(Percentage));
    }
    public string FailFormatted { get => formatMethodFailDelegate?.Invoke(fail?.Value) ?? string.Empty; }



    //private delegate double PercetageMethod(double total);
    //private PercetageMethod? PercetageMethodDelegate;

    public double Percentage { get => ((fail?.Value / totals.Overall) ?? 0) * 100; }
  }
}
