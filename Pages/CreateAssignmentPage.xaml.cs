using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using CollegeManager.DataBase;

namespace CollegeManager.Pages
{
    public partial class CreateAssignmentPage : Page
    {
        CollegeManagerEntities db = CollegeManagerEntities.GetContext();
        int moduleId; // Предполагаю, что задание привязано к модулю

        public CreateAssignmentPage(int id)
        {
            InitializeComponent();
            moduleId = id;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Загрузка статусов
                StatusBox.ItemsSource = db.Statuses.ToList();
                StatusBox.DisplayMemberPath = "StatusName";
                if (StatusBox.Items.Count > 0) StatusBox.SelectedIndex = 0;

                // Загрузка пользователей/студентов
                UserBox.ItemsSource = db.Users.ToList();
                UserBox.DisplayMemberPath = "Login"; // Уточните поле (может быть FullName)
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке: {ex.Message}");
            }
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(AssignmentNameBox.Text))
            {
                MessageBox.Show("Введите название задания.");
                return;
            }

            var selectedStatus = StatusBox.SelectedItem as Statuses;
            if (selectedStatus == null)
            {
                MessageBox.Show("Выберите статус.");
                return;
            }

            var selectedUser = UserBox.SelectedItem as Users;
            if (selectedUser == null)
            {
                MessageBox.Show("Выберите исполнителя.");
                return;
            }

            try
            {
                // Создание записи в БД
                Assignments newAssignment = new Assignments
                {
                    AssignmentName = AssignmentNameBox.Text.Trim(),
                    // Исправлено: свойство называется AssignmentDescription
                    AssignmentDescription = DescriptionBox.Text.Trim(),
                    ModuleID = moduleId,
                    StatusID = selectedStatus.StatusID,
                    // Исправлено: свойство называется AssignedTo, 
                    // а значение берем из ID выбранного пользователя
                    AssignedTo = selectedUser.UserID
                };

                db.Assignments.Add(newAssignment);
                db.SaveChanges();

                MessageBox.Show("Задание успешно создано!");
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании: {ex.Message}");
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }
    }
}