using CollegeManager.DataBase;
using CollegeManager.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CollegeManager.Pages
{
    public partial class CourseDetailsPage : Page
    {
        // Используем контекст вашей новой базы данных
        CollegeManagerEntities db = CollegeManagerEntities.GetContext();
        private int courseId;
        private Courses currentCourse;

        public CourseDetailsPage(int id)
        {
            InitializeComponent();
            courseId = id;
            LoadCourse();
        }

        /// Загрузка данных курса
        void LoadCourse()
        {
            try
            {
                currentCourse = db.Courses.FirstOrDefault(x => x.CourseID == courseId);

                if (currentCourse == null)
                {
                    MessageBox.Show("Курс не найден!",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    NavigationService.GoBack();
                    return;
                }

                // Привязка данных к элементам интерфейса
                CourseTitle.Text = $"Курс: {currentCourse.CourseName}";
                NameText.Text = currentCourse.CourseName;
                DescriptionText.Text = currentCourse.Description;
                ControlFormText.Text = currentCourse.ControlForm; // Замена Priority
                StatusText.Text = currentCourse.Statuses?.StatusName ?? "Не указан";

                StartDateText.Text = currentCourse.StartDate.ToShortDateString();
                DeadlineText.Text = currentCourse.Deadline.ToShortDateString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных курса: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (currentCourse == null) return;

            if (!CurrentUser.CanManageEducationalProjects) // Проверка прав для курсов
            {
                MessageBox.Show("У вас нет прав для редактирования курсов!",
                    "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            NavigationService.Navigate(new EditCoursePage(courseId));
        }

        private void OpenModules_Click(object sender, RoutedEventArgs e)
        {
            if (currentCourse == null) return;
            NavigationService.Navigate(new CourseModulesPage(courseId));
        }

        private void OpenAssignments_Click(object sender, RoutedEventArgs e)
        {
            if (currentCourse == null) return;
            NavigationService.Navigate(new AssignmentsPage(courseId));
        }
    }
}