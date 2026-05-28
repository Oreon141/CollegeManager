using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using CollegeManager.DataBase; // Важно для подключения к БД

namespace CollegeManager.Pages
{
    public partial class AssignmentsPage : Page
    {
        // Использование правильного класса контекста из вашего файла
        private CollegeManagerEntities db = CollegeManagerEntities.GetContext();
        private int _courseId;

        // Пустой конструктор для WPF дизайнера
        public AssignmentsPage()
        {
            InitializeComponent();
        }

        // Основной конструктор
        public AssignmentsPage(int id)
        {
            InitializeComponent();
            _courseId = id;
            LoadPage();
        }

        private void LoadPage()
        {
            try
            {
                StatusFilter.ItemsSource = db.Statuses.ToList();
                LoadAssignments();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
            }
        }

        private void LoadAssignments()
        {
            // Фильтрация по CourseID, так как он определен в вашей модели
            AssignmentsGrid.ItemsSource = db.Assignments
                .Where(a => a.CourseID == _courseId)
                .ToList();
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            try
            {
                var list = db.Assignments.Where(a => a.CourseID == _courseId);

                if (!string.IsNullOrWhiteSpace(SearchBox.Text))
                {
                    string txt = SearchBox.Text.ToLower();
                    list = list.Where(a => a.AssignmentName.ToLower().Contains(txt));
                }

                if (StatusFilter.SelectedItem is Statuses s)
                {
                    list = list.Where(a => a.StatusID == s.StatusID);
                }

                AssignmentsGrid.ItemsSource = list.ToList();
            }
            catch { }
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            StatusFilter.SelectedIndex = -1;
            LoadAssignments();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack) NavigationService.GoBack();
        }

        private void CreateAssignment_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CreateAssignmentPage(_courseId));
        }

        private void OpenAssignment_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.Tag is int id)
            {
                NavigationService.Navigate(new AssignmentDetailsPage(id));
            }
        }

        private void EditAssignment_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.Tag is int id)
            {
                NavigationService.Navigate(new EditAssignmentPage(id));
            }
        }

        private void DeleteAssignment_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.Tag is int id)
            {
                if (MessageBox.Show("Удалить задание?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    var item = db.Assignments.FirstOrDefault(x => x.AssignmentID == id);
                    if (item != null)
                    {
                        db.Assignments.Remove(item);
                        db.SaveChanges();
                        LoadAssignments();
                    }
                }
            }
        }
    }
}