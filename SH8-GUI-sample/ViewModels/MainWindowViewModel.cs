using ipeenginectrlLib;
using SH8_Sample.Models;
using SH8_Sample.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SH8Res = ipeenginectrlLib.Result;

namespace SH8_Sample.ViewModels
{
  // must implement INotifyPropertyChanged to update the view when properties change
  public class MainWindowViewModel : INotifyPropertyChanged
  {
    private System.Threading.Timer? tmrStatusReset;
    private const int clearHeaderTime = 10000;

    #region PROPERTIES ##################################################################################################################################
    // required for INotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    private ipeenginectrlLib.AxIpeEngine? hSh8;
    public ipeenginectrlLib.AxIpeEngine? HSh8 { get => hSh8; private set { if (hSh8 != value) { hSh8 = value; OnPropertyChanged(); } } }

    private bool isConnected;
    public bool IsConnected
    {
      get => isConnected;
      set
      {
        if (isConnected != value)
        {
          isConnected = value;
          OnPropertyChanged(); // trigger the ui to update
          OnPropertyChanged(nameof(IsNotConnected)); // trigger the ui to also update this property
        }
      }
    }
    // a readonly property. no way to trigger OnPropertyChanged
    public bool IsNotConnected { get => !IsConnected; }
    private bool isInAuto;
    public bool IsInAuto
    {
      get => isInAuto;
      set
      {
        if (isInAuto != value)
        {
          isInAuto = value;
          OnPropertyChanged(); // trigger the ui to update
          OnPropertyChanged(nameof(IsNotInAuto)); // trigger the ui to also update this property
        }
      }
    }
    // a readonly property. no way to trigger OnPropertyChanged
    public bool IsNotInAuto { get => !IsInAuto; }
    public string Filename
    {
      get { return string.IsNullOrWhiteSpace(Settings.Default.LastFilename) ? $"{System.AppDomain.CurrentDomain.BaseDirectory}include\\sample_inspection.sh8" : Settings.Default.LastFilename; }
      set { if (Settings.Default.LastFilename != value) { Settings.Default.LastFilename = value; OnPropertyChanged(); } }
    }

    private ObservableCollection<SherlockAttribute> sherlockAttributeList = new();
    public ObservableCollection<SherlockAttribute> SherlockAttributeList { get => sherlockAttributeList; set { if (sherlockAttributeList != value) { sherlockAttributeList = value; OnPropertyChanged(); } } }

    private Totals totals = new();
    public Totals Totals
    {
      get => totals;
    }
    //private double? total = 0;
    //public double? Total
    //{
    //  get => total;
    //  // only settable from within the class
    //  private set { if (total != value) { total = value; OnPropertyChanged(); } }
    //}
    //private double pass = 0;
    //public double Pass
    //{
    //  get => pass;
    //  // only settable from within the class
    //  private set { if (pass != value) { pass = value; OnPropertyChanged(); } }
    //}
    //private double fail = 0;
    //public double Fail
    //{
    //  get => fail;
    //  // only settable from within the class
    //  private set { if (fail != value) { fail = value; OnPropertyChanged(); } }
    //}

    private string statusText = string.Empty;
    public string StatusText
    {
      get => statusText;
      // only settable from within the class
      private set { if (statusText != value) { statusText = value; OnPropertyChanged(); } }
    }
    public System.Windows.Media.Brush statusBrush = System.Windows.Media.Brushes.Transparent;
    public System.Windows.Media.Brush StatusBrush
    {
      get => statusBrush;
      private set { if (statusBrush != value) { statusBrush = value; OnPropertyChanged(); } }
    }
    #endregion

    #region METHODS ##################################################################################################################################
    public bool Dispose()
    {
      /* code that should run to clean up resources */
      try
      {
        if (!IsConnected) { return true; }
        if (IsInAuto) { StopMachine(); }
        // check that stopping was successful
        if (IsInAuto) { return false; }
        HSh8?.disconnect();
        HSh8?.terminate();
        return true;
      }
      catch (Exception)
      {
      }
      return false;
    }
    public bool ConnectSherlock()
    {
      if (string.IsNullOrWhiteSpace(Filename)) { AssignStatusText("Select a file first", 3); return false; }
      if (!System.IO.File.Exists(Filename)) { AssignStatusText("File not found", 3); return false; }
      try
      {
        HSh8 = new();
        // initialize the engine
        if (HSh8.initialize("default") != SH8Res.Ok) { IsConnected = false; return false; }
        // load the file
        if (HSh8.load(Filename) != SH8Res.Ok) { IsConnected = false; return false; }
        // connect events
        HSh8.programLoopCompleted += GetSherlockValues;
        // GUI update if bound
        IsConnected = true;
        // build class objects
        IntializeComponents();
        // load the initial values into the GUI
        GetSherlockValues(false);
        return true;
      }
      catch (Exception ex)
      {
        // don't crash on an error. show it
        AssignStatusText($"{ex.Message}", 4);
        // GUI update if bound
        IsConnected = false;
        return false;
      }
    }


    private void IntializeComponents()
    {
      SherlockAttributeList.Clear();
      SherlockAttributeList.Add(new SherlockAttribute("avg grayscale")
      {
        Max = new SherlockReading("AvgPixMax"),
        Min = new SherlockReading("AvgPixMin"),
        Cur = new SherlockReading("AvgPixCur"),
        Fail = new SherlockReading("AvgPixFail"),
        Totals=this.Totals
      });
      SherlockAttributeList.Add(new SherlockAttribute("blob count")
      {
        Format = "#,##0",
        Max = new SherlockReading("BlobsMax"),
        Min = new SherlockReading("BlobsMin"),
        Cur = new SherlockReading("BlobsCur"),
        Fail = new SherlockReading("BlobsFail"),
        Totals = this.Totals
      });
      SherlockAttributeList.Add(new SherlockAttribute("pass/fail")
      {
        Format = string.Empty,
        Cur = new SherlockReading("Status"),
        Totals = this.Totals
      });
    }
    private void GetSherlockValues()
    {
      GetSherlockValues(true);
    }
    private void GetSherlockValues(bool currentOnly)
    {
      if (HSh8 == null) { return; }
      try
      {
        // get the values from the engine and assign to properties which will update GUI
        // these Sherlock variables must exist. it will fail if you loaded a program without them

        Totals.Overall = (double)HSh8.value("Total.value");
        Totals.Pass = (double)HSh8.value("Pass.value");
        Totals.Fail = (double)HSh8.value("Fail.value");
      }
      catch (Exception ex)
      {
        // don't crash on an error. show it
        AssignStatusText($"{ex.Message}", 4);
      }
      foreach (var reading in SherlockAttributeList)
      {
        GetSherlockReading(reading.Cur);
        GetSherlockReading(reading.Fail);
        if (!currentOnly)
        {
          GetSherlockReading(reading.Max);
          GetSherlockReading(reading.Min);
        }
      }
    }
    private void GetSherlockReading(SherlockReading? p)
    {
      try
      {
        if (p != null) { p.Value = HSh8?.value($"{p.VariableName}.value"); }
      }
      catch (Exception ex)
      {
        // don't crash on an error. show it
        AssignStatusText($"{ex.Message}", 4);
      }
    }
    private void ResetVariables()
    {
      if (HSh8 == null) { AssignStatusText("Sherlock not initialized", 3); return; }
      try
      {
        // assign all properties to 0
        Totals.Overall = Totals.Pass = Totals.Fail = 0;
        // assign the values to the engine
        // these Sherlock variables must exist. it will fail if you loaded a program without them
        HSh8.setValue("Total.value", totals.Overall);
        HSh8.setValue("Pass.value", Totals.Pass);
        HSh8.setValue("Fail.value", Totals.Fail);
        foreach (var reading in SherlockAttributeList)
        {
          if (reading.Fail is not null)
          {
            reading.Fail.Value = 0;
            HSh8.setValue($"{reading.Fail.VariableName}.value", 0);
          }
        }

        // success
        AssignStatusText("Values reset");
      }
      catch (Exception ex)
      {
        // don't crash on an error. show it
        AssignStatusText($"{ex.Message}", 4);
      }
    }
    private void RunMachine()
    {
      try
      {
        // start the inspection and get result. if it fails, show that we are not successful and quit
        if (HSh8?.run(-1) != SH8Res.Ok) { AssignStatusText("Could not start inspection", 4); return; }
        // GUI update if bound
        IsInAuto = true;
        // success
        AssignStatusText("Inspection is running", 1);
      }
      catch (Exception ex)
      {
        // don't crash on an error. show it
        AssignStatusText($"{ex.Message}", 4);
      }
    }
    private void StopMachine()
    {
      if (HSh8 == null) { return; }
      try
      {
        // request a stop first before trying to abort. if api call fails (returns not ok), abort
        if (HSh8.requestStop() != SH8Res.Ok) { AssignStatusText("Inspection could not be stopped properly", 4); return; }
        int tries = 10;
        while (HSh8.isRunning())
        {
          Thread.Sleep(100); // wait for the machine to stop
          tries--;
          if (tries < 0) { break; } // give up after 10 tries
        }
        tries = 10;
        if (HSh8.isRunning())
        {
          // if we are still running, try to abort
          if (HSh8.requestAbort() != SH8Res.Ok) { AssignStatusText("Inspection could not be stopped properly", 4); return; }
          while (HSh8.isRunning())
          {
            Thread.Sleep(100); // wait for the machine to abort
            tries--;
            if (tries < 0)
            {
              // cancel
              AssignStatusText("Inspection could not be stopped properly", 4);
              return;
            }
          }
        }
        // success
        AssignStatusText("Inspection stopped.", 2);
        // GUI update if bound
        IsInAuto = false;
      }
      catch (Exception ex)
      {
        // don't crash on an error. show it
        AssignStatusText(ex.Message, 2);
      }
    }
    public void AssignStatusText(string text, int level = 0)
    {
      // this method can be called locally or from the GUI to set a status message and background color
      StatusText = text;
      if (level <= 0) { StatusBrush = System.Windows.Media.Brushes.Transparent; }
      else if (level > 3) { StatusBrush = System.Windows.Media.Brushes.OrangeRed; }
      else if (level > 2) { StatusBrush = System.Windows.Media.Brushes.Yellow; }
      else if (level > 1) { StatusBrush = System.Windows.Media.Brushes.LightYellow; }
      else if (level > 0) { StatusBrush = System.Windows.Media.Brushes.LimeGreen; }
      // clear the text and background after a short time, this may need adjusted if statustext is set multiple times quickly
      if (tmrStatusReset == null) { tmrStatusReset = new(p => ClearStatus(), null, clearHeaderTime, Timeout.Infinite); } else { tmrStatusReset?.Change(clearHeaderTime, Timeout.Infinite); }
    }
    private void ClearStatus()
    {
      System.Windows.Application.Current.Dispatcher.Invoke(() =>
      {
        // clear the status text and background
        StatusText = string.Empty;
        StatusBrush = System.Windows.Media.Brushes.Transparent;
      });
    }
    #endregion

    #region COMMANDS ##################################################################################################################################
    ICommand? resetCommand;
    public ICommand ResetCommand => resetCommand ??= new Command(() => Task.Run(ResetVariables), p => true); // always enabled
    ICommand? stopCommand;
    public ICommand StopCommand => stopCommand ??= new Command(async () => { await Task.Run(StopMachine); CommandManager.InvalidateRequerySuggested(); }, p => IsConnected && IsInAuto);
    ICommand? startCommand;
    public ICommand StartCommand => startCommand ??= new Command(async () => { await Task.Run(RunMachine); CommandManager.InvalidateRequerySuggested(); }, p => IsConnected && IsNotInAuto);
    #endregion
  }
}
