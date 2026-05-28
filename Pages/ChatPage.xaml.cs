using CollegeManager.DataBase;
using CollegeManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CollegeManager.Pages
{
    public partial class ChatPage : Page
    {
        private CollegeManagerEntities db =
            CollegeManagerEntities.GetContext();

        private Users selectedUser;

        private Chats currentChat;

        private List<ChatListItem> allChats =
            new List<ChatListItem>();

        DispatcherTimer timer =
            new DispatcherTimer();

        public ChatPage()
        {
            InitializeComponent();

            LoadChats();

            timer.Interval =
                TimeSpan.FromSeconds(2);

            timer.Tick += Timer_Tick;

            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (currentChat != null)
            {
                LoadMessages();
            }
        }

        /// ЗАГРУЗКА ЧАТОВ

        private void LoadChats()
        {
            allChats.Clear();

            int currentUserId =
                CurrentUser.UserID;

            // ПОЛЬЗОВАТЕЛИ

            var users = db.Users
                .Include("UserRoles")
                .Where(u => u.UserID != currentUserId)
                .ToList();

            foreach (var user in users)
            {
                Chats existingChat = null;

                var chats = db.Chats
                    .Where(c => c.IsGroup == false)
                    .ToList();

                foreach (var chat in chats)
                {
                    var participants = db.ChatParticipants
                        .Where(cp => cp.ChatID == chat.ChatID)
                        .Select(cp => cp.UserID)
                        .ToList();

                    if (participants.Count == 2 &&
                        participants.Contains(currentUserId) &&
                        participants.Contains(user.UserID))
                    {
                        existingChat = chat;
                        break;
                    }
                }

                DateTime? lastMessageTime = null;

                if (existingChat != null)
                {
                    lastMessageTime = db.Messages
                        .Where(m => m.ChatID == existingChat.ChatID)
                        .OrderByDescending(m => m.SentAt)
                        .Select(m => (DateTime?)m.SentAt)
                        .FirstOrDefault();
                }

                allChats.Add(new ChatListItem()
                {
                    Chat = existingChat,

                    User = user,

                    DisplayName =
                        $"{user.FirstName} {user.Surname}",

                    TypeText =
                        user.UserRoles?.RoleName,

                    LastMessageTime =
                        lastMessageTime
                });
            }

            // ГРУППЫ

            var groupChats = db.Chats
                .Where(c => c.IsGroup == true)
                .ToList();

            foreach (var group in groupChats)
            {
                bool isParticipant =
                    db.ChatParticipants.Any(cp =>
                        cp.ChatID == group.ChatID &&
                        cp.UserID == currentUserId);

                if (!isParticipant)
                    continue;

                DateTime? lastMessageTime =
                    db.Messages
                    .Where(m => m.ChatID == group.ChatID)
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => (DateTime?)m.SentAt)
                    .FirstOrDefault();

                allChats.Add(new ChatListItem()
                {
                    Chat = group,

                    DisplayName = group.Name,

                    TypeText = "Группа",

                    LastMessageTime = lastMessageTime
                });
            }

            ChatsList.ItemsSource = allChats
                .OrderByDescending(c =>
                    c.LastMessageTime ?? DateTime.MinValue)
                .ThenBy(c => c.DisplayName)
                .ToList();
        }

        /// УДАЛЕНИЕ ЧАТА

        private void DeleteChat_Click(object sender,
            RoutedEventArgs e)
        {
            if (currentChat == null)
            {
                MessageBox.Show("Выберите чат");
                return;
            }

            MessageBoxResult result =
                MessageBox.Show(
                    $"Удалить чат '{currentChat.Name}'?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                var messages = db.Messages
                    .Where(m => m.ChatID == currentChat.ChatID)
                    .ToList();

                db.Messages.RemoveRange(messages);

                var participants = db.ChatParticipants
                    .Where(cp => cp.ChatID == currentChat.ChatID)
                    .ToList();

                db.ChatParticipants.RemoveRange(participants);

                db.Chats.Remove(currentChat);

                db.SaveChanges();

                MessageBox.Show("Чат удалён");

                currentChat = null;

                SelectedChatText.Text =
                    "Выберите чат";

                MessagesList.ItemsSource = null;

                DeleteChatButton.Visibility =
                    Visibility.Collapsed;

                LoadChats();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка удаления: {ex.Message}");
            }
        }

        /// ПОИСК

        private void SearchBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            string text = SearchBox.Text
                .ToLower()
                .Trim();

            if (string.IsNullOrWhiteSpace(text))
            {
                ChatsList.ItemsSource = allChats
                    .OrderByDescending(c =>
                        c.LastMessageTime ??
                        DateTime.MinValue)
                    .ThenBy(c => c.DisplayName)
                    .ToList();

                return;
            }

            ChatsList.ItemsSource = allChats
                .Where(c =>
                    c.DisplayName
                    .ToLower()
                    .Contains(text))
                .OrderByDescending(c =>
                    c.LastMessageTime ??
                    DateTime.MinValue)
                .ToList();
        }

        /// ВЫБОР ЧАТА

        private void ChatsList_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            var item =
                ChatsList.SelectedItem as ChatListItem;

            if (item == null)
                return;

            SelectedChatText.Text =
                item.DisplayName;

            DeleteChatButton.Visibility =
                Visibility.Visible;

            // ГРУППА

            if (item.Chat != null &&
                item.Chat.IsGroup == true)
            {
                currentChat = item.Chat;

                LoadMessages();

                return;
            }

            // ЛИЧНЫЙ ЧАТ

            selectedUser = item.User;

            OpenOrCreateChat();
        }

        /// ОТКРЫТИЕ ИЛИ СОЗДАНИЕ ЧАТА

        private void OpenOrCreateChat()
        {
            int currentUserId =
                CurrentUser.UserID;

            var chats = db.Chats
                .Where(c => c.IsGroup == false)
                .ToList();

            foreach (var chat in chats)
            {
                var participants =
                    db.ChatParticipants
                    .Where(cp => cp.ChatID == chat.ChatID)
                    .Select(cp => cp.UserID)
                    .ToList();

                if (participants.Count == 2 &&
                    participants.Contains(currentUserId) &&
                    participants.Contains(selectedUser.UserID))
                {
                    currentChat = chat;

                    LoadMessages();

                    return;
                }
            }

            CreateNewChat();
        }

        /// СОЗДАНИЕ ЧАТА

        private void CreateNewChat()
        {
            Chats chat = new Chats()
            {
                Name =
                    $"{CurrentUser.Login}_{selectedUser.Login}",

                IsGroup = false,

                CreatedAt = DateTime.Now
            };

            db.Chats.Add(chat);

            db.SaveChanges();

            db.ChatParticipants.Add(
                new ChatParticipants()
                {
                    ChatID = chat.ChatID,
                    UserID = CurrentUser.UserID
                });

            db.ChatParticipants.Add(
                new ChatParticipants()
                {
                    ChatID = chat.ChatID,
                    UserID = selectedUser.UserID
                });

            db.SaveChanges();

            currentChat = chat;

            LoadMessages();

            LoadChats();
        }

        /// ЗАГРУЗКА СООБЩЕНИЙ

        private void LoadMessages()
        {
            if (currentChat == null)
                return;

            var messages = db.Messages
                .Include("Users")
                .Where(m =>
                    m.ChatID == currentChat.ChatID)
                .OrderBy(m => m.SentAt)
                .ToList();

            MessagesList.ItemsSource =
                messages;
        }

        /// ОТПРАВКА

        private void SendMessage_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (currentChat == null)
            {
                MessageBox.Show(
                    "Выберите чат");

                return;
            }

            if (string.IsNullOrWhiteSpace(
                MessageTextBox.Text))
            {
                MessageBox.Show(
                    "Введите сообщение");

                return;
            }

            Messages message =
                new Messages()
                {
                    ChatID = currentChat.ChatID,

                    SenderID = CurrentUser.UserID,

                    MessageText =
                        MessageTextBox.Text.Trim(),

                    SentAt = DateTime.Now,

                    IsRead = false
                };

            db.Messages.Add(message);

            db.SaveChanges();

            MessageTextBox.Clear();

            LoadMessages();

            LoadChats();
        }

        /// СОЗДАНИЕ ГРУППЫ

        
    }

    public class ChatListItem
    {
        public Chats Chat { get; set; }

        public Users User { get; set; }

        public string DisplayName { get; set; }

        public string TypeText { get; set; }

        public DateTime? LastMessageTime { get; set; }
    }
}