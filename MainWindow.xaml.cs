using System;
using System.Windows;
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
            
            // Событие изменения файла (показывает имя файла)
            _launcher.FileChanged += (e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    DownloadBar.Visibility = Visibility.Visible;
                    StatusText.Text = $"Скачивание: {e.FileName}";
                });
            };

            // Событие прогресса (показывает проценты)
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
                StatusText.Text = "Загрузка списка версий...";
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
                StatusText.Text = "Ошибка: " + ex.Message;
            }
        }

        private async void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            var nick = NickBox.Text.Trim();
            if (string.IsNullOrEmpty(nick)) { StatusText.Text = "Введи ник"; return; }
            var version = VersionBox.SelectedItem?.ToString();
            if (version == null) { StatusText.Text = "Выбери версию"; return; }

            try
            {
                PlayBtn.IsEnabled = false;
                StatusText.Text = "Подготовка...";

                var session = MSession.GetOfflineSession(nick);
                var option = new MLaunchOption
                {
                    Session = session,
                    MaximumRamMb = 2048,
                };

                var process = await _launcher.CreateProcessAsync(version, option);
                process.Start();
                
                StatusText.Text = "Игра запущена!";
                DownloadBar.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                StatusText.Text = "Ошибка: " + ex.Message;
                MessageBox.Show(ex.ToString(), "Ошибка запуска");
            }
            finally
            {
                PlayBtn.IsEnabled = true;
            }
        }
    }
}
