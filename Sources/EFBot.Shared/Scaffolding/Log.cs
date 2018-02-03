using System;
using System.Reactive.Subjects;
using JetBrains.Annotations;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using ILog = Common.Logging.ILog;
using LogManager = Common.Logging.LogManager;

namespace EFBot.Shared.Scaffolding
{
    public class Log : DisposableReactiveObject
    {
        private static readonly Lazy<Log> InstanceProvider = new Lazy<Log>();

        private readonly Lazy<ILog> loggerInstanceProvider = new Lazy<ILog>(() => LogManager.GetLogger(typeof(Log)));

        public Log()
        {
            Errors.Subscribe(HandleUiException).AddTo(Anchors);
        }

        public static ILog Instance => InstanceProvider.Value.Logger;

        public static ISubject<Exception> ErrorsSubject => InstanceProvider.Value.Errors;

        public ISubject<Exception> Errors { get; } = new Subject<Exception>();

        public ILog Logger => loggerInstanceProvider.Value;

        public static void HandleException([NotNull] Exception exception)
        {
            Instance.Error("Exception occurred", exception);
        }

        public static void HandleUiException([NotNull] Exception exception)
        {
            Instance.Error("UI Exception occurred", exception);
        }

        public static void InitializeLogging(string configurationMode)
        {
            GlobalContext.Properties["configuration"] = configurationMode;
            Instance.Info($"Logging in '{configurationMode}' mode initialized");
        }

        public static void SwitchLoggingLevel(Level loggingLevel)
        {
            var repository = (Hierarchy) log4net.LogManager.GetRepository();
            repository.Root.Level = loggingLevel;
            repository.RaiseConfigurationChanged(EventArgs.Empty);
            Instance.Info($"Logging level switched to '{loggingLevel}'");
        }
    }
}
