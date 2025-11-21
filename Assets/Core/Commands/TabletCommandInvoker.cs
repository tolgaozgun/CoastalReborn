using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Commands
{
    /// <summary>
    /// Invoker for tablet commands with undo/redo support.
    /// Implements Command pattern with history tracking.
    /// </summary>
    public class TabletCommandInvoker : MonoBehaviour
    {
        private readonly Stack<ITabletCommand> commandHistory = new Stack<ITabletCommand>();
        private readonly Stack<ITabletCommand> undoStack = new Stack<ITabletCommand>();
        private readonly Dictionary<string, ITabletCommand> pendingCommands = new Dictionary<string, ITabletCommand>();

        // Events for UI integration
        public event Action<ITabletCommand> OnCommandExecuted;
        public event Action<ITabletCommand> OnCommandUndone;
        public event Action<ITabletCommand> OnCommandFailed;

        /// <summary>
        /// Executes a command and adds it to history.
        /// </summary>
        public bool ExecuteCommand(ITabletCommand command)
        {
            if (command == null)
            {
                Debug.LogError("[TabletCommandInvoker] Cannot execute null command");
                return false;
            }

            try
            {
                if (!command.CanExecute())
                {
                    Debug.LogWarning($"[TabletCommandInvoker] Command cannot be executed: {command.Description}");
                    OnCommandFailed?.Invoke(command);
                    return false;
                }

                command.Execute();

                // Clear redo stack when new command is executed
                undoStack.Clear();
                commandHistory.Push(command);

                OnCommandExecuted?.Invoke(command);
                Debug.Log($"[TabletCommandInvoker] Executed command: {command.Description}");

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TabletCommandInvoker] Error executing command {command.Description}: {ex.Message}");
                OnCommandFailed?.Invoke(command);
                return false;
            }
        }

        /// <summary>
        /// Adds a command to pending queue for batch execution.
        /// </summary>
        public void AddPendingCommand(ITabletCommand command)
        {
            if (command == null)
            {
                Debug.LogError("[TabletCommandInvoker] Cannot add null pending command");
                return;
            }

            pendingCommands[command.Id] = command;
        }

        /// <summary>
        /// Executes all pending commands.
        /// </summary>
        public bool ExecutePendingCommands()
        {
            var commands = new List<ITabletCommand>(pendingCommands.Values);
            pendingCommands.Clear();

            var allSuccess = true;
            foreach (var command in commands)
            {
                if (!ExecuteCommand(command))
                    allSuccess = false;
            }

            return allSuccess;
        }

        /// <summary>
        /// Undoes the last executed command.
        /// </summary>
        public bool UndoLastCommand()
        {
            if (commandHistory.Count == 0)
            {
                Debug.LogWarning("[TabletCommandInvoker] No commands to undo");
                return false;
            }

            var command = commandHistory.Pop();
            try
            {
                command.Undo();
                undoStack.Push(command);

                OnCommandUndone?.Invoke(command);
                Debug.Log($"[TabletCommandInvoker] Undone command: {command.Description}");

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TabletCommandInvoker] Error undoing command {command.Description}: {ex.Message}");
                // Push back to history if undo failed
                commandHistory.Push(command);
                return false;
            }
        }

        /// <summary>
        /// Redoes the last undone command.
        /// </summary>
        public bool RedoLastCommand()
        {
            if (undoStack.Count == 0)
            {
                Debug.LogWarning("[TabletCommandInvoker] No commands to redo");
                return false;
            }

            var command = undoStack.Pop();
            try
            {
                command.Execute();
                commandHistory.Push(command);

                OnCommandExecuted?.Invoke(command);
                Debug.Log($"[TabletCommandInvoker] Redone command: {command.Description}");

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TabletCommandInvoker] Error redoing command {command.Description}: {ex.Message}");
                // Push back to undo stack if redo failed
                undoStack.Push(command);
                return false;
            }
        }

        /// <summary>
        /// Checks if undo is available.
        /// </summary>
        public bool CanUndo()
        {
            return commandHistory.Count > 0;
        }

        /// <summary>
        /// Checks if redo is available.
        /// </summary>
        public bool CanRedo()
        {
            return undoStack.Count > 0;
        }

        /// <summary>
        /// Gets the count of executed commands in history.
        /// </summary>
        public int GetHistoryCount()
        {
            return commandHistory.Count;
        }

        /// <summary>
        /// Gets the count of commands that can be redone.
        /// </summary>
        public int GetUndoCount()
        {
            return undoStack.Count;
        }

        /// <summary>
        /// Clears all command history.
        /// </summary>
        public void ClearHistory()
        {
            commandHistory.Clear();
            undoStack.Clear();
            pendingCommands.Clear();
            Debug.Log("[TabletCommandInvoker] Command history cleared");
        }

        /// <summary>
        /// Validates all pending commands.
        /// </summary>
        public bool ValidatePendingCommands()
        {
            foreach (var command in pendingCommands.Values)
            {
                if (!command.CanExecute())
                    return false;
            }
            return true;
        }

        private void Update()
        {
            // Optional: Handle keyboard shortcuts for undo/redo during development
            if (Application.isEditor && Input.GetKeyDown(KeyCode.Z) && Input.GetKey(KeyCode.LeftControl))
            {
                UndoLastCommand();
            }
            else if (Application.isEditor && Input.GetKeyDown(KeyCode.Y) && Input.GetKey(KeyCode.LeftControl))
            {
                RedoLastCommand();
            }
        }
    }
}