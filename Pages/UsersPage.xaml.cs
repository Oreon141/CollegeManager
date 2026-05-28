using CollegeManager.DataBase;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CollegeManager.Pages
{
    public partial class UsersPage : Page
    {
        CollegeManagerEntities db = CollegeManagerEntities.GetContext();

        Users selectedUser;

        public UsersPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadRoles();
            LoadUsers();
            UpdateButtons();
        }

        private void LoadRoles()
        {
            RoleFilter.ItemsSource = db.UserRoles.ToList();
            RoleBox.ItemsSource = db.UserRoles.ToList();
        }

        private void LoadUsers()
        {
            var users = db.Users.ToList();

            var selectedRole = RoleFilter.SelectedItem as UserRoles;

            if (selectedRole != null)
            {
                users = users
                    .Where(x => x.RoleID == selectedRole.RoleID)
                    .ToList();
            }

            UsersGrid.ItemsSource = users;
        }

        private void RoleFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadUsers();
        }

        private void ResetSelection_Click(object sender, RoutedEventArgs e)
        {
            UsersGrid.SelectedItem = null;
            RoleFilter.SelectedItem = null;

            selectedUser = null;

            ClearForm();

            LoadUsers();

            UpdateButtons();
        }

        private void UsersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedUser = UsersGrid.SelectedItem as Users;

            if (selectedUser == null)
            {
                ClearForm();
                UpdateButtons();
                return;
            }

            SurnameBox.Text = selectedUser.Surname;
            FirstNameBox.Text = selectedUser.FirstName;
            LastNameBox.Text = selectedUser.LastName;
            EmailBox.Text = selectedUser.Email;
            LoginBox.Text = selectedUser.Login;

            PasswordBox.Password = "";

            RoleBox.SelectedItem = db.UserRoles
                .FirstOrDefault(x => x.RoleID == selectedUser.RoleID);

            UpdateButtons();
        }

        private void CreateUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var role = RoleBox.SelectedItem as UserRoles;

                if (role == null)
                {
                    MessageBox.Show("Выберите роль");
                    return;
                }

                Users user = new Users()
                {
                    Surname = SurnameBox.Text,
                    FirstName = FirstNameBox.Text,
                    LastName = LastNameBox.Text,
                    Email = EmailBox.Text,
                    Login = LoginBox.Text,
                    Password = PasswordBox.Password,
                    RoleID = role.RoleID
                };

                db.Users.Add(user);

                db.SaveChanges();

                MessageBox.Show("Пользователь создан");

                LoadUsers();

                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SaveUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedUser == null)
                {
                    MessageBox.Show("Выберите пользователя");
                    return;
                }

                var user = db.Users
                    .FirstOrDefault(x => x.UserID == selectedUser.UserID);

                if (user == null)
                    return;

                var role = RoleBox.SelectedItem as UserRoles;

                user.Surname = SurnameBox.Text;
                user.FirstName = FirstNameBox.Text;
                user.LastName = LastNameBox.Text;
                user.Email = EmailBox.Text;
                user.Login = LoginBox.Text;

                if (!string.IsNullOrWhiteSpace(PasswordBox.Password))
                {
                    user.Password = PasswordBox.Password;
                }

                if (role != null)
                {
                    user.RoleID = role.RoleID;
                }

                db.SaveChanges();

                MessageBox.Show("Данные сохранены");

                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedUser == null)
                {
                    MessageBox.Show("Выберите пользователя");
                    return;
                }

                var result = MessageBox.Show(
                    "Удалить пользователя?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                var user = db.Users
                    .FirstOrDefault(x => x.UserID == selectedUser.UserID);

                if (user == null)
                    return;

                db.Users.Remove(user);

                db.SaveChanges();

                MessageBox.Show("Пользователь удалён");

                LoadUsers();

                ClearForm();

                selectedUser = null;

                UpdateButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ClearForm()
        {
            SurnameBox.Text = "";
            FirstNameBox.Text = "";
            LastNameBox.Text = "";
            EmailBox.Text = "";
            LoginBox.Text = "";
            PasswordBox.Password = "";

            RoleBox.SelectedItem = null;
        }

        private void UpdateButtons()
        {
            if (selectedUser == null)
            {
                CreateButton.IsEnabled = true;

                SaveButton.IsEnabled = false;
                DeleteButton.IsEnabled = false;
            }
            else
            {
                CreateButton.IsEnabled = false;

                SaveButton.IsEnabled = true;
                DeleteButton.IsEnabled = true;
            }
        }
    }
}