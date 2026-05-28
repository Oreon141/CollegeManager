using System.Collections.Generic;
using System.Linq;
using CollegeManager.DataBase; // Подключение контекста сущностей новой БД

namespace CollegeManager.Services
{
    public class AuthService
    {
        // ---------------- ПРОВЕРКА ПОЛЕЙ ----------------

        /// <summary>
        /// Проверка корректности заполнения полей авторизации (валидация на пустые строки)
        /// </summary>
        public bool ValidateAuthorizationFields(
            string login,
            string password,
            out string errorMessage)
        {
            errorMessage = string.Empty;

            // Проверка логина
            if (string.IsNullOrWhiteSpace(login))
            {
                errorMessage = "Введите логин!";
                return false;
            }

            // Проверка пароля
            if (string.IsNullOrWhiteSpace(password))
            {
                errorMessage = "Введите пароль!";
                return false;
            }

            return true;
        }

        // ---------------- АВТОРИЗАЦИЯ ----------------

        /// <summary>
        /// Авторизация пользователя по коллекции из таблицы Users
        /// </summary>
        public bool AuthorizeUser(
            IEnumerable<Users> usersCollection,
            string login,
            string password,
            out Users authorizedUser)
        {
            authorizedUser = usersCollection
                .FirstOrDefault(user =>
                    user.Login == login &&
                    user.Password == password);

            return authorizedUser != null;
        }
    }
}