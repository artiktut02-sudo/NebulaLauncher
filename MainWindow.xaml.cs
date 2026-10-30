using System;
using System.Windows;
using System.Diagnostics;
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
            _path = new MinecraftPath();
            _launcher = new CMLauncher(_path);
            
            // Подписка на события скачивания
            _launcher.FileChanged += (e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    DownloadBar.Visibility = Visibility.Visible;
                    StatusText.Text = $"Качаем: {e.FileName}";
                });
            };

            _launcher.ProgressChanged += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    DownloadBar.Value = e.ProgressPercentage;
                });
            };

            LoadVersions();
        }

        private async void LoadVersions()
        {
            try
            {
                StatusText.Text = "Загрузка версий...";
                var versions = await _launcher.GetAllVersionsAsync();
                VersionBox.Items.Clear();
                foreach (var v in versions)
                    VersionBox.Items.Add(v.Name);
                if (VersionBox.Items.Count > 0)
                    VersionBox.SelectedIndex = 0;
                StatusText.Text = "Готов к запуску";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Ошибка версий";
                MessageBox.Show("Не удалось загрузить список версий:\n" + ex.Message);
            }
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
                StatusText.Text = "Подготовка...";
                DownloadBar.Visibility = Visibility.Visible;
                DownloadBar.Value = 0;

                var session = MSession.GetOfflineSession(nick);
                var option = new MLaunchOption
                {
                    Session = session,
                    MaximumRamMb = 2048, // 2ГБ памяти
                };

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
