using CollegeManager.DataBase;
using CollegeManager.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CollegeManager.Pages
{
    public partial class EditCoursePage : Page
    {
        // Контекст базы данных
        CollegeManagerEntities db = CollegeManagerEntities.GetContext();
        Courses currentCourse;

        public EditCoursePage(int courseId)
        {
            InitializeComponent();
            LoadFields(courseId);
        }

        /// <summary>
        /// Загрузка данных курса в поля формы
        /// </summary>
        void LoadFields(int id)
        {
            try
            {
                currentCourse = db.Courses.FirstOrDefault(c => c.CourseID == id);

                if (currentCourse == null)
                {
                    MessageBox.Show("Курс не найден!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    NavigationService.GoBack();
                    return;
                }

                CourseNameBox.Text = currentCourse.CourseName;
                DescriptionBox.Text = currentCourse.Description;
                DeadlinePicker.SelectedDate = currentCourse.Deadline;

                // Заполнение выпадающего списка форм контроля
                ControlFormBox.ItemsSource = new string[] { "Зачет", "Диф. зачет", "Экзамен", "Курсовая работа" };
                ControlFormBox.SelectedItem = currentCourse.ControlForm;

                // Заполнение статусов
                var statuses = db.Statuses.ToList();
                StatusBox.ItemsSource = statuses;
                StatusBox.DisplayMemberPath = "StatusName";
                StatusBox.SelectedItem = currentCourse.Statuses;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Обработчик сохранения изменений
        /// </summary>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Валидация полей
            if (string.IsNullOrWhiteSpace(CourseNameBox.Text))
            {
                MessageBox.Show("Введите название курса.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                CourseNameBox.Focus();
                return;
            }

            if (DeadlinePicker.SelectedDate == null)
            {
                MessageBox.Show("Укажите дедлайн.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                DeadlinePicker.Focus();
                return;
            }

            // Исправление для C# 7.3: используем 'as' вместо 'is not'
            var selectedStatus = StatusBox.SelectedItem as Statuses;
            if (selectedStatus == null)
            {
                MessageBox.Show("Выберите статус.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                StatusBox.Focus();
                return;
            }

            // Проверка на уникальность названия (исключая текущий редактируемый курс)
            if (db.Courses.Any(c => c.CourseName == CourseNameBox.Text.Trim() && c.CourseID != currentCourse.CourseID))
            {
                MessageBox.Show("Курс с таким названием уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                CourseNameBox.Focus();
                return;
            }

            try
            {
                string oldStatusName = currentCourse.Statuses?.StatusName ?? "";

                // Обновление данных объекта
                currentCourse.CourseName = CourseNameBox.Text.Trim();
                currentCourse.Description = DescriptionBox.Text.Trim();
                currentCourse.ControlForm = ControlFormBox.SelectedItem?.ToString();
                currentCourse.Deadline = DeadlinePicker.SelectedDate.Value;
                currentCourse.StatusID = selectedStatus.StatusID;

                db.SaveChanges();

                // Логика уведомлений при смене статуса
                if (oldStatusName != selectedStatus.StatusName)
                {
                    var courseMembers = db.CourseMembers
                        .Where(cm => cm.CourseID == currentCourse.CourseID)
                        .ToList();

                    foreach (var member in courseMembers)
                    {
                        // Не уведомляем того, кто внес изменения
                        if (member.UserID == CurrentUser.UserID) continue;

                        db.Notifications.Add(new Notifications()
                        {
                            UserID = member.UserID,
                            Title = "Изменение статуса курса",
                            Message = $"Курс '{currentCourse.CourseName}' сменил статус: {oldStatusName} → {selectedStatus.StatusName}",
                            Type = "CourseStatus",
                            CreatedAt = DateTime.Now,
                            IsRead = false,
                            RelatedEntityID = currentCourse.CourseID
                        });
                    }
                    db.SaveChanges();
                }

                MessageBox.Show("Изменения успешно сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.Navigate(new CourseDetailsPage(currentCourse.CourseID));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (currentCourse != null)
            {
                NavigationService.Navigate(new CourseDetailsPage(currentCourse.CourseID));
            }
            else
            {
                NavigationService.GoBack();
            }
        }
    }
}