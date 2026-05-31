using finish.Classes;
using finish.Model;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace finish.Pages
{
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            dataGridUsers.ItemsSource = AppData.db.User.ToList();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Заполните логин и пароль");
                return;
            }

            if (AppData.db.User.Any(u => u.login == login))
            {
                MessageBox.Show("Пользователь с таким логином уже существует");
                return;
            }

            int roleId = cmbRole.SelectedIndex == 0 ? 1 : 2;

            User newUser = new User
            {
                login = login,
                password = password,
                block = false,
                role_id = roleId
            };

            AppData.db.User.Add(newUser);
            AppData.db.SaveChanges();

            MessageBox.Show("Пользователь добавлен");
            LoadUsers();
            ClearForm();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridUsers.SelectedItem == null)
            {
                MessageBox.Show("Выберите пользователя");
                return;
            }

            User selectedUser = (User)dataGridUsers.SelectedItem;
            string login = txtLogin.Text.Trim();

            if (string.IsNullOrEmpty(login))
            {
                MessageBox.Show("Введите логин");
                return;
            }

            if (AppData.db.User.Any(u => u.login == login && u.id != selectedUser.id))
            {
                MessageBox.Show("Пользователь с таким логином уже существует");
                return;
            }

            selectedUser.login = login;

            if (!string.IsNullOrEmpty(txtPassword.Text))
            {
                selectedUser.password = txtPassword.Text;
            }

            selectedUser.role_id = cmbRole.SelectedIndex == 0 ? 1 : 2;
            AppData.db.SaveChanges();

            MessageBox.Show("Данные изменены");
            LoadUsers();
            ClearForm();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridUsers.SelectedItem == null)
            {
                MessageBox.Show("Выберите пользователя");
                return;
            }

            User selectedUser = (User)dataGridUsers.SelectedItem;

            if (MessageBox.Show($"Удалить {selectedUser.login}?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                AppData.db.User.Remove(selectedUser);
                AppData.db.SaveChanges();
                LoadUsers();
                ClearForm();
                MessageBox.Show("Пользователь удалён");
            }
        }

        private void btnUnblock_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridUsers.SelectedItem == null)
            {
                MessageBox.Show("Выберите пользователя");
                return;
            }

            User selectedUser = (User)dataGridUsers.SelectedItem;
            selectedUser.block = false;
            AppData.db.SaveChanges();

            MessageBox.Show("Блокировка снята");
            LoadUsers();
        }

        private void dataGridUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGridUsers.SelectedItem != null)
            {
                User selected = (User)dataGridUsers.SelectedItem;
                txtLogin.Text = selected.login;
                txtPassword.Text = "";
                cmbRole.SelectedIndex = (selected.role_id == 1) ? 0 : 1;
            }
        }

        private void ClearForm()
        {
            txtLogin.Clear();
            txtPassword.Text = "";
            cmbRole.SelectedIndex = 1;
            dataGridUsers.SelectedItem = null;
        }
    }
}