using System.Diagnostics;
using System.Windows;
using Microsoft.Win32;

namespace BoosterBot
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // 检测 VC++ 运行库
            if (!IsVCRedistInstalled())
            {
                ShowErrorAndExit("需要安装 VC++ 2015-2022 运行库！", "https://aka.ms/vs/17/release/vc_redist.x64.exe");
                return;
            }

            base.OnStartup(e);
        }

        // VC++ 2015-2022 运行库检测
        private bool IsVCRedistInstalled()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64");
                if (key == null) return false;
                var installed = key.GetValue("Installed");
                var majorVersion = key.GetValue("Major");
                return installed != null && (int)installed == 1 && majorVersion != null && (int)majorVersion >= 14;
            }
            catch
            {
                return false;
            }
        }

        // 统一错误处理
        private void ShowErrorAndExit(string message, string downloadUrl)
        {
            MessageBox.Show(message + "\n点击确定打开下载页面。", "缺少依赖", MessageBoxButton.OK, MessageBoxImage.Error);
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = downloadUrl,
                    UseShellExecute = true
                });
            }
            catch { }
            Shutdown();
        }
    }
}