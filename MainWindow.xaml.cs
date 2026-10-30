using System;
using System.Windows;
using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.ProcessBuilder; // Необходимо для новых событий

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
            
            // Подписываемся на события для прогресс-бара
            _launcher.FileChanged += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    DownloadBar.Visibility = Visibility.Visible;
                    StatusText.Text = $"Скачивание: {e.FileName}";
                    if (e.TotalTasks > 0)
                    {
                        DownloadBar.Value = (double)e.ProgressedTasks / e.TotalTasks * 100;
                    }
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
                    MaximumRamMb = 2048, // 2 ГБ ОЗУ
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
