using ChildPrison.Models;
using diplomApp.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace diplomApp.View.Nutrition
{
    public partial class MenuManagementPage : Page
    {
        private DataContext _context;
        private List<Dish> _allDishes;

        public MenuManagementPage()
        {
            InitializeComponent();
        }

        private void DayOfWeekComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}