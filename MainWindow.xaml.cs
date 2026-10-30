using System;
using System.Windows;
using System.Diagnostics;
using System.Threading.Tasks;
using CmlLib.Core;
using CmlLib.Core.Auth;

namespace NebulaLauncher
{
    public partial class MainWindow : Window
    {
        private CMLauncher _launcher;
        private MinecraftPath _path;

        public MainWindow()
        {
            InitializeComponent();
            StatusText.Text = "Готов к запуску";
            
            VersionBox.Items.Add("1.20.1");
            VersionBox.Items.Add("1.20.4");
            VersionBox.Items.Add("1.19.4");
            VersionBox.Items.Add("1.16.5");
            VersionBox.Items.Add("1.12.2");
            VersionBox.Items.Add("1.8.9");
            VersionBox.SelectedIndex = 0;
        }

        private async void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            var nick = NickBox.Text.Trim();
            if (string.IsNullOrEmpty(nick)) { StatusText.Text = "Введи ник!"; return; }
            var version = VersionBox.SelectedItem?.ToString();
            if (version == null) { StatusText.Text = "Выбери версию!"; return; }

            try
            {
                PlayBtn.IsEnabled = false;
                StatusText.Text = "Подключение...";
                DownloadBar.Visibility = Visibility.Visible;
                DownloadBar.Value = 0;

                if (_launcher == null)
                {
                    _path = new MinecraftPath();
                    // Используем встроенный в библиотеку загрузчик
                    _launcher = new CMLauncher(_path);

                    _launcher.FileChanged += (args) =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            StatusText.Text = $"Качаем: {args.FileName}";
                        });
                    };

                    _launcher.ProgressChanged += (s, args) =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            DownloadBar.Value = args.ProgressPercentage;
                        });
                    };
                }

                StatusText.Text = "Скачивание...";
                var session = MSession.CreateOfflineSession(nick);
                
                var option = new MLaunchOption
                {
                    Session = session,
                    MaximumRamMb = 2048,
                };

                // Пытаемся скачать и запустить с таймаутом
                var downloadTask = _launcher.CreateProcessAsync(version, option);
                
                // Ждем максимум 1 минуту на подключение
                if (await Task.WhenAny(downloadTask, Task.Delay(60000)) == downloadTask)
                {
                    var process = await downloadTask;
                    StatusText.Text = "Запуск игры...";
                    process.Start();
                    StatusText.Text = "Игра запущена!";
                    DownloadBar.Visibility = Visibility.Collapsed;
                }
                else
                {
                    // Если прошла минута, а скачивание не началось
                    StatusText.Text = "ОШИБКА СЕТИ";
                    DownloadBar.Visibility = Visibility.Collapsed;
                    MessageBox.Show("Не удалось подключиться к серверам Mojang.\n\nПопробуй включить VPN или отключить блокировщики (zapret).", "Ошибка сети");
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = "ОШИБКА";
                DownloadBar.Visibility = Visibility.Collapsed;
                MessageBox.Show("Ошибка:\n\n" + ex.ToString(), "Ошибка");
            }
            finally
            {
                PlayBtn.IsEnabled = true;
            }
        }

        private void ModsBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://modrinth.com/mods",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось открыть сайт: " + ex.Message);
            }
        }
    }
}
