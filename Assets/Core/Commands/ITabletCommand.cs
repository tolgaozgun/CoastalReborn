using System;

namespace Core.Commands
{
    /// <summary>
    /// Base interface for all tablet commands.
    /// Follows Command pattern for encapsulating operations.
    /// </summary>
    public interface ITabletCommand
    {
        string Id { get; }
        string Description { get; }
        bool CanExecute();
        void Execute();
        void Undo();
    }
}