using ChildPrison.Models;
using diplomApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Fonts;
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
            Loaded += MenuManagementPage_Loaded;
        }

        private void MenuManagementPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
            DayOfWeekComboBox.SelectedIndex = 0;
        }

        private void LoadData()
        {
            try
            {
                _context = new DataContext();

                _allDishes = _context.Dishes
                    .Include(d => d.DishProducts)
                    .ThenInclude(dp => dp.Product)
                    .ToList();

                var dishesWithEmpty = new List<Dish> { new Dish { Id = 0, DishName = "-- ВЫБЕРИТЕ БЛЮДО --" } };
                dishesWithEmpty.AddRange(_allDishes);

                FillComboBoxes(dishesWithEmpty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillComboBoxes(List<Dish> dishes)
        {
            Breakfast1ComboBox.ItemsSource = dishes;
            Breakfast2ComboBox.ItemsSource = dishes;
            Breakfast3ComboBox.ItemsSource = dishes;
            SecondBreakfast1ComboBox.ItemsSource = dishes;
            SecondBreakfast2ComboBox.ItemsSource = dishes;
            SecondBreakfast3ComboBox.ItemsSource = dishes;
            Lunch1ComboBox.ItemsSource = dishes;
            Lunch2ComboBox.ItemsSource = dishes;
            Lunch3ComboBox.ItemsSource = dishes;
            Snack1ComboBox.ItemsSource = dishes;
            Snack2ComboBox.ItemsSource = dishes;
            Snack3ComboBox.ItemsSource = dishes;
        }

        private void DayOfWeekComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DayOfWeekComboBox.SelectedItem is ComboBoxItem selectedDay)
            {
                LoadMenuForDay(selectedDay.Tag.ToString());
            }
        }

        private void LoadMenuForDay(string dayNumber)
        {
            try
            {
                ClearAllComboBoxes();

                var menuItems = _context.Set<global::ChildPrison.Models.Menu>()
                    .Where(m => m.DayOfWeek == dayNumber)
                    .ToList();

                foreach (var item in menuItems)
                {
                    SetComboBoxValue(item.MealTime, item.Order, item.DishId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки меню: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearAllComboBoxes()
        {
            Breakfast1ComboBox.SelectedIndex = 0;
            Breakfast2ComboBox.SelectedIndex = 0;
            Breakfast3ComboBox.SelectedIndex = 0;
            SecondBreakfast1ComboBox.SelectedIndex = 0;
            SecondBreakfast2ComboBox.SelectedIndex = 0;
            SecondBreakfast3ComboBox.SelectedIndex = 0;
            Lunch1ComboBox.SelectedIndex = 0;
            Lunch2ComboBox.SelectedIndex = 0;
            Lunch3ComboBox.SelectedIndex = 0;
            Snack1ComboBox.SelectedIndex = 0;
            Snack2ComboBox.SelectedIndex = 0;
            Snack3ComboBox.SelectedIndex = 0;
        }

        private void SetComboBoxValue(string mealTime, int order, int? dishId)
        {
            if (dishId == null || dishId == 0) return;

            ComboBox targetComboBox = null;

            switch (mealTime)
            {
                case "ЗАВТРАК":
                    if (order == 1) targetComboBox = Breakfast1ComboBox;
                    else if (order == 2) targetComboBox = Breakfast2ComboBox;
                    else if (order == 3) targetComboBox = Breakfast3ComboBox;
                    break;
                case "ВТОРОЙ ЗАВТРАК":
                    if (order == 1) targetComboBox = SecondBreakfast1ComboBox;
                    else if (order == 2) targetComboBox = SecondBreakfast2ComboBox;
                    else if (order == 3) targetComboBox = SecondBreakfast3ComboBox;
                    break;
                case "ОБЕД":
                    if (order == 1) targetComboBox = Lunch1ComboBox;
                    else if (order == 2) targetComboBox = Lunch2ComboBox;
                    else if (order == 3) targetComboBox = Lunch3ComboBox;
                    break;
                case "ПОЛДНИК":
                    if (order == 1) targetComboBox = Snack1ComboBox;
                    else if (order == 2) targetComboBox = Snack2ComboBox;
                    else if (order == 3) targetComboBox = Snack3ComboBox;
                    break;
            }

            if (targetComboBox != null)
            {
                targetComboBox.SelectedValue = dishId;
            }
        }

        private void SaveMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DayOfWeekComboBox.SelectedItem is not ComboBoxItem selectedDay)
                {
                    MessageBox.Show("Выберите день недели!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string dayNumber = selectedDay.Tag.ToString();

                var oldMenu = _context.Set<global::ChildPrison.Models.Menu>()
                    .Where(m => m.DayOfWeek == dayNumber)
                    .ToList();
                _context.Set<global::ChildPrison.Models.Menu>().RemoveRange(oldMenu);

                var newMenuItems = new List<global::ChildPrison.Models.Menu>();

                AddMenuItem(newMenuItems, dayNumber, Breakfast1ComboBox, "ЗАВТРАК", 1);
                AddMenuItem(newMenuItems, dayNumber, Breakfast2ComboBox, "ЗАВТРАК", 2);
                AddMenuItem(newMenuItems, dayNumber, Breakfast3ComboBox, "ЗАВТРАК", 3);
                AddMenuItem(newMenuItems, dayNumber, SecondBreakfast1ComboBox, "ВТОРОЙ ЗАВТРАК", 1);
                AddMenuItem(newMenuItems, dayNumber, SecondBreakfast2ComboBox, "ВТОРОЙ ЗАВТРАК", 2);
                AddMenuItem(newMenuItems, dayNumber, SecondBreakfast3ComboBox, "ВТОРОЙ ЗАВТРАК", 3);
                AddMenuItem(newMenuItems, dayNumber, Lunch1ComboBox, "ОБЕД", 1);
                AddMenuItem(newMenuItems, dayNumber, Lunch2ComboBox, "ОБЕД", 2);
                AddMenuItem(newMenuItems, dayNumber, Lunch3ComboBox, "ОБЕД", 3);
                AddMenuItem(newMenuItems, dayNumber, Snack1ComboBox, "ПОЛДНИК", 1);
                AddMenuItem(newMenuItems, dayNumber, Snack2ComboBox, "ПОЛДНИК", 2);
                AddMenuItem(newMenuItems, dayNumber, Snack3ComboBox, "ПОЛДНИК", 3);

                _context.Set<global::ChildPrison.Models.Menu>().AddRange(newMenuItems);
                _context.SaveChanges();

                MessageBox.Show("Меню успешно сохранено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddMenuItem(List<global::ChildPrison.Models.Menu> menuList, string dayNumber, ComboBox comboBox, string mealTime, int order)
        {
            if (comboBox.SelectedItem is Dish selectedDish && selectedDish.Id > 0)
            {
                menuList.Add(new global::ChildPrison.Models.Menu
                {
                    DayOfWeek = dayNumber,
                    MealTime = mealTime,
                    Order = order,
                    DishId = selectedDish.Id
                });
            }
        }

        private void ExportToPdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DayOfWeekComboBox.SelectedItem is not ComboBoxItem selectedDay)
                {
                    MessageBox.Show("Выберите день недели!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string dayName = (selectedDay.Content as string) ?? "МЕНЮ";

                var menuData = new Dictionary<string, List<DishInfo>>();
                var mealTimes = new[] { "ЗАВТРАК", "ВТОРОЙ ЗАВТРАК", "ОБЕД", "ПОЛДНИК" };

                foreach (var mealTime in mealTimes)
                {
                    var dishes = new List<DishInfo>();
                    for (int order = 1; order <= 3; order++)
                    {
                        ComboBox comboBox = GetComboBoxByMealTime(mealTime, order);
                        if (comboBox.SelectedItem is Dish selectedDish && selectedDish.Id > 0)
                        {
                            var dish = _context.Dishes
                                .Include(d => d.DishProducts)
                                .ThenInclude(dp => dp.Product)
                                .FirstOrDefault(d => d.Id == selectedDish.Id);

                            dishes.Add(new DishInfo
                            {
                                Name = dish?.DishName ?? "",
                                Ingredients = dish?.IngredientsList ?? "НЕТ ИНГРЕДИЕНТОВ"
                            });
                        }
                        else
                        {
                            dishes.Add(new DishInfo { Name = "НЕ ВЫБРАНО", Ingredients = "" });
                        }
                    }
                    menuData[mealTime] = dishes;
                }

                var saveDialog = new SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    DefaultExt = ".pdf",
                    FileName = $"Меню_{dayName}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    GeneratePdf(saveDialog.FileName, dayName, menuData);
                    MessageBox.Show($"PDF успешно сохранён!\n{saveDialog.FileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private ComboBox GetComboBoxByMealTime(string mealTime, int order)
        {
            return (mealTime, order) switch
            {
                ("ЗАВТРАК", 1) => Breakfast1ComboBox,
                ("ЗАВТРАК", 2) => Breakfast2ComboBox,
                ("ЗАВТРАК", 3) => Breakfast3ComboBox,
                ("ВТОРОЙ ЗАВТРАК", 1) => SecondBreakfast1ComboBox,
                ("ВТОРОЙ ЗАВТРАК", 2) => SecondBreakfast2ComboBox,
                ("ВТОРОЙ ЗАВТРАК", 3) => SecondBreakfast3ComboBox,
                ("ОБЕД", 1) => Lunch1ComboBox,
                ("ОБЕД", 2) => Lunch2ComboBox,
                ("ОБЕД", 3) => Lunch3ComboBox,
                ("ПОЛДНИК", 1) => Snack1ComboBox,
                ("ПОЛДНИК", 2) => Snack2ComboBox,
                ("ПОЛДНИК", 3) => Snack3ComboBox,
                _ => null
            };
        }

        private void GeneratePdf(string filePath, string dayName, Dictionary<string, List<DishInfo>> menuData)
        {
            GlobalFontSettings.UseWindowsFontsUnderWindows = true;

            using (var document = new PdfDocument())
            {
                var page = document.AddPage();
                page.Width = XUnit.FromMillimeter(210);
                page.Height = XUnit.FromMillimeter(297);

                using (var gfx = XGraphics.FromPdfPage(page))
                {
                    var titleFont = new XFont("Arial", 18, XFontStyleEx.Bold);
                    var headerFont = new XFont("Arial", 11, XFontStyleEx.Bold);
                    var regularFont = new XFont("Arial", 10, XFontStyleEx.Regular);
                    var boldFont = new XFont("Arial", 10, XFontStyleEx.Bold);
                    var smallFont = new XFont("Arial", 9, XFontStyleEx.Regular);

                    double yPosition = 40;
                    double leftMargin = 30;
                    double rightMargin = 30;
                    double pageWidth = page.Width.Point;

                    gfx.DrawLine(new XPen(XColor.FromArgb(0, 0, 0), 2), leftMargin, yPosition, pageWidth - rightMargin, yPosition);
                    yPosition += 15;

                    gfx.DrawString("ДЕТСКОЕ УЧРЕЖДЕНИЕ", titleFont, XBrushes.Black,
                        new XRect(0, yPosition, pageWidth, 25), XStringFormats.TopCenter);
                    yPosition += 30;

                    gfx.DrawString($"МЕНЮ НА {dayName}", titleFont, XBrushes.Black,
                        new XRect(0, yPosition, pageWidth, 25), XStringFormats.TopCenter);
                    yPosition += 35;

                    var dateText = $"Дата формирования: {DateTime.Now:dd.MM.yyyy}";
                    gfx.DrawString(dateText, regularFont, XBrushes.Black,
                        pageWidth - rightMargin - gfx.MeasureString(dateText, regularFont).Width, yPosition);
                    yPosition += 25;

                    gfx.DrawLine(new XPen(XColor.FromArgb(0, 0, 0), 1), leftMargin, yPosition, pageWidth - rightMargin, yPosition);
                    yPosition += 15;

                    double cellPadding = 5;
                    double[] columnWidths = { 75, 155, 210 };
                    double rowHeight = 30;
                    double headerHeight = 32;
                    double xPos = leftMargin;

                    var headerColor = XColor.FromArgb(240, 240, 240);
                    var borderPen = new XPen(XColor.FromArgb(80, 80, 80), 0.8);
                    var lightBorderPen = new XPen(XColor.FromArgb(200, 200, 200), 0.5);
                    var grayBrush = new XSolidBrush(XColor.FromArgb(100, 100, 100));
                    var headerBrush = new XSolidBrush(headerColor);

                    var centerFormat = new XStringFormat();
                    centerFormat.Alignment = XStringAlignment.Center;
                    centerFormat.LineAlignment = XLineAlignment.Center;

                    var leftCenterFormat = new XStringFormat();
                    leftCenterFormat.Alignment = XStringAlignment.Near;
                    leftCenterFormat.LineAlignment = XLineAlignment.Center;

                    string[] headers = { "ПРИЁМ ПИЩИ", "БЛЮДО", "СОСТАВ" };
                    xPos = leftMargin;

                    for (int i = 0; i < headers.Length; i++)
                    {
                        var headerRect = new XRect(xPos, yPosition, columnWidths[i], headerHeight);
                        gfx.DrawRectangle(headerBrush, headerRect);
                        gfx.DrawRectangle(borderPen, headerRect);
                        gfx.DrawString(headers[i], headerFont, XBrushes.Black, headerRect, centerFormat);
                        xPos += columnWidths[i];
                    }
                    yPosition += headerHeight;

                    foreach (var meal in menuData)
                    {
                        int rowSpan = meal.Value.Count;
                        bool firstRow = true;

                        double maxRowHeight = rowHeight;

                        var rowHeights = new List<double>();
                        foreach (var dish in meal.Value)
                        {
                            double dishHeight = CalculateTextHeight(dish.Name, regularFont, columnWidths[1] - cellPadding * 2);
                            double ingrHeight = CalculateTextHeight(dish.Ingredients, smallFont, columnWidths[2] - cellPadding * 2);
                            double currentHeight = Math.Max(dishHeight, ingrHeight) + cellPadding * 2;
                            rowHeights.Add(Math.Max(currentHeight, rowHeight));
                        }

                        for (int idx = 0; idx < meal.Value.Count; idx++)
                        {
                            var dish = meal.Value[idx];
                            double currentRowHeight = rowHeights[idx];
                            xPos = leftMargin;

                            if (firstRow)
                            {
                                double totalHeight = rowHeights.Sum();
                                var mealRect = new XRect(xPos, yPosition, columnWidths[0], totalHeight);
                                gfx.DrawRectangle(borderPen, mealRect);

                                var mealLines = WrapText(meal.Key, regularFont, columnWidths[0] - cellPadding * 2);
                                double totalTextHeight = mealLines.Count * regularFont.GetHeight();
                                double mealTextY = mealRect.Y + (mealRect.Height - totalTextHeight) / 2;
                                foreach (var line in mealLines)
                                {
                                    gfx.DrawString(line, regularFont, XBrushes.Black,
                                        mealRect.X + cellPadding, mealTextY);
                                    mealTextY += regularFont.GetHeight();
                                }
                                firstRow = false;
                            }

                            var dishRect = new XRect(xPos + columnWidths[0], yPosition, columnWidths[1], currentRowHeight);
                            gfx.DrawRectangle(lightBorderPen, dishRect);

                            var dishLines = WrapText(dish.Name, regularFont, columnWidths[1] - cellPadding * 2);
                            double totalDishHeight = dishLines.Count * regularFont.GetHeight();
                            double dishY = dishRect.Y + (dishRect.Height - totalDishHeight) / 2;
                            foreach (var line in dishLines)
                            {
                                gfx.DrawString(line, dish.Name == "НЕ ВЫБРАНО" ? regularFont : boldFont, XBrushes.Black,
                                    dishRect.X + cellPadding, dishY);
                                dishY += regularFont.GetHeight();
                            }

                            var ingrRect = new XRect(xPos + columnWidths[0] + columnWidths[1], yPosition, columnWidths[2], currentRowHeight);
                            gfx.DrawRectangle(lightBorderPen, ingrRect);

                            var ingrLines = WrapText(dish.Ingredients, smallFont, columnWidths[2] - cellPadding * 2);
                            double totalIngrHeight = ingrLines.Count * smallFont.GetHeight();
                            double ingrY = ingrRect.Y + (ingrRect.Height - totalIngrHeight) / 2;
                            foreach (var line in ingrLines)
                            {
                                if (line.Length > 0)
                                {
                                    var brush = line == "—" ? grayBrush : XBrushes.Black;
                                    gfx.DrawString(line, smallFont, brush,
                                        ingrRect.X + cellPadding, ingrY);
                                    ingrY += smallFont.GetHeight();
                                }
                            }

                            yPosition += currentRowHeight;
                        }
                    }

                    yPosition += 15;

                    gfx.DrawLine(new XPen(XColor.FromArgb(0, 0, 0), 1), leftMargin, yPosition, pageWidth - rightMargin, yPosition);
                    yPosition += 20;

                    double signatureY = page.Height.Point - 50;
                    gfx.DrawString("Заведующий: ___________________", regularFont, XBrushes.Black,
                        leftMargin, signatureY);
                    gfx.DrawString("Шеф-повар: ___________________", regularFont, XBrushes.Black,
                        pageWidth - rightMargin - 160, signatureY);
                }

                document.Save(filePath);
            }
        }

        private double CalculateTextHeight(string text, XFont font, double maxWidth)
        {
            if (string.IsNullOrEmpty(text)) return 0;

            var lines = WrapText(text, font, maxWidth);
            return lines.Count * font.GetHeight();
        }

        private List<string> WrapText(string text, XFont font, double maxWidth)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(text))
            {
                result.Add("—");
                return result;
            }

            using (var tempGfx = XGraphics.CreateMeasureContext(new XSize(1000, 1000), XGraphicsUnit.Point, XPageDirection.Downwards))
            {
                var totalSize = tempGfx.MeasureString(text, font);
                if (totalSize.Width <= maxWidth)
                {
                    result.Add(text);
                    return result;
                }
            }

            var words = text.Split(' ');
            var currentLine = "";

            using (var tempGfx = XGraphics.CreateMeasureContext(new XSize(1000, 1000), XGraphicsUnit.Point, XPageDirection.Downwards))
            {
                foreach (var word in words)
                {
                    var testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
                    var size = tempGfx.MeasureString(testLine, font);

                    if (size.Width <= maxWidth)
                    {
                        currentLine = testLine;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(currentLine))
                        {
                            result.Add(currentLine);
                        }
                        currentLine = word;
                    }
                }

                if (!string.IsNullOrEmpty(currentLine))
                {
                    result.Add(currentLine);
                }
            }

            if (result.Count == 0)
            {
                result.Add("—");
            }

            return result;
        }

        public class DishInfo
        {
            public string Name { get; set; }
            public string Ingredients { get; set; }
        }
    }
}
