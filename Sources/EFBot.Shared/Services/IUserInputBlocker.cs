using System;

namespace EFBot.Shared.Services {
    internal interface IUserInputBlocker
    {
        IDisposable Block();
    }
}