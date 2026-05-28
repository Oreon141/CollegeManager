using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CollegeManager.DataBase;
using CollegeManager.Models;

namespace CollegeManager.Windows
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            Loaded += LoginWindow_Loaded;
        }

        /// Обработчик загрузки окна
        private void LoginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoginBox.Focus();
        }

        /// Обработчик изменения текста в поле пароля
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholder.Visibility =
                string.IsNullOrEmpty(PasswordBox.Password)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        /// Обработчик нажатия кнопки "Войти"
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LoginBox.Text))
            {
                MessageBox.Show("Введите логин!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                LoginBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Введите пароль!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                PasswordBox.Focus();
                return;
            }

            try
            {
                var db = CollegeManagerEntities.GetContext();

                // Загружаем пользователя и его роль из новой БД
                var user = db.Users
                    .Include("UserRoles")
                    .FirstOrDefault(u =>
                        u.Login == LoginBox.Text &&
                        u.Password == PasswordBox.Password);

                if (user == null)
                {
                    MessageBox.Show("Неверный логин или пароль!",
                        "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);

                    PasswordBox.Password = "";
                    PasswordBox.Focus();
                    return;
                }

                // Инициализируем сессию пользователя
                CurrentUser.Initialize(
                    user.UserID,
                    user.Login,
                    user.UserRoles?.RoleName ?? "Пользователь",
                    user.RoleID
                );

                MessageBox.Show($"Добро пожаловать, {user.FirstName} {user.Surname}!\nВаша роль: {CurrentUser.Role}",
                    "Успешный вход", MessageBoxButton.OK, MessageBoxImage.Information);

                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при авторизации: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// Обработчик изменения текста в поле логина
        private void LoginBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoginPlaceHolder.Visibility =
                string.IsNullOrEmpty(LoginBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        /// Обработчик нажатия клавиши Enter в поле логина
        private void LoginBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PasswordBox.Focus();
                e.Handled = true;
            }
        }

        /// Обработчик нажатия клавиши Enter в поле пароля
        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Login_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
        }
    }
}