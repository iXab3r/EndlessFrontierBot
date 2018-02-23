using System.Drawing;

namespace EFBot.Shared.GameLogic.ImageReaders
{
    internal interface IGameDialog : IGameElement
    {
        string Text { get; }
        
        IGameButton[] Buttons { get; }
    }

    internal interface IGameButton : IGameElement
    {
        string Text { get; }
    }

    internal interface IGameElement
    {
        Rectangle Area { get; }
    }

    internal interface IUnitAcquisitionGameDialog : IGameDialog
    {
        string UnitPrice { get; }
        
        string UnitName { get; }
    }
}