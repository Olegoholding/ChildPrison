using ChildPrison.Models;
using diplomApp.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace diplomApp.View
{
    public partial class EmployeesPage : Page
    {
        private DataContext _context;
        private Employee _currentEmployee;
        private bool _isNewEmployee = false;

        public EmployeesPage()
        {
            InitializeComponent();
            Loaded += EmployeesPage_Loaded;
        }

        private void EmployeesPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                _context = new DataContext();

                var employees = _context.Employees
                    .Include(e => e.Status)
                    .ToList();
                EmployeesListBox.ItemsSource = employees;

                var statuses = _context.EmployeeStatuses.ToList();
                StatusComboBox.ItemsSource = statuses;
                StatusComboBox.DisplayMemberPath = "StatusName";
                StatusComboBox.SelectedValuePath = "Id";

                ClearForm();
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Ошибка: {ex.Message}";
            }
        }

        private void EmployeesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EmployeesListBox.SelectedItem is Employee selectedEmployee)
            {
                _isNewEmployee = false;
                _currentEmployee = _context.Employees
                    .Include(e => e.Status)
                    .FirstOrDefault(e => e.Id == selectedEmployee.Id);

                if (_currentEmployee != null)
                {
                    DisplayEmployeeData();
                    DeleteButton.Visibility = Visibility.Visible;
                    TitleText.Text = "ИНФОРМАЦИЯ О СОТРУДНИКЕ";
                    StatusText.Text = "";

                    CheckProfessionAndShowGroups();
                }
            }
            else
            {
                if (!_isNewEmployee)
                {
                    ClearForm();
                }
            }
        }

        private void DisplayEmployeeData()
        {
            if (_currentEmployee == null) return;

            LastNameBox.Text = _currentEmployee.LastName;
            FirstNameBox.Text = _currentEmployee.FirstName;
            MidNameBox.Text = _currentEmployee.MidName;
            ProfessionBox.Text = _currentEmployee.Profession;
            EducationBox.Text = _currentEmployee.Education;
            HireDatePicker.SelectedDate = _currentEmployee.HireDate;

            if (_currentEmployee.StatusId.HasValue && _currentEmployee.StatusId.Value > 0)
            {
                StatusComboBox.SelectedValue = _currentEmployee.StatusId.Value;
            }
            else
            {
                StatusComboBox.SelectedIndex = -1;
            }
        }

        private void AddEmployee_Click(object sender, RoutedEventArgs e)
        {
            _isNewEmployee = true;
            _currentEmployee = new Employee();
            ClearForm();
            DeleteButton.Visibility = Visibility.Collapsed;
            TitleText.Text = "ДОБАВЛЕНИЕ НОВОГО СОТРУДНИКА";
            StatusText.Text = "";
            StatusText.Foreground = System.Windows.Media.Brushes.Red;

            EmployeesListBox.SelectedItem = null;
            GroupsBorder.Visibility = Visibility.Collapsed;
        }

        private void ClearForm()
        {
            LastNameBox.Text = "";
            FirstNameBox.Text = "";
            MidNameBox.Text = "";
            ProfessionBox.Text = "";
            EducationBox.Text = "";
            HireDatePicker.SelectedDate = null;
            StatusComboBox.SelectedIndex = -1;
            GroupsListBox.ItemsSource = null;
            GroupsListBox.SelectedItems.Clear();
            GroupsBorder.Visibility = Visibility.Collapsed;
        }

        private void CheckProfessionAndShowGroups()
        {
            if (_currentEmployee == null) return;

            bool isTeacher = !string.IsNullOrEmpty(_currentEmployee.Profession) &&
                             _currentEmployee.Profession.ToLower().Contains("воспитатель");

            if (isTeacher)
            {
                LoadTeacherGroups();
            }
            else
            {
                GroupsBorder.Visibility = Visibility.Collapsed;
                GroupsListBox.ItemsSource = null;
            }
        }

        private void LoadTeacherGroups()
        {
            try
            {
                if (_currentEmployee == null) return;

                var allGroups = _context.Groups.ToList();

                var teacherGroupIds = _context.Groups
                    .Where(g => g.TeacherId == _currentEmployee.Id)
                    .Select(g => g.Id)
                    .ToList();

                GroupsListBox.ItemsSource = allGroups;

                GroupsListBox.SelectedItems.Clear();
                foreach (var group in allGroups)
                {
                    if (teacherGroupIds.Contains(group.Id))
                    {
                        GroupsListBox.SelectedItems.Add(group);
                    }
                }

                GroupsBorder.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Ошибка загрузки групп: {ex.Message}";
            }
        }

        private void ProfessionBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_currentEmployee != null && !_isNewEmployee)
            {
                _currentEmployee.Profession = ProfessionBox.Text;
                CheckProfessionAndShowGroups();
            }
            else if (_isNewEmployee && ProfessionBox != null)
            {
                bool isTeacher = !string.IsNullOrEmpty(ProfessionBox.Text) &&
                                 ProfessionBox.Text.ToLower().Contains("воспитатель");

                if (isTeacher)
                {
                    var allGroups = _context.Groups.ToList();
                    GroupsListBox.ItemsSource = allGroups;
                    GroupsBorder.Visibility = Visibility.Visible;
                }
                else
                {
                    GroupsBorder.Visibility = Visibility.Collapsed;
                    GroupsListBox.ItemsSource = null;
                }
            }
        }

        private void SaveEmployee_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentEmployee == null)
                {
                    _currentEmployee = new Employee();
                    _isNewEmployee = true;
                }

                if (string.IsNullOrWhiteSpace(LastNameBox.Text) ||
                    string.IsNullOrWhiteSpace(FirstNameBox.Text) ||
                    string.IsNullOrWhiteSpace(MidNameBox.Text))
                {
                    StatusText.Text = "ЗАПОЛНИТЕ ФАМИЛИЮ, ИМЯ И ОТЧЕСТВО СОТРУДНИКА!";
                    StatusText.Foreground = System.Windows.Media.Brushes.Red;
                    return;
                }

                _currentEmployee.LastName = LastNameBox.Text.Trim();
                _currentEmployee.FirstName = FirstNameBox.Text.Trim();
                _currentEmployee.MidName = MidNameBox.Text.Trim();
                _currentEmployee.Profession = string.IsNullOrWhiteSpace(ProfessionBox.Text) ? null : ProfessionBox.Text.Trim();
                _currentEmployee.Education = string.IsNullOrWhiteSpace(EducationBox.Text) ? null : EducationBox.Text.Trim();
                _currentEmployee.HireDate = HireDatePicker.SelectedDate;

                if (StatusComboBox.SelectedValue != null && int.TryParse(StatusComboBox.SelectedValue.ToString(), out int statusId))
                {
                    _currentEmployee.StatusId = statusId;
                }
                else
                {
                    _currentEmployee.StatusId = null;
                }

                if (_isNewEmployee)
                {
                    _context.Employees.Add(_currentEmployee);
                    _context.SaveChanges();
                }
                else
                {
                    _context.Entry(_currentEmployee).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    _context.SaveChanges();
                }

                if (_currentEmployee.Profession != null && _currentEmployee.Profession.ToLower().Contains("воспитатель"))
                {
                    UpdateTeacherGroups();
                }
                else
                {
                    ClearTeacherGroups();
                }

                StatusText.Text = _isNewEmployee ? "СОТРУДНИК УСПЕШНО ДОБАВЛЕН!" : "ДАННЫЕ СОХРАНЕНЫ!";
                StatusText.Foreground = System.Windows.Media.Brushes.Green;

                LoadData();

                var savedEmployee = _context.Employees
                    .Include(e => e.Status)
                    .FirstOrDefault(e => e.Id == _currentEmployee.Id);

                if (savedEmployee != null)
                {
                    EmployeesListBox.SelectedItem = savedEmployee;
                    _currentEmployee = savedEmployee;
                }

                _isNewEmployee = false;
                DeleteButton.Visibility = Visibility.Visible;
                TitleText.Text = "ИНФОРМАЦИЯ О СОТРУДНИКЕ";
                CheckProfessionAndShowGroups();
            }
            catch (Exception ex)
            {
                StatusText.Text = $"ОШИБКА: {ex.Message}";
                StatusText.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private void UpdateTeacherGroups()
        {
            try
            {
                var selectedGroups = GroupsListBox.SelectedItems.Cast<Group>().ToList();
                var selectedGroupIds = selectedGroups.Select(g => g.Id).ToList();

                var allGroups = _context.Groups.ToList();

                foreach (var group in allGroups)
                {
                    if (selectedGroupIds.Contains(group.Id))
                    {
                        if (group.TeacherId != _currentEmployee.Id)
                        {
                            group.TeacherId = _currentEmployee.Id;
                            _context.Entry(group).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        }
                    }
                    else
                    {
                        if (group.TeacherId == _currentEmployee.Id)
                        {
                            group.TeacherId = null;
                            _context.Entry(group).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        }
                    }
                }

                _context.SaveChanges();
                StatusText.Text = "Группы успешно обновлены!";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"ОШИБКА ПРИ ОБНОВЛЕНИИ ГРУПП: {ex.Message}";
                StatusText.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private void ClearTeacherGroups()
        {
            try
            {
                var groupsWithThisTeacher = _context.Groups
                    .Where(g => g.TeacherId == _currentEmployee.Id)
                    .ToList();

                foreach (var group in groupsWithThisTeacher)
                {
                    group.TeacherId = null;
                    _context.Entry(group).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                StatusText.Text = $"ОШИБКА ПРИ ОЧИСТКЕ ГРУПП: {ex.Message}";
            }
        }

        private void DeleteEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (_currentEmployee == null || _isNewEmployee) return;

            var result = MessageBox.Show(
                $"ВЫ УВЕРЕНЫ, ЧТО ХОТИТЕ УДАЛИТЬ {_currentEmployee.LastName} {_currentEmployee.FirstName} {_currentEmployee.MidName}?",
                "ПОДТВЕРЖДЕНИЕ УДАЛЕНИЯ",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var groupsWithThisTeacher = _context.Groups
                        .Where(g => g.TeacherId == _currentEmployee.Id)
                        .ToList();

                    foreach (var group in groupsWithThisTeacher)
                    {
                        group.TeacherId = null;
                        _context.Entry(group).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    }
                    _context.SaveChanges();

                    _context.Employees.Remove(_currentEmployee);
                    _context.SaveChanges();

                    StatusText.Text = "СОТРУДНИК УДАЛЁН";
                    StatusText.Foreground = System.Windows.Media.Brushes.Red;

                    LoadData();
                    ClearForm();
                    _currentEmployee = null;
                    DeleteButton.Visibility = Visibility.Collapsed;
                    TitleText.Text = "ИНФОРМАЦИЯ О СОТРУДНИКЕ";
                }
                catch (Exception ex)
                {
                    StatusText.Text = $"ОШИБКА УДАЛЕНИЯ: {ex.Message}";
                    StatusText.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
        }
    }
}