using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using CollegeManager.DataBase;
// Если у вас есть класс CurrentUser для прав доступа, убедитесь, что он подключен
// using CollegeManager.Models; 

namespace CollegeManager.Pages
{
    public partial class AssignmentDetailsPage : Page
    {
        private CollegeManagerEntities db = CollegeManagerEntities.GetContext();
        private Assignments _assignment;

        public AssignmentDetailsPage(int assignmentId)
        {
            InitializeComponent();
            LoadAssignmentData(assignmentId);
        }

        private void LoadAssignmentData(int assignmentId)
        {
            try
            {
                // Загружаем задание из БД
                _assignment = db.Assignments.FirstOrDefault(a => a.AssignmentID == assignmentId);

                if (_assignment == null)
                {
                    MessageBox.Show("Задание не найдено!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    NavigationService.GoBack();
                    return;
                }

                LoadAssignmentDetails();
                ConfigureEditButton();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
            }
        }

        private void LoadAssignmentDetails()
        {
            // Используем поля из вашей модели Assignments
            TitleText.Text = _assignment.AssignmentName;
            DescriptionText.Text = string.IsNullOrEmpty(_assignment.AssignmentDescription)
                ? "Описание отсутствует"
                : _assignment.AssignmentDescription;

            // Навигационные свойства из модели
            ModuleText.Text = _assignment.StudyModules?.ModuleName ?? "Не указан";
            StatusText.Text = _assignment.Statuses?.StatusName ?? "Не указан";

            // Исполнитель (AssignedTo -> Users1)
            if (_assignment.Users1 != null)
            {
                // Предполагаем, что у модели Users есть поля Surname и FirstName
                UserText.Text = $"{_assignment.Users1.Surname} {_assignment.Users1.FirstName}";
            }
            else
            {
                UserText.Text = "Не назначен";
            }
        }

        private void ConfigureEditButton()
        {
            // Здесь должна быть ваша проверка прав. 
            // Если используете класс CurrentUser, как в примере:
            // bool canEdit = CurrentUser.CanManageTasks;
            bool canEdit = true; // Замените на вашу логику прав доступа

            EditButton.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (_assignment == null) return;

            NavigationService.Navigate(new EditAssignmentPage(_assignment.AssignmentID));
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }
    }
}