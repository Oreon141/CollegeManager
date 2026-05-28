using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using CollegeManager.DataBase;

namespace CollegeManager.Pages
{
    public partial class EditAssignmentPage : Page
    {
        private CollegeManagerEntities db = CollegeManagerEntities.GetContext();
        private Assignments _assignment;

        public EditAssignmentPage(int assignmentId)
        {
            InitializeComponent();
            LoadAssignment(assignmentId);
        }

        private void LoadAssignment(int id)
        {
            try
            {
                _assignment = db.Assignments.FirstOrDefault(a => a.AssignmentID == id);

                if (_assignment == null)
                {
                    MessageBox.Show("Задание не найдено!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    NavigationService.GoBack();
                    return;
                }

                // Заполнение полей
                AssignmentNameBox.Text = _assignment.AssignmentName;
                DescriptionBox.Text = _assignment.AssignmentDescription;
                WeightBox.Text = _assignment.Weight;

                // Статус
                StatusBox.ItemsSource = db.Statuses.ToList();
                StatusBox.SelectedItem = _assignment.Statuses;

                // Пользователь (Исполнитель)
                UserBox.ItemsSource = db.Users.ToList();
                // Используем Users1, так как это навигационное свойство для AssignedTo
                UserBox.SelectedItem = _assignment.Users1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке: {ex.Message}");
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(AssignmentNameBox.Text))
            {
                MessageBox.Show("Введите название!");
                return;
            }

            try
            {
                _assignment.AssignmentName = AssignmentNameBox.Text.Trim();
                _assignment.AssignmentDescription = DescriptionBox.Text.Trim();
                _assignment.Weight = WeightBox.Text.Trim();

                // Обновление статуса
                var selectedStatus = StatusBox.SelectedItem as Statuses;
                if (selectedStatus != null)
                    _assignment.StatusID = selectedStatus.StatusID;

                // Обновление пользователя
                var selectedUser = UserBox.SelectedItem as Users;
                _assignment.AssignedTo = selectedUser?.UserID;

                db.SaveChanges();
                MessageBox.Show("Изменения сохранены!");
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }
    }
}