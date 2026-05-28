using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using CollegeManager.DataBase; // Убедитесь, что это пространство имен верно для вашей модели

namespace CollegeManager.Pages
{
    public partial class CalendarPage : Page
    {
        // Контекст базы данных
        private readonly CollegeManagerEntities db = CollegeManagerEntities.GetContext();
        private DateTime currentDate;
        private List<Courses> allCourses;

        // Модель для отображения дня в календаре
        public class CalendarDay
        {
            public int DayNumber { get; set; }
            public DateTime Date { get; set; }
            public bool IsCurrentMonth { get; set; }
            public bool IsToday { get; set; }
            public bool HasDeadline { get; set; }
            public Brush DayBackground { get; set; } = Brushes.Transparent;
            public Brush TextColor { get; set; } = Brushes.Black;
            public FontWeight FontWeight { get; set; } = FontWeights.Normal;
        }

        // Модель для отображения курса в ListView
        public class CourseDeadlineView
        {
            public int CourseID { get; set; }
            public string CourseName { get; set; }
            public DateTime Deadline { get; set; }
            public string Description { get; set; }
            public string ControlForm { get; set; }
            public string FormattedDeadline => Deadline.ToString("dd MMMM yyyy", new CultureInfo("ru-RU"));
        }

        public CalendarPage()
        {
            InitializeComponent();
            currentDate = DateTime.Today;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCourses();
            GenerateCalendar();
            SelectDay(DateTime.Today);
        }

        private void LoadCourses()
        {
            allCourses = db.Courses.ToList();
        }

        private void GenerateCalendar()
        {
            var days = new ObservableCollection<CalendarDay>();
            var firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
            int firstDayWeekOffset = ((int)firstDayOfMonth.DayOfWeek + 6) % 7; // Пн - 0, Вс - 6

            // Дни предыдущего месяца для заполнения сетки
            for (int i = 0; i < firstDayWeekOffset; i++)
            {
                var date = firstDayOfMonth.AddDays(-(firstDayWeekOffset - i));
                days.Add(CreateCalendarDay(date, false));
            }

            // Дни текущего месяца
            int daysInMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
            for (int i = 1; i <= daysInMonth; i++)
            {
                var date = new DateTime(currentDate.Year, currentDate.Month, i);
                days.Add(CreateCalendarDay(date, true));
            }

            // Заполнение оставшихся ячеек (до 42)
            int totalCells = 42;
            int remainingCells = totalCells - days.Count;
            for (int i = 1; i <= remainingCells; i++)
            {
                var date = new DateTime(currentDate.Year, currentDate.Month, daysInMonth).AddDays(i);
                days.Add(CreateCalendarDay(date, false));
            }

            CalendarDaysControl.ItemsSource = days;
            UpdateMonthText();
        }

        private CalendarDay CreateCalendarDay(DateTime date, bool isCurrentMonth)
        {
            bool hasDeadline = allCourses.Any(c => c.Deadline.Date == date.Date);
            bool isToday = date.Date == DateTime.Today.Date;

            var day = new CalendarDay
            {
                DayNumber = date.Day,
                Date = date,
                IsCurrentMonth = isCurrentMonth,
                IsToday = isToday,
                HasDeadline = hasDeadline
            };

            // Стилизация
            if (hasDeadline)
            {
                day.DayBackground = new SolidColorBrush(Color.FromArgb(100, 244, 67, 54)); // Красный полупрозрачный
                day.TextColor = Brushes.White;
                day.FontWeight = FontWeights.Bold;
            }
            else if (isToday)
            {
                day.DayBackground = new SolidColorBrush(Color.FromArgb(100, 33, 150, 243)); // Синий полупрозрачный
                day.TextColor = Brushes.White;
            }
            else if (!isCurrentMonth)
            {
                day.TextColor = Brushes.Gray;
            }
            else if (date.DayOfWeek == DayOfWeek.Saturday) day.TextColor = Brushes.Blue;
            else if (date.DayOfWeek == DayOfWeek.Sunday) day.TextColor = Brushes.Red;

            return day;
        }

        private void SelectDay(DateTime date)
        {
            currentDate = date;
            SelectedDateText.Text = date.ToString("dddd, d MMMM yyyy", new CultureInfo("ru-RU"));

            var coursesOnDate = allCourses
                .Where(c => c.Deadline.Date == date.Date)
                .Select(c => new CourseDeadlineView
                {
                    CourseID = c.CourseID,
                    CourseName = c.CourseName,
                    Deadline = c.Deadline,
                    Description = c.Description ?? "Описание отсутствует",
                    ControlForm = c.ControlForm ?? "Не указана"
                })
                .ToList();

            EventsList.ItemsSource = coursesOnDate.Count > 0
                ? coursesOnDate
                : new List<CourseDeadlineView> {
                    new CourseDeadlineView { CourseName = "Нет дедлайнов", Description = "На эту дату курсов нет", Deadline = date }
                };
        }

        private void UpdateMonthText()
        {
            var culture = new CultureInfo("ru-RU");
            CurrentMonthText.Text = currentDate.ToString("MMMM yyyy", culture).ToUpper();
        }

        // Обработчики кнопок
        private void DayButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is DateTime date) SelectDay(date);
        }

        private void BtnPrevMonth_Click(object sender, RoutedEventArgs e) { currentDate = currentDate.AddMonths(-1); GenerateCalendar(); }
        private void BtnNextMonth_Click(object sender, RoutedEventArgs e) { currentDate = currentDate.AddMonths(1); GenerateCalendar(); }
        private void BtnPrevYear_Click(object sender, RoutedEventArgs e) { currentDate = currentDate.AddYears(-1); GenerateCalendar(); }
        private void BtnNextYear_Click(object sender, RoutedEventArgs e) { currentDate = currentDate.AddYears(1); GenerateCalendar(); }

        // Навигация при клике на курс
        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is CourseDeadlineView courseView)
            {
                
                NavigationService.Navigate(new CourseDetailsPage(courseView.CourseID));

            }
        }
    }
}