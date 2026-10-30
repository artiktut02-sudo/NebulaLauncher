using System;
using System.Windows;
using System.Diagnostics;
using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.Version;

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

            // Временно вписываем версии вручную, чтобы лаунчер не зависал при старте
            VersionBox.Items.Add("1.20.1");
            VersionBox.Items.Add("1.20.4");
            VersionBox.Items.Add("1.19.4");
            VersionBox.Items.Add("1.16.5");
            VersionBox.Items.Add("1.12.2");
            VersionBox.Items.Add("1.8.9");
            VersionBox.Items.Add("1.21.4");
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
                StatusText.Text = "Подключение к серверам...";
                DownloadBar.Visibility = Visibility.Visible;
                DownloadBar.Value = 0;

                // Инициализируем библиотеку ТОЛЬКО сейчас, при нажатии на кнопку
                if (_launcher == null)
                {
                    _path = new MinecraftPath();
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
                var session = MSession.GetOfflineSession(nick);
                var option = new MLaunchOption
                {
                    Session = session,
                    MaximumRamMb = 2048,
                };

                // Запускаем скачивание и игру
                var process = await _launcher.CreateProcessAsync(version, option);
                
                StatusText.Text = "Запуск игры...";
                process.Start();
                
                StatusText.Text = "Игра запущена!";
                DownloadBar.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                StatusText.Text = "ОШИБКА";
                DownloadBar.Visibility = Visibility.Collapsed;
                MessageBox.Show("Ошибка при скачивании/запуске:\n\n" + ex.ToString(), "Ошибка");
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
