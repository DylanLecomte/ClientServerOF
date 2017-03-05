using System;
using System.Windows.Input;

// Classe créee pour gérer l'évenement mettant à jour le mot de passe.
// Nécessaire car utilisation d'un PasswordBox qui ne permet pas de bind directement le mot de passe
public class RelayCommandPassword : ICommand
{
    // Attributs
    private Action<object> command;
    private Func<bool> canExecute;

    // Méthodes

    public RelayCommandPassword(Action<object> commandAction, Func<bool> canExecute = null)
    {
        this.command = commandAction;
        this.canExecute = canExecute;
    }

    public bool CanExecute(object parameter)
    {
        return this.canExecute == null ? true : this.canExecute();
    }

    public event EventHandler CanExecuteChanged;

    public void Execute(object parameter)
    {
        if (this.command != null)
        {
            this.command(parameter);
        }
    }
}