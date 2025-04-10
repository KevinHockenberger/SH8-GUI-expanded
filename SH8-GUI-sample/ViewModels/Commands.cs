using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SH8_Sample.ViewModels
{
  // use the relay command for commands that take a parameter
  public class RelayCommand<T>(Action<T?>? execute, Predicate<T?> canExecute) : ICommand
  {
    private readonly Action<T?> _execute = execute ?? throw new ArgumentNullException(nameof(execute));

    public RelayCommand(Action<T?> execute) : this(execute, (o) => { return true; }) { }

    public bool CanExecute(object? parameter)
    {
      return canExecute == null || canExecute((T?)parameter);
    }

    public virtual event EventHandler? CanExecuteChanged
    {
      add { CommandManager.RequerySuggested += value; }
      remove { CommandManager.RequerySuggested -= value; }
    }
    public void Execute(object? parameter)
    {
      _execute((T?)parameter);
    }
  }
  // a simple command for commands that don't take a parameter
  public class Command(Action? execute, Predicate<bool> canExecute) : ICommand
  {
    private readonly Action _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    public Command(Action execute) : this(execute, p => { return true; }) { }
    public bool CanExecute(object? parameter)
    {
      return canExecute == null || canExecute(false);
    }

    public virtual event EventHandler? CanExecuteChanged
    {
      add { CommandManager.RequerySuggested += value; }
      remove { CommandManager.RequerySuggested -= value; }
    }
    public void Execute(object? parameter)
    {
      _execute();
    }
  }

}
