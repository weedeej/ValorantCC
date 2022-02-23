using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace ValorantCC
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            //Copy pasta for logging :V
            Startup += new StartupEventHandler(AppEventHandler);

            DispatcherUnhandledException += LogDispatcherUnhandled;

            TaskScheduler.UnobservedTaskException += LogUnobservedTaskException;
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
        }

        private void AppEventHandler(object sender, StartupEventArgs e)
        {
            AppDomain.CurrentDomain.FirstChanceException += LogFirstChanceException;
            AppDomain.CurrentDomain.UnhandledException += LogUnhandled;
        }

        private void LogFirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            Utilities.Utils.Log($"{e.Exception.Message}: {e.Exception.StackTrace}");
        }

        private void LogDispatcherUnhandled(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Utilities.Utils.Log($"{e.Exception.Message}: {e.Exception.StackTrace}");
            e.Handled = false;
        }

        private void LogUnhandled(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            if (e.IsTerminating)
            {
                Utilities.Utils.Log($"{ex.Message}: {ex.StackTrace}");
            }
        }

        private void LogUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Utilities.Utils.Log($"{e.Exception.Message}: {e.Exception.StackTrace}");
            e.SetObserved();
        }
    }
}
