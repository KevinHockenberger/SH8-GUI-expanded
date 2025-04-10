using SH8_Sample.Properties;
using SH8_Sample.ViewModels;
using System.Windows;
using AxipedisplayctrlLib;

namespace SH8_Sample
{  /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
  public partial class MainWindow : Window
  {
    // assigned this way allows backend methods to reference the viewmodel
    static MainWindowViewModel vm = new();
    public MainWindow()
    {
      // this is what links the view to the viewmodel at runtime
      DataContext = vm;
      // or you could simply do this if you didn't need the reference
      //DataContext = new MainWindowViewModel();
      InitializeComponent();
    }
    private void Window_Initialized(object sender, EventArgs e)
    {
      ApplyAppSettings();
      ApplyUserSettings();
      vm.AssignStatusText("Load a Sherlock program at the top.", 0);
    }
    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      SaveAppSettings();
      if (!vm.Dispose())
      {
        Thread.Sleep(1000);
        if (!vm.Dispose())
        {
          Thread.Sleep(1000);
          if (!vm.Dispose())
          {
            // could not stop ...
            e.Cancel = true;
            vm.AssignStatusText("Could not close application", 4);
            return;
          }
        }
      }
    }
    private void ApplyAppSettings()
    {
      // assign the last used settings from the application settings file
      if (Settings.Default.UpgradeRequired)
      {
        Settings.Default.Upgrade();
        Settings.Default.UpgradeRequired = false;
        Settings.Default.Save();
      }
      Width = Settings.Default.LastSavedWidth;
      Height = Settings.Default.LastSavedHeight; if (Height < 0) { Height = 0; }
      Top = Settings.Default.LastSavedTop;
      Left = Settings.Default.LastSavedLeft;
      WindowState = Settings.Default.LastWindowState == "Maximized" ? WindowState.Maximized : WindowState.Normal;
      // vaidate the window is somewhat on screen and large enough to view
      if (Top < 0) { Top = 0; }
      else if (Top > SystemParameters.VirtualScreenHeight) { Top = 0; }
      if (Left < 0) { Left = 0; }
      if (Width < 325) { Width = 325; }
      else if (Width > SystemParameters.VirtualScreenWidth) { Width = SystemParameters.VirtualScreenWidth; }
      if (Height < 200) { Height = 200; }
      else if (Height > SystemParameters.VirtualScreenHeight) { Height = SystemParameters.VirtualScreenHeight; }
    }
    private void SaveAppSettings()
    {
      // save the current settings to the application settings file. typically befoe closing but can be set anytime
      Settings.Default.LastWindowState = this.WindowState.ToString();
      Settings.Default.LastSavedWidth = Width;
      Settings.Default.LastSavedHeight = Height;
      Settings.Default.LastSavedTop = Top;
      Settings.Default.LastSavedLeft = Left;
      Settings.Default.Save();
    }
    private void ApplyUserSettings()
    {
      // any code that should load user/file settings from another config or database
    }
    private void InitSherlock(object sender, RoutedEventArgs e)
    {
      // since we are loading an image window control, this code is here and not in a view model command
      try
      {
        // initialize the Sherlock engine
        vm.ConnectSherlock();
        // create a Sherlock image window
        AxAxIpeDisplay imgWindow = new AxAxIpeDisplay();
        // create a WinForms host to use in WPF
        System.Windows.Forms.Integration.WindowsFormsHost host = new();
        // assign the image window to the host
        host.Child = imgWindow;
        // put the host on the form ImageArea is actually a border control in XAML
        ImageArea.Child = host;
        // connect the image window to the Sherlock engine
        vm.HSh8?.connectDisplay(imgWindow.dspHandle, "workspace_mono8A");
        // we done. let 'em know
        vm.AssignStatusText("Sherlock ready.", 1);
      }
      catch (Exception ex)
      {
        // don't crash on an error. show it
        vm.AssignStatusText(ex.Message, 2);
      }
    }
  }
}
