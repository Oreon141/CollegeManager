using CollegeManager.DataBase;
using CollegeManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CollegeManager.Pages
{
    public partial class CoursesPage : Page
    {
        CollegeManagerEntities db = CollegeManagerEntities.GetContext();
        List<Courses> allCourses;

        // Флаг для предотвращения множественного вызова UpdateGrid при сбросе
        private bool _isResetting = false;

        public CoursesPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                allCourses = db.Courses.ToList();

                StatusFilter.ItemsSource = db.Statuses.ToList();
                StatusFilter.DisplayMemberPath = "StatusName";
                StatusFilter.SelectedValuePath = "StatusID";

                // Подгружаем формы контроля (Экзамен, Зачет и т.д.) вместо Priority
                var controlForms = allCourses
                    .Select(c => c.ControlForm)
                    .Distinct()
                    .Where(c => !string.IsNullOrEmpty(c))
                    .ToList();

                ControlFormFilter.Items.Clear();
                ControlFormFilter.Items.Add("Все");
                foreach (var form in controlForms)
                {
                    ControlFormFilter.Items.Add(form);
                }
                ControlFormFilter.SelectedIndex = 0;

                ConfigureActionButtons();
                UpdateGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConfigureActionButtons()
        {
            CreateCourseButton.Visibility = CurrentUser.CanManageEducationalProjects ?
                Visibility.Visible : Visibility.Collapsed;

            if (CurrentUser.CanViewOnly)
            {
                foreach (var column in CoursesGrid.Columns)
                {
                    if (column.Header?.ToString() == "Действия")
                    {
                        column.Visibility = Visibility.Collapsed;
                        break;
                    }
                }
            }
        }

        // Разделенные обработчики (решает ошибку делегатов WPF)
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isResetting) UpdateGrid();
        }

        private void FilterChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isResetting) UpdateGrid();
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            _isResetting = true;

            SearchBox.Text = "";
            StatusFilter.SelectedIndex = -1;
            ControlFormFilter.SelectedIndex = 0;

            _isResetting = false;
            UpdateGrid();
        }

        private void UpdateGrid()
        {
            if (allCourses == null) return;

            var list = allCourses.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                string searchText = SearchBox.Text.ToLower();
                list = list.Where(x => (x.CourseName ?? "").ToLower().Contains(searchText) ||
                                       (x.Description ?? "").ToLower().Contains(searchText));
            }

            if (StatusFilter.SelectedIndex != -1 && StatusFilter.SelectedItem != null)
            {
                int statusId = (int)StatusFilter.SelectedValue;
                list = list.Where(x => x.StatusID == statusId);
            }

            if (ControlFormFilter.SelectedIndex > 0 && ControlFormFilter.SelectedItem != null)
            {
                string selectedForm = ControlFormFilter.SelectedItem.ToString();
                if (selectedForm != "Все")
                {
                    list = list.Where(x => x.ControlForm == selectedForm);
                }
            }

            CoursesGrid.ItemsSource = list.ToList();
        }

        private void CoursesGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Height = double.NaN;
        }

        private void CoursesGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column is DataGridTextColumn textColumn)
            {
                var style = new Style(typeof(TextBlock));
                style.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));
                style.Setters.Add(new Setter(TextBlock.TextTrimmingProperty, TextTrimming.None));
                style.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center));
                style.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center));
                textColumn.ElementStyle = style;
                textColumn.MinWidth = 80;

                if (textColumn.Binding is Binding binding)
                {
                    binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                }
            }
        }

        private void CoursesGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (CoursesGrid.SelectedItem is Courses selectedCourse)
            {
                OpenCourseDetails(selectedCourse.CourseID);
            }
        }

        private void OpenCourseDetails(int courseId)
        {
            
            var courseDetailsPage = new CourseDetailsPage(courseId);
            NavigationService.Navigate(courseDetailsPage);
            
        }

        private void CreateCourse_Click(object sender, RoutedEventArgs e)
        {
            if (!CurrentUser.CanManageEducationalProjects)
            {
                MessageBox.Show("У вас нет прав для создания курсов!",
                    "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var createCoursePage = new CreateCoursePage();
            NavigationService.Navigate(createCoursePage);
            
        }

        private void OpenCourse_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int courseId = Convert.ToInt32(btn.Tag);
                OpenCourseDetails(courseId);
            }
        }

        private void DeleteCourse_Click(object sender, RoutedEventArgs e)
        {
            if (!CurrentUser.CanManageEducationalProjects)
            {
                MessageBox.Show("У вас нет прав для удаления курсов!",
                    "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!(sender is Button btn) || btn.Tag == null) return;
            int courseId = Convert.ToInt32(btn.Tag);

            var course = db.Courses.FirstOrDefault(p => p.CourseID == courseId);
            if (course == null)
            {
                MessageBox.Show("Курс не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Вы уверены, что хотите удалить курс «{course.CourseName}»?\nЭто удалит модули, задания, ресурсы и ведомости.",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                // Правильное каскадное удаление данных для новой БД

                // 1. Комментарии к заданиям
                var assignmentIds = db.Assignments.Where(a => a.CourseID == courseId).Select(a => a.AssignmentID).ToList();
                var comments = db.AssignmentComments.Where(c => assignmentIds.Contains(c.AssignmentID)).ToList();
                if (comments.Any()) db.AssignmentComments.RemoveRange(comments);

                // 2. Задания
                var assignments = db.Assignments.Where(a => a.CourseID == courseId).ToList();
                if (assignments.Any()) db.Assignments.RemoveRange(assignments);

                // 3. Модули
                var modules = db.StudyModules.Where(m => m.CourseID == courseId).ToList();
                if (modules.Any()) db.StudyModules.RemoveRange(modules);

                // 4. Состав участников
                var members = db.CourseMembers.Where(m => m.CourseID == courseId).ToList();
                if (members.Any()) db.CourseMembers.RemoveRange(members);

                // 5. Ресурсы
                var resources = db.CourseResources.Where(r => r.CourseID == courseId).ToList();
                if (resources.Any()) db.CourseResources.RemoveRange(resources);

                // 6. Ведомости
                var sheets = db.AcademicSheets.Where(s => s.CourseID == courseId).ToList();
                if (sheets.Any()) db.AcademicSheets.RemoveRange(sheets);

                // 7. Сам курс
                db.Courses.Remove(course);
                db.SaveChanges();

                allCourses = db.Courses.ToList();
                UpdateGrid();

                MessageBox.Show("Курс успешно удалён.",
                    "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}\n{ex.InnerException?.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}