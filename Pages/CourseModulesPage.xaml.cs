using CollegeManager.DataBase;
using CollegeManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CollegeManager.Pages
{
    public partial class CourseModulesPage : Page
    {
        private readonly CollegeManagerEntities db = CollegeManagerEntities.GetContext();
        private int _courseId;
        private List<StudyModules> allModules; // Убедитесь, что имя класса модели StudyModules

        public CourseModulesPage(int courseId)
        {
            InitializeComponent();
            _courseId = courseId;
            ConfigureActionButtons();
        }

        private void ConfigureActionButtons()
        {
            // Проверка прав (используйте ваше свойство прав доступа)
            CreateModuleBtn.Visibility = CurrentUser.CanManageEducationalProjects ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusViewFilter.ItemsSource = db.Statuses.ToList();
                StatusViewFilter.DisplayMemberPath = "StatusName";

                allModules = db.StudyModules
                              .Where(m => m.CourseID == _courseId)
                              .OrderBy(m => m.ModuleNumber)
                              .ToList();

                LoadModules();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadModules()
        {
            var query = allModules.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                string text = SearchBox.Text.ToLower();
                query = query.Where(m => m.ModuleName.ToLower().Contains(text) || m.ModuleDescription.ToLower().Contains(text));
            }

            if (StatusViewFilter.SelectedItem is Statuses selectedStatus)
            {
                query = query.Where(m => m.StatusID == selectedStatus.StatusID);
            }

            ModulesGrid.ItemsSource = query.ToList();
        }

        private void FilterChanged(object sender, RoutedEventArgs e) => LoadModules();

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            StatusViewFilter.SelectedIndex = -1;
            LoadModules();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) => NavigationService.GoBack();

        private void AddModule_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddEditModulePage(_courseId));
        }

        private void ModulesGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ModulesGrid.SelectedItem is StudyModules selectedModule)
            {
                NavigationService.Navigate(new AddEditModulePage(selectedModule));
            }
        }

        private void OpenModule_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var module = allModules.FirstOrDefault(m => m.ModuleID == id);
                if (module != null)
                {
                    NavigationService.Navigate(new AddEditModulePage(module));
                }
            }
        }

        private void DeleteModule_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var module = db.StudyModules.FirstOrDefault(m => m.ModuleID == id);
                if (module == null) return;

                if (MessageBox.Show("Удалить модуль?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    db.StudyModules.Remove(module);
                    db.SaveChanges();

                    // Обновляем локальный список
                    allModules = db.StudyModules.Where(m => m.CourseID == _courseId).ToList();
                    LoadModules();
                }
            }
        }
    }
}