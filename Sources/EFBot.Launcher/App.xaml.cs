using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using EFBot.Launcher.Prism;
using EFBot.Shared.Prism;
using EFBot.Shared.Scaffolding;
using log4net.Core;
using ReactiveUI;
using Unity;

namespace EFBot.Launcher
{
    public partial class App
    {
        private static readonly string AppVersion = $"v{Assembly.GetExecutingAssembly().GetName().Version}";
        
        private readonly UnityContainer container = new UnityContainer();

        public App()
        {
            var arguments = Environment.GetCommandLineArgs();
            InitializeLogging();
            
            Log.Instance.Debug($"[App..ctor] Arguments: {arguments.DumpToText()}");
            Log.Instance.Debug($"[App..ctor] Culture: {Thread.CurrentThread.CurrentCulture}, UICulture: {Thread.CurrentThread.CurrentUICulture}");
                
            RxApp.SupportsRangeNotifications = false; //FIXME DynamicData (as of v4.11) does not support RangeNotifications

            container.AddExtension(new UiContainerExtensions());
            container.AddExtension(new SharedContainerExtensions());
        }
        
        private void InitializeLogging()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            Application.Current.Dispatcher.UnhandledException += DispatcherOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
            
            RxApp.DefaultExceptionHandler = Log.ErrorsSubject;
            Log.InitializeLogging("Debug");
            Log.SwitchLoggingLevel(Level.Debug);
        }
        
        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ReportCrash(e.ExceptionObject as Exception, "CurrentDomainUnhandledException");
        }
        
        private void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ReportCrash(e.Exception, "DispatcherUnhandledException");
        }
        
        private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            ReportCrash(e.Exception, "TaskSchedulerUnobservedTaskException");
        }
        
        private void ReportCrash(Exception exception, string developerMessage = "")
        {
            Log.Instance.Error($"Unhandled application exception({developerMessage})", exception);

            AppDomain.CurrentDomain.UnhandledException -= CurrentDomainOnUnhandledException;
            Application.Current.Dispatcher.UnhandledException -= DispatcherOnUnhandledException;
            TaskScheduler.UnobservedTaskException -= TaskSchedulerOnUnobservedTaskException;
        }
    }
}