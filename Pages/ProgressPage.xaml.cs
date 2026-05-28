using CollegeManager.DataBase;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CollegeManager.Pages
{
    public partial class ProgressPage : Page
    {
        CollegeManagerEntities db = CollegeManagerEntities.GetContext();

        private int currentChart = 0;

        private Courses selectedCourse;

        public ProgressPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                CourseFilter.ItemsSource = db.Courses.ToList();

                if (CourseFilter.Items.Count > 0)
                    CourseFilter.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CourseFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            selectedCourse = CourseFilter.SelectedItem as Courses;

            if (selectedCourse == null)
                return;

            DrawAll();
        }

        private void PrevChart_Click(object sender, RoutedEventArgs e)
        {
            currentChart--;

            if (currentChart < 0)
                currentChart = 2;

            ShowChart();
        }

        private void NextChart_Click(object sender, RoutedEventArgs e)
        {
            currentChart++;

            if (currentChart > 2)
                currentChart = 0;

            ShowChart();
        }

        private void DrawAll()
        {
            DrawPieChart();
            DrawBarChart();
            DrawModuleChart();

            ShowChart();
        }

        private void ShowChart()
        {
            PieChart.Visibility = Visibility.Collapsed;
            BarChart.Visibility = Visibility.Collapsed;
            ModuleChart.Visibility = Visibility.Collapsed;

            NoDataMessage.Visibility = Visibility.Collapsed;

            switch (currentChart)
            {
                case 0:
                    ChartTitle.Text = "Задания по статусам";

                    if (PieChart.Series == null || PieChart.Series.Count == 0)
                    {
                        NoDataMessage.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        PieChart.Visibility = Visibility.Visible;
                    }

                    break;

                case 1:
                    ChartTitle.Text = "Нагрузка студентов";

                    if (BarChart.Series == null || BarChart.Series.Count == 0)
                    {
                        NoDataMessage.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        BarChart.Visibility = Visibility.Visible;
                    }

                    break;

                case 2:
                    ChartTitle.Text = "Задания по модулям";

                    if (ModuleChart.Series == null || ModuleChart.Series.Count == 0)
                    {
                        NoDataMessage.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ModuleChart.Visibility = Visibility.Visible;
                    }

                    break;
            }
        }

        private void DrawPieChart()
        {
            PieChart.Series = new SeriesCollection();

            var assignments = db.Assignments
                .Where(a => a.CourseID == selectedCourse.CourseID)
                .ToList();

            var groups = assignments
                .GroupBy(a => a.Statuses.StatusName)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToList();

            foreach (var item in groups)
            {
                PieChart.Series.Add(new PieSeries
                {
                    Title = item.Status,
                    Values = new ChartValues<int> { item.Count },
                    DataLabels = true
                });
            }
        }

        private void DrawBarChart()
        {
            BarChart.Series = new SeriesCollection();

            var data = db.Assignments
                .Where(a => a.CourseID == selectedCourse.CourseID
                         && a.Users != null)
                .GroupBy(a => a.Users.Login)
                .Select(g => new
                {
                    User = g.Key,
                    Count = g.Count()
                })
                .ToList();

            if (data.Count == 0)
                return;

            BarChart.AxisX[0].Labels = data
                .Select(d => d.User)
                .ToArray();

            BarChart.Series.Add(new ColumnSeries
            {
                Title = "Задания",
                Values = new ChartValues<int>(
                    data.Select(d => d.Count)
                ),
                DataLabels = true,
                Fill = Brushes.DodgerBlue
            });
        }

        private void DrawModuleChart()
        {
            ModuleChart.Series = new SeriesCollection();

            var data = db.StudyModules
                .Where(m => m.CourseID == selectedCourse.CourseID)
                .Select(m => new
                {
                    Module = m.ModuleName,
                    Count = m.Assignments.Count
                })
                .ToList();

            if (data.Count == 0)
                return;

            ModuleChart.AxisX[0].Labels = data
                .Select(d => d.Module)
                .ToArray();

            ModuleChart.Series.Add(new ColumnSeries
            {
                Title = "Задания",
                Values = new ChartValues<int>(
                    data.Select(d => d.Count)
                ),
                DataLabels = true,
                Fill = Brushes.MediumSeaGreen
            });
        }
    }
}