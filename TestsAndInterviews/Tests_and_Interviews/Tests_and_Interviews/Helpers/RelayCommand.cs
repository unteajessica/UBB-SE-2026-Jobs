// <copyright file="RelayCommand.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews.Helpers
{
    using System;
    using System.Windows.Input;

    /// <summary>
    /// A command implementation that delegates execution and can-execute logic to provided callbacks.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> execute;
        private readonly Predicate<object?>? canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class with a parameterized execute action.
        /// </summary>
        /// <param name="execute">The action to invoke when the command is executed.</param>
        /// <param name="canExecute">An optional predicate that determines whether the command can execute.</param>
        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class with a parameterless execute action.
        /// </summary>
        /// <param name="execute">The action to invoke when the command is executed.</param>
        /// <param name="canExecute">An optional function that determines whether the command can execute.</param>
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
            : this(
                execute: obj => execute(),
                canExecute: canExecute == null ? null : obj => canExecute())
        {
        }

        /// <summary>
        /// Occurs when the ability to execute the command has changed.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. Can be null.</param>
        /// <returns>True if the command can execute; otherwise false.</returns>
        public bool CanExecute(object? parameter)
        {
            return this.canExecute == null || this.canExecute(parameter);
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">Data used by the command. Can be null.</param>
        public void Execute(object? parameter)
        {
            this.execute(parameter);
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event to notify listeners that the command's ability to execute has changed.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}