using ChildPrison.Models;
using diplomApp.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace diplomApp.View
{
    public partial class ChildrenPage : Page
    {
        private DataContext _context;
        private Child _currentChild;
        private bool _isNewChild = false;

        public ChildrenPage()
        {
            InitializeComponent();
            Loaded += ChildrenPage_Loaded;
        }

        private void ChildrenPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                _context = new DataContext();

                var children = _context.Children
                    .Include(c => c.Parent)
                    .Include(c => c.Group)
                    .ToList();
                ChildrenListBox.ItemsSource = children;

                var groups = _context.Groups.ToList();
                GroupComboBox.ItemsSource = groups;
                GroupComboBox.DisplayMemberPath = "GroupName";
                GroupComboBox.SelectedValuePath = "Id";

                var parents = _context.Parents.ToList();
                var parentItems = parents.Select(p => new ParentDisplayItem
                {
                    Id = p.Id,
                    FullName = $"{p.LastName} {p.FirstName} {p.MiddleName}".Trim()
                }).ToList();

                ParentComboBox.ItemsSource = parentItems;
                ParentComboBox.DisplayMemberPath = "FullName";
                ParentComboBox.SelectedValuePath = "Id";

                ClearForm();
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Ошибка: {ex.Message}";
            }
        }

        private void ChildrenListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChildrenListBox.SelectedItem is Child selectedChild)
            {
                _isNewChild = false;
                _currentChild = _context.Children
                    .Include(c => c.Parent)
                    .Include(c => c.Group)
                    .FirstOrDefault(c => c.Id == selectedChild.Id);

                if (_currentChild != null)
                {
                    DisplayChildData();
                    DeleteButton.Visibility = Visibility.Visible;
                    TitleText.Text = "ИНФОРМАЦИЯ О РЕБЁНКЕ";
                    StatusText.Text = "";
                }
            }
        }

        private void DisplayChildData()
        {
            if (_currentChild == null) return;

            LastNameBox.Text = _currentChild.LastName;
            FirstNameBox.Text = _currentChild.FirstName;
            MidNameBox.Text = _currentChild.MidName;
            BirthDatePicker.SelectedDate = _currentChild.BirthDate;

            if (_currentChild.GroupId > 0)
            {
                GroupComboBox.SelectedValue = _currentChild.GroupId;
            }
            else
            {
                GroupComboBox.SelectedIndex = -1;
            }

            if (_currentChild.ParentId > 0)
            {
                var parentItem = ((IEnumerable<ParentDisplayItem>)ParentComboBox.ItemsSource)
                    ?.FirstOrDefault(p => p.Id == _currentChild.ParentId);
                if (parentItem != null)
                {
                    ParentComboBox.SelectedValue = _currentChild.ParentId;
                }

                if (_currentChild.Parent != null)
                {
                    ParentLastNameBox.Text = _currentChild.Parent.LastName;
                    ParentFirstNameBox.Text = _currentChild.Parent.FirstName;
                    ParentMiddleNameBox.Text = _currentChild.Parent.MiddleName;
                    ParentContactBox.Text = _currentChild.Parent.ContactInfo;
                }
            }
            else
            {
                ParentComboBox.SelectedIndex = -1;
            }

            NewParentCheckBox.IsChecked = false;
            ExistingParentPanel.Visibility = Visibility.Visible;
            NewParentPanel.Visibility = Visibility.Collapsed;
        }

        private void AddChild_Click(object sender, RoutedEventArgs e)
        {
            _isNewChild = true;
            _currentChild = new Child(); 
            ClearForm();
            DeleteButton.Visibility = Visibility.Collapsed;
            TitleText.Text = "ДОБАВЛЕНИЕ НОВОГО РЕБЁНКА";
            StatusText.Text = "";
            StatusText.Foreground = System.Windows.Media.Brushes.Red;
        }

        private void ClearForm()
        {
            LastNameBox.Text = "";
            FirstNameBox.Text = "";
            MidNameBox.Text = "";
            BirthDatePicker.SelectedDate = null;
            GroupComboBox.SelectedIndex = -1;

            ParentLastNameBox.Text = "";
            ParentFirstNameBox.Text = "";
            ParentMiddleNameBox.Text = "";
            ParentContactBox.Text = "";

            NewParentCheckBox.IsChecked = false;
            ExistingParentPanel.Visibility = Visibility.Visible;
            NewParentPanel.Visibility = Visibility.Collapsed;
            ParentComboBox.SelectedIndex = -1;
        }

        private void NewParentCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ExistingParentPanel.Visibility = Visibility.Collapsed;
            NewParentPanel.Visibility = Visibility.Visible;
        }

        private void NewParentCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ExistingParentPanel.Visibility = Visibility.Visible;
            NewParentPanel.Visibility = Visibility.Collapsed;
        }

        private void ParentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ParentComboBox.SelectedItem is ParentDisplayItem selectedParent && !_isNewChild && _currentChild != null)
            {
                var parent = _context.Parents.Find(selectedParent.Id);
                if (parent != null)
                {
                    ParentLastNameBox.Text = parent.LastName;
                    ParentFirstNameBox.Text = parent.FirstName;
                    ParentMiddleNameBox.Text = parent.MiddleName;
                    ParentContactBox.Text = parent.ContactInfo;
                }
            }
        }

        private void SaveChild_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentChild == null)
                {
                    _currentChild = new Child();
                    _isNewChild = true;
                }

                if (string.IsNullOrWhiteSpace(LastNameBox.Text) ||
                    string.IsNullOrWhiteSpace(FirstNameBox.Text))
                {
                    StatusText.Text = "ЗАПОЛНИТЕ ФАМИЛИЮ И ИМЯ РЕБЁНКА!";
                    StatusText.Foreground = System.Windows.Media.Brushes.Red;
                    return;
                }

                _currentChild.LastName = LastNameBox.Text.Trim();
                _currentChild.FirstName = FirstNameBox.Text.Trim();
                _currentChild.MidName = string.IsNullOrWhiteSpace(MidNameBox.Text) ? null : MidNameBox.Text.Trim();
                _currentChild.BirthDate = BirthDatePicker.SelectedDate;

                if (GroupComboBox.SelectedValue != null && int.TryParse(GroupComboBox.SelectedValue.ToString(), out int groupId))
                {
                    _currentChild.GroupId = groupId;
                }

                if (NewParentCheckBox.IsChecked == true)
                {
                    if (string.IsNullOrWhiteSpace(ParentLastNameBox.Text) ||
                        string.IsNullOrWhiteSpace(ParentFirstNameBox.Text))
                    {
                        StatusText.Text = "ЗАПОЛНИТЕ ФАМИЛИЮ И ИМЯ РОДИТЕЛЯ!";
                        StatusText.Foreground = System.Windows.Media.Brushes.Red;
                        return;
                    }

                    var newParent = new Parent
                    {
                        LastName = ParentLastNameBox.Text.Trim(),
                        FirstName = ParentFirstNameBox.Text.Trim(),
                        MiddleName = string.IsNullOrWhiteSpace(ParentMiddleNameBox.Text) ? null : ParentMiddleNameBox.Text.Trim(),
                        ContactInfo = string.IsNullOrWhiteSpace(ParentContactBox.Text) ? null : ParentContactBox.Text.Trim()
                    };

                    if (_isNewChild)
                    {
                        _context.Parents.Add(newParent);
                        _context.SaveChanges();
                        _currentChild.ParentId = newParent.Id;
                        _context.Children.Add(_currentChild);
                        _context.SaveChanges();
                    }
                    else
                    {
                        _context.Parents.Add(newParent);
                        _context.SaveChanges();
                        _currentChild.ParentId = newParent.Id;
                        _context.Entry(_currentChild).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        _context.SaveChanges();
                    }
                }
                else
                {
                    if (ParentComboBox.SelectedValue == null)
                    {
                        StatusText.Text = "ВЫБЕРИТЕ РОДИТЕЛЯ ИЛИ ДОБАВЬТЕ НОВОГО!";
                        StatusText.Foreground = System.Windows.Media.Brushes.Red;
                        return;
                    }

                    _currentChild.ParentId = (int)ParentComboBox.SelectedValue;

                    if (_isNewChild)
                    {
                        _context.Children.Add(_currentChild);
                        _context.SaveChanges();
                    }
                    else
                    {
                        _context.Entry(_currentChild).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        _context.SaveChanges();
                    }
                }

                StatusText.Text = _isNewChild ? "РЕБЁНОК УСПЕШНО ДОБАВЛЕН!" : "ДАННЫЕ СОХРАНЕНЫ!";
                StatusText.Foreground = System.Windows.Media.Brushes.Green;

                LoadData();

                var savedChild = _context.Children
                    .Include(c => c.Parent)
                    .Include(c => c.Group)
                    .FirstOrDefault(c => c.Id == _currentChild.Id);

                if (savedChild != null)
                {
                    ChildrenListBox.SelectedItem = savedChild;
                    _currentChild = savedChild;
                }

                _isNewChild = false;
                DeleteButton.Visibility = Visibility.Visible;
                TitleText.Text = "ИНФОРМАЦИЯ О РЕБЁНКЕ";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"ОШИБКА: {ex.Message}";
                StatusText.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private void DeleteChild_Click(object sender, RoutedEventArgs e)
        {
            if (_currentChild == null || _isNewChild) return;

            var result = MessageBox.Show(
                $"ВЫ УВЕРЕНЫ, ЧТО ХОТИТЕ УДАЛИТЬ {_currentChild.LastName} {_currentChild.FirstName}?",
                "ПОДТВЕРЖДЕНИЕ УДАЛЕНИЯ",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Children.Remove(_currentChild);
                    _context.SaveChanges();

                    StatusText.Text = "РЕБЁНОК УДАЛЁН";
                    StatusText.Foreground = System.Windows.Media.Brushes.Red;

                    LoadData();
                    ClearForm();
                    _currentChild = null;
                    DeleteButton.Visibility = Visibility.Collapsed;
                    TitleText.Text = "ИНФОРМАЦИЯ О РЕБЁНКЕ";
                }
                catch (Exception ex)
                {
                    StatusText.Text = $"ОШИБКА УДАЛЕНИЯ: {ex.Message}";
                    StatusText.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
        }
    }

    public class ParentDisplayItem
    {
        public int Id { get; set; }
        public string FullName { get; set; }
    }
}