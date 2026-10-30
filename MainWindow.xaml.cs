using System;
using System.Windows;
using System.Diagnostics;
using System.IO;
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
            // Сначала инициализируем интерфейс, чтобы окно сразу появилось
            InitializeComponent();
            
            // Теперь запускаем инициализацию в фоне
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Записываем лог при старте
                File.WriteAllText("launcher_log.txt", "Запуск лаунчера...\n");

                _path = new MinecraftPath();
                _launcher = new CMLauncher(_path);
                
                _launcher.FileChanged += (args) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        DownloadBar.Visibility = Visibility.Visible;
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

                LoadVersions();
            }
            catch (Exception ex)
            {
                File.AppendAllText("launcher_log.txt", "Ошибка при инициализации: " + ex.ToString() + "\n");
                MessageBox.Show("Ошибка при запуске:\n" + ex.Message);
            }
        }

        private async void LoadVersions()
        {
            try
            {
                StatusText.Text = "Загрузка версий...";
                
                // Добавляем таймаут, чтобы не висело вечно
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
                File.AppendAllText("launcher_log.txt", "Ошибка загрузки версий: " + ex.ToString() + "\n");
                MessageBox.Show("Не удалось загрузить список версий. Проверьте интернет.\n\n" + ex.Message);
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
                    MaximumRamMb = 2048,
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
                File.AppendAllText("launcher_log.txt", "Ошибка при запуске игры: " + ex.ToString() + "\n");
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
