using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using CollegeManager.DataBase; // Убедитесь, что namespace БД верный

namespace CollegeManager.Pages
{
    public partial class CreateCoursePage : Page
    {
        // Использование контекста БД
        CollegeManagerEntities db = CollegeManagerEntities.GetContext();

        public CreateCoursePage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Загружаем статусы в выпадающий список
                StatusBox.ItemsSource = db.Statuses.ToList();
                StatusBox.DisplayMemberPath = "StatusName"; // Указываем свойство для отображения
                StatusBox.SelectedIndex = 0;

                // Устанавливаем даты по умолчанию
                StartDatePicker.SelectedDate = DateTime.Now;
                DeadlinePicker.SelectedDate = DateTime.Now.AddMonths(1);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке: {ex.Message}");
            }
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            // 1. Валидация
            if (string.IsNullOrWhiteSpace(CourseNameBox.Text))
            {
                MessageBox.Show("Введите название курса.");
                CourseNameBox.Focus();
                return;
            }
            if (StatusBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите статус.");
                return;
            }
            if (StartDatePicker.SelectedDate == null || DeadlinePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите корректные даты.");
                return;
            }

            try
            {
                // 2. Создание объекта курса
                Courses newCourse = new Courses
                {
                    CourseName = CourseNameBox.Text.Trim(),
                    Description = DescriptionBox.Text.Trim(),
                    StartDate = StartDatePicker.SelectedDate.Value,
                    Deadline = DeadlinePicker.SelectedDate.Value,
                    ControlForm = ControlFormBox.Text.Trim(),
                    StatusID = (StatusBox.SelectedItem as Statuses).StatusID
                };

                // 3. Сохранение
                db.Courses.Add(newCourse);
                db.SaveChanges();

                MessageBox.Show("Курс успешно создан!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                if (NavigationService.CanGoBack)
                    NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }
    }
}