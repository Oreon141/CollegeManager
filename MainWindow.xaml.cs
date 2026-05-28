using CollegeManager.DataBase;
using CollegeManager.Models;
using CollegeManager.Pages;
using CollegeManager.Windows;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace CollegeManager
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer notificationTimer = new DispatcherTimer();
        private CollegeManagerEntities db = CollegeManagerEntities.GetContext();

        public MainWindow()
        {
            InitializeComponent();

            if (CurrentUser.UserID == 0)
            {
                MessageBox.Show("Ошибка авторизации. Пожалуйста, войдите в систему.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
                return;
            }

            ConfigureNavigationByRole();
            LoadDefaultPage();
            SetupNotifications();
            CheckDeadlines();
        }

        /// Настройка видимости кнопок навигации в зависимости от роли пользователя
        private void ConfigureNavigationByRole()
        {
            UserInfoText.Text = $"{CurrentUser.Login} ({CurrentUser.Role})";

            CoursesNavButton.Visibility = Visibility.Visible;
            
            UsersNavButton.Visibility = CurrentUser.CanManageUsers ?
                Visibility.Visible : Visibility.Collapsed;

            this.Title = $"CollegeManager - {CurrentUser.Login} ({CurrentUser.Role})";
        }

        /// Проверка дедлайнов по курсам (Courses)
        private void CheckDeadlines()
        {
            DateTime now = DateTime.Now;
            DateTime deadlineLimit = now.AddDays(3);

            // Получаем курсы, у которых подходит дедлайн
            var upcomingCourses = db.Courses
                .Where(c => c.Deadline >= now && c.Deadline <= deadlineLimit)
                .ToList();

            foreach (var course in upcomingCourses)
            {
                // Находим всех участников этого курса
                var courseMembers = db.CourseMembers
                    .Where(cm => cm.CourseID == course.CourseID)
                    .ToList();

                foreach (var member in courseMembers)
                {
                    // Проверяем, создано ли уже такое уведомление
                    bool exists = db.Notifications.Any(n =>
                        n.UserID == member.UserID &&
                        n.RelatedEntityID == course.CourseID &&
                        n.Type == "Deadline");

                    if (exists)
                        continue;

                    Notifications notification = new Notifications()
                    {
                        UserID = member.UserID,
                        Title = "Приближается дедлайн курса",
                        Message = $"Срок завершения курса '{course.CourseName}' истекает {course.Deadline:dd.MM.yyyy}",
                        Type = "Deadline",
                        CreatedAt = now,
                        IsRead = false,
                        RelatedEntityID = course.CourseID
                    };

                    db.Notifications.Add(notification);
                }
            }

            db.SaveChanges();
        }

        private void SetupNotifications()
        {
            UpdateNotificationBadge();

            notificationTimer.Interval = TimeSpan.FromSeconds(3);
            notificationTimer.Tick += (s, e) =>
            {
                UpdateNotificationBadge();
            };
            notificationTimer.Start();
        }

        private void UpdateNotificationBadge()
        {
            int unreadCount = db.Notifications
                .Count(n => n.UserID == CurrentUser.UserID && n.IsRead == false);

            if (unreadCount > 0)
            {
                NotificationBadge.Visibility = Visibility.Visible;
                NotificationCountText.Text = unreadCount.ToString();
            }
            else
            {
                NotificationBadge.Visibility = Visibility.Collapsed;
            }
        }

        /// Загрузка страницы по умолчанию
        private void LoadDefaultPage()
        {
            MainFrame.Navigate(new CoursesPage());
        }

        private void Calendar_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CalendarPage());
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ProgressPage());
        }

        private void Courses_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CoursesPage());
        }

        private void Users_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser.CanManageUsers)
            {
                MainFrame.Navigate(new UsersPage());
            }
            else
            {
                MessageBox.Show("Доступ запрещен! Требуются права администратора.",
                    "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                MainFrame.Navigate(new CoursesPage());
            }
        }

        private void Notifications_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new NotificationsPage());
            UpdateNotificationBadge();
        }

        private void Chat_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ChatPage());
        }

        private void AcademicSheets_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new AcademicSheetsPage());
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Вы уверены, что хотите выйти из системы?",
                "Подтверждение выхода",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                CurrentUser.Clear();

                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }
    }
}