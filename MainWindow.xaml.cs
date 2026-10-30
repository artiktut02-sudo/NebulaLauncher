using System;
using System.Windows;

namespace NebulaLauncher
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            StatusText.Text = "Программа успешно запустилась!";
        }

        // Заглушки, чтобы кнопки не выдавали ошибку при нажатии
        private void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Кнопка Играть работает!");
        }

        private void ModsBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Кнопка Моды работает!");
        }
    }
}
