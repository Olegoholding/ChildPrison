using ChildPrison.Models;
using diplomApp.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace diplomApp.View
{
    public partial class GroupsPage : Page
    {
        private DataContext _context;
        private Group _currentGroup;
        private bool _isNewGroup = false;

        public GroupsPage()
        {
            InitializeComponent();
            Loaded += GroupsPage_Loaded;
        }

        private void GroupsPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                _context = new DataContext();

                var groups = _context.Groups
                    .Include(g => g.Teacher)
                    .Include(g => g.Children)
                    .ToList();
                GroupsListBox.ItemsSource = groups;

                var teachers = _context.Employees
                    .Where(e => e.Profession != null && e.Profession.ToLower().Contains("воспитатель"))
                    .ToList();
                TeacherComboBox.ItemsSource = teachers;
                TeacherComboBox.DisplayMemberPath = "FullName";
                TeacherComboBox.SelectedValuePath = "Id";

                var allChildren = _context.Children
                    .Include(c => c.Parent)
                    .ToList();
                ChildrenListBox.ItemsSource = allChildren;

                ClearForm();
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Ошибка: {ex.Message}";
            }
        }

        private void GroupsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GroupsListBox.SelectedItem is Group selectedGroup)
            {
                _isNewGroup = false;
                _currentGroup = _context.Groups
                    .Include(g => g.Teacher)
                    .Include(g => g.Children)
                    .FirstOrDefault(g => g.Id == selectedGroup.Id);

                if (_currentGroup != null)
                {
                    DisplayGroupData();
                    DeleteButton.Visibility = Visibility.Visible;
                    TitleText.Text = "ИНФОРМАЦИЯ О ГРУППЕ";
                    StatusText.Text = "";
                }
            }
            else
            {
                if (!_isNewGroup)
                {
                    ClearForm();
                }
            }
        }

        private void DisplayGroupData()
        {
            if (_currentGroup == null) return;

            GroupNameBox.Text = _currentGroup.GroupName;
            RoomNumberBox.Text = _currentGroup.RoomNumber;

            if (_currentGroup.TeacherId.HasValue && _currentGroup.TeacherId.Value > 0)
            {
                TeacherComboBox.SelectedValue = _currentGroup.TeacherId.Value;
            }
            else
            {
                TeacherComboBox.SelectedIndex = -1;
            }

            // Выбор детей в группе
            ChildrenListBox.SelectedItems.Clear();
            if (_currentGroup.Children != null && _currentGroup.Children.Any())
            {
                var childIds = _currentGroup.Children.Select(c => c.Id).ToList();
                foreach (var child in ChildrenListBox.Items)
                {
                    var c = child as Child;
                    if (c != null && childIds.Contains(c.Id))
                    {
                        ChildrenListBox.SelectedItems.Add(child);
                    }
                }
            }
        }

        private void AddGroup_Click(object sender, RoutedEventArgs e)
        {
            _isNewGroup = true;
            _currentGroup = new Group();
            ClearForm();
            DeleteButton.Visibility = Visibility.Collapsed;
            TitleText.Text = "ДОБАВЛЕНИЕ НОВОЙ ГРУППЫ";
            StatusText.Text = "";
            StatusText.Foreground = System.Windows.Media.Brushes.Red;

            GroupsListBox.SelectedItem = null;
        }

        private void ClearForm()
        {
            GroupNameBox.Text = "";
            RoomNumberBox.Text = "";
            TeacherComboBox.SelectedIndex = -1;
            ChildrenListBox.SelectedItems.Clear();
        }

        private void SaveGroup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentGroup == null)
                {
                    _currentGroup = new Group();
                    _isNewGroup = true;
                }

                // Валидация
                if (string.IsNullOrWhiteSpace(GroupNameBox.Text))
                {
                    StatusText.Text = "ЗАПОЛНИТЕ НАЗВАНИЕ ГРУППЫ!";
                    StatusText.Foreground = System.Windows.Media.Brushes.Red;
                    return;
                }

                // Заполнение данных группы
                _currentGroup.GroupName = GroupNameBox.Text.Trim();
                _currentGroup.RoomNumber = string.IsNullOrWhiteSpace(RoomNumberBox.Text) ? null : RoomNumberBox.Text.Trim();

                // Выбор воспитателя
                if (TeacherComboBox.SelectedValue != null && int.TryParse(TeacherComboBox.SelectedValue.ToString(), out int teacherId))
                {
                    _currentGroup.TeacherId = teacherId;
                }
                else
                {
                    _currentGroup.TeacherId = null;
                }

                // Сохранение группы
                if (_isNewGroup)
                {
                    _context.Groups.Add(_currentGroup);
                    _context.SaveChanges();
                }
                else
                {
                    _context.Entry(_currentGroup).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    _context.SaveChanges();
                }

                // Обновляем детей в группе
                UpdateGroupChildren();

                StatusText.Text = _isNewGroup ? "ГРУППА УСПЕШНО ДОБАВЛЕНА!" : "ДАННЫЕ СОХРАНЕНЫ!";
                StatusText.Foreground = System.Windows.Media.Brushes.Green;

                // Обновление данных
                LoadData();

                var savedGroup = _context.Groups
                    .Include(g => g.Teacher)
                    .Include(g => g.Children)
                    .FirstOrDefault(g => g.Id == _currentGroup.Id);

                if (savedGroup != null)
                {
                    GroupsListBox.SelectedItem = savedGroup;
                    _currentGroup = savedGroup;
                }

                _isNewGroup = false;
                DeleteButton.Visibility = Visibility.Visible;
                TitleText.Text = "ИНФОРМАЦИЯ О ГРУППЕ";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"ОШИБКА: {ex.Message}";
                StatusText.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private void UpdateGroupChildren()
        {
            try
            {
                // Получаем выбранных детей из списка
                var selectedChildren = ChildrenListBox.SelectedItems.Cast<Child>().ToList();
                var selectedChildIds = selectedChildren.Select(c => c.Id).ToList();

                // Получаем всех детей
                var allChildren = _context.Children.ToList();

                // Обновляем group_id у детей
                foreach (var child in allChildren)
                {
                    if (selectedChildIds.Contains(child.Id))
                    {
                        // Ребёнок должен быть в этой группе
                        if (child.GroupId != _currentGroup.Id)
                        {
                            child.GroupId = _currentGroup.Id;
                            _context.Entry(child).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        }
                    }
                    else
                    {
                        // Ребёнок не должен быть в этой группе
                        if (child.GroupId == _currentGroup.Id)
                        {
                            child.GroupId = 0; // или null, в зависимости от вашей БД
                            _context.Entry(child).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        }
                    }
                }

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                StatusText.Text = $"ОШИБКА ПРИ ОБНОВЛЕНИИ ДЕТЕЙ: {ex.Message}";
            }
        }

        private void DeleteGroup_Click(object sender, RoutedEventArgs e)
        {
            if (_currentGroup == null || _isNewGroup) return;

            // Проверяем, есть ли дети в группе
            var childrenCount = _context.Children.Count(c => c.GroupId == _currentGroup.Id);

            if (childrenCount > 0)
            {
                var result = MessageBox.Show(
                    $"В группе {childrenCount} детей. При удалении группы дети останутся без группы.\n\nВы уверены, что хотите удалить группу?",
                    "ПОДТВЕРЖДЕНИЕ УДАЛЕНИЯ",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes) return;
            }
            else
            {
                var result = MessageBox.Show(
                    $"ВЫ УВЕРЕНЫ, ЧТО ХОТИТЕ УДАЛИТЬ ГРУППУ {_currentGroup.GroupName}?",
                    "ПОДТВЕРЖДЕНИЕ УДАЛЕНИЯ",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes) return;
            }

            try
            {
                // Отвязываем детей от группы
                var childrenInGroup = _context.Children.Where(c => c.GroupId == _currentGroup.Id).ToList();
                foreach (var child in childrenInGroup)
                {
                    child.GroupId = 0;
                    _context.Entry(child).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }
                _context.SaveChanges();

                // Удаляем группу
                _context.Groups.Remove(_currentGroup);
                _context.SaveChanges();

                StatusText.Text = "ГРУППА УДАЛЕНА";
                StatusText.Foreground = System.Windows.Media.Brushes.Red;

                LoadData();
                ClearForm();
                _currentGroup = null;
                DeleteButton.Visibility = Visibility.Collapsed;
                TitleText.Text = "ИНФОРМАЦИЯ О ГРУППЕ";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"ОШИБКА УДАЛЕНИЯ: {ex.Message}";
                StatusText.Foreground = System.Windows.Media.Brushes.Red;
            }
        }
    }
}