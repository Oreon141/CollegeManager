using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using CollegeManager.DataBase;

namespace CollegeManager.Pages
{
    public partial class AddEditModulePage : Page
    {
        private CollegeManagerEntities db = CollegeManagerEntities.GetContext();
        private StudyModules _module;
        private int _courseId;
        private bool _isEdit = false;

        // Конструктор для добавления нового модуля
        public AddEditModulePage(int courseId)
        {
            InitializeComponent();
            _courseId = courseId;
            _isEdit = false;
        }

        // Конструктор для редактирования существующего модуля
        public AddEditModulePage(StudyModules module)
        {
            InitializeComponent();
            _module = module;
            _courseId = module.CourseID;
            _isEdit = true;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            StatusBox.ItemsSource = db.Statuses.ToList();

            if (_isEdit)
            {
                HeaderText.Text = "Редактирование модуля";
                ModuleNumberBox.Text = _module.ModuleNumber.ToString();
                ModuleNameBox.Text = _module.ModuleName;
                ModuleDescriptionBox.Text = _module.ModuleDescription;

                PlannedStartBox.SelectedDate = _module.PlannedStartDate;
                PlannedEndBox.SelectedDate = _module.PlannedEndDate;
                ActualStartBox.SelectedDate = _module.ActualStartDate;
                ActualEndBox.SelectedDate = _module.ActualEndDate;

                StatusBox.SelectedItem = db.Statuses.FirstOrDefault(s => s.StatusID == _module.StatusID);
                DeleteButton.Visibility = Visibility.Visible;
            }
            else
            {
                HeaderText.Text = "Создание модуля";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация полей
            if (!int.TryParse(ModuleNumberBox.Text, out int num))
            {
                MessageBox.Show("Номер модуля должен быть числом!");
                return;
            }
            if (string.IsNullOrWhiteSpace(ModuleNameBox.Text))
            {
                MessageBox.Show("Введите название!");
                return;
            }

            // Исправленная проверка статуса для совместимости со старыми версиями C#
            var selectedStatus = StatusBox.SelectedItem as Statuses;
            if (selectedStatus == null)
            {
                MessageBox.Show("Выберите статус!");
                return;
            }

            // Инициализация объекта, если создаем новый
            if (!_isEdit)
                _module = new StudyModules { CourseID = _courseId };

            // Присвоение значений
            _module.ModuleNumber = num;
            _module.ModuleName = ModuleNameBox.Text;
            _module.ModuleDescription = ModuleDescriptionBox.Text;
            _module.PlannedStartDate = PlannedStartBox.SelectedDate;
            _module.PlannedEndDate = PlannedEndBox.SelectedDate;
            _module.ActualStartDate = ActualStartBox.SelectedDate;
            _module.ActualEndDate = ActualEndBox.SelectedDate;
            _module.StatusID = selectedStatus.StatusID;

            try
            {
                if (!_isEdit)
                    db.StudyModules.Add(_module);

                db.SaveChanges();
                MessageBox.Show("Сохранено!");
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isEdit) return;

            if (MessageBox.Show("Удалить модуль?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    db.StudyModules.Remove(_module);
                    db.SaveChanges();
                    MessageBox.Show("Модуль удалён!");
                    NavigationService.GoBack();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}");
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }
    }
}