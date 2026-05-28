using System;

namespace CollegeManager.Models
{
    public static class CurrentUser
    {
        // ---------------- ТЕКУЩИЙ ПОЛЬЗОВАТЕЛЬ ----------------

        public static int UserID { get; set; }

        public static string Login { get; set; }

        public static string Role { get; set; }

        public static int RoleID { get; set; }

        // ---------------- ПРАВА ДОСТУПА ----------------

        // Управление преподавателями/студентами
        public static bool CanManageUsers =>
            Role == "Администратор";

        // Управление учебными курсами и модулями
        public static bool CanManageEducationalProjects =>
            Role == "Администратор" ||
            Role == "Преподаватель" ||
            Role == "Заведующий отделением" ||
            Role == "Методист";

        // Управление заданиями (выдача, изменение статусов)
        public static bool CanManageTasks =>
            Role == "Администратор" ||
            Role == "Преподаватель";

        // Просмотр учебных материалов
        public static bool CanViewEducationalContent => true;

        // Только просмотр (студенты)
        public static bool CanViewOnly =>
            Role == "Студент";

        // Генерация ведомостей и отчетов
        public static bool CanGenerateReports =>
            Role == "Администратор" ||
            Role == "Преподаватель" ||
            Role == "Заведующий отделением" ||
            Role == "Методист";

        // ---------------- ИНИЦИАЛИЗАЦИЯ ----------------

        /// <summary>
        /// Инициализация данных текущего пользователя при успешном входе
        /// </summary>
        public static void Initialize(
            int userId,
            string login,
            string role,
            int roleId)
        {
            UserID = userId;
            Login = login;
            Role = role;
            RoleID = roleId;
        }

        // ---------------- ОЧИСТКА ----------------

        /// <summary>
        /// Очистка данных пользователя при выходе из системы (Log Out)
        /// </summary>
        public static void Clear()
        {
            UserID = 0;
            Login = string.Empty;
            Role = string.Empty;
            RoleID = 0;
        }

        // ---------------- ПРОВЕРКИ РОЛЕЙ ----------------

        /// <summary>
        /// Проверка прав администратора
        /// </summary>
        public static bool IsAdministrator()
        {
            return Role == "Администратор";
        }

        /// <summary>
        /// Проверка прав преподавателя
        /// </summary>
        public static bool IsTeacher()
        {
            return Role == "Преподаватель";
        }

        /// <summary>
        /// Проверка прав только на просмотр
        /// </summary>
        public static bool HasViewOnlyRights()
        {
            return CanViewOnly;
        }
    }
}