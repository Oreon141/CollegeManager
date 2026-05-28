using CollegeManager.DataBase;
using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace CollegeManager.Pages
{
    public partial class AcademicSheetsPage : Page
    {
        CollegeManagerEntities db = CollegeManagerEntities.GetContext();

        public AcademicSheetsPage()
        {
            InitializeComponent();
        }

        /// Загрузка страницы
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                GroupFilter.ItemsSource = db.Courses.ToList();
                SubjectFilter.ItemsSource = db.StudyModules.ToList();

                SemesterFilter.ItemsSource = new[]
                {
                    "1 семестр",
                    "2 семестр",
                    "3 семестр",
                    "4 семестр",
                    "5 семестр",
                    "6 семестр",
                    "7 семестр",
                    "8 семестр"
                };

                if (GroupFilter.Items.Count > 0)
                    GroupFilter.SelectedIndex = 0;

                if (SubjectFilter.Items.Count > 0)
                    SubjectFilter.SelectedIndex = 0;

                SemesterFilter.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// Формирование ведомости
        private void GenerateSheet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GroupFilter.SelectedItem == null ||
                    SubjectFilter.SelectedItem == null)
                {
                    MessageBox.Show("Выберите курс и модуль.");
                    return;
                }

                var selectedCourse = GroupFilter.SelectedItem as Courses;
                var selectedModule = SubjectFilter.SelectedItem as StudyModules;

                var data = db.Assignments
                    .Where(a =>
                        a.CourseID == selectedCourse.CourseID &&
                        a.ModuleID == selectedModule.ModuleID)
                    .Select(a => new
                    {
                        Задание = a.AssignmentName,

                        Описание = a.AssignmentDescription,

                        Студент =
                            a.Users != null
                            ? a.Users.Surname + " " + a.Users.FirstName
                            : "",

                        Статус = a.Statuses.StatusName,

                        Оценка = a.Weight
                    })
                    .ToList();

                AcademicGrid.ItemsSource = data;

                if (data.Count == 0)
                {
                    MessageBox.Show(
                        "Нет данных для отображения.",
                        "Информация",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// Настройка колонок
        private void AcademicGrid_AutoGeneratingColumn(
            object sender,
            DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column is DataGridTextColumn textColumn)
            {
                var style = new Style(typeof(TextBlock));

                style.Setters.Add(new Setter(
                    TextBlock.TextWrappingProperty,
                    TextWrapping.Wrap));

                style.Setters.Add(new Setter(
                    TextBlock.TextAlignmentProperty,
                    TextAlignment.Center));

                textColumn.ElementStyle = style;

                e.Column.MinWidth = 120;

                if (e.Column.Header.ToString().Contains("Описание"))
                {
                    e.Column.Width =
                        new DataGridLength(2,
                        DataGridLengthUnitType.Star);
                }
            }
        }

        /// Экспорт Excel
        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            if (AcademicGrid.ItemsSource == null)
            {
                MessageBox.Show("Сначала сформируйте ведомость.");
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "Excel файл (*.xlsx)|*.xlsx",
                FileName = $"Ведомость_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (saveDialog.ShowDialog() != true)
                return;

            try
            {
                var items = AcademicGrid.ItemsSource as IEnumerable;
                var firstItem = items.Cast<object>().FirstOrDefault();

                using (ExcelPackage package = new ExcelPackage())
                {
                    var worksheet =
                        package.Workbook.Worksheets.Add("Ведомость");

                    var properties =
                        firstItem.GetType().GetProperties();

                    int col = 1;

                    foreach (var property in properties)
                    {
                        worksheet.Cells[1, col].Value =
                            property.Name;

                        worksheet.Cells[1, col].Style.Font.Bold = true;

                        worksheet.Cells[1, col]
                            .Style.Fill.PatternType =
                            ExcelFillStyle.Solid;

                        worksheet.Cells[1, col]
                            .Style.Fill.BackgroundColor
                            .SetColor(System.Drawing.Color.LightBlue);

                        worksheet.Cells[1, col]
                            .Style.Border.BorderAround(
                            ExcelBorderStyle.Thin);

                        col++;
                    }

                    int row = 2;

                    foreach (var item in items)
                    {
                        col = 1;

                        foreach (var property in properties)
                        {
                            worksheet.Cells[row, col].Value =
                                property.GetValue(item);

                            worksheet.Cells[row, col]
                                .Style.Border.BorderAround(
                                ExcelBorderStyle.Thin);

                            col++;
                        }

                        row++;
                    }

                    worksheet.Cells.AutoFitColumns();

                    package.SaveAs(
                        new System.IO.FileInfo(saveDialog.FileName));
                }

                MessageBox.Show(
                    "Excel файл успешно сохранён.",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// Экспорт DOCX
        private void ExportDocx_Click(object sender, RoutedEventArgs e)
        {
            if (AcademicGrid.ItemsSource == null)
            {
                MessageBox.Show("Сначала сформируйте ведомость.");
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "Word документ (*.docx)|*.docx",
                FileName = $"Ведомость_{DateTime.Now:yyyyMMdd_HHmmss}.docx"
            };

            if (saveDialog.ShowDialog() != true)
                return;

            try
            {
                var items = AcademicGrid.ItemsSource as IEnumerable;
                var firstItem = items.Cast<object>().FirstOrDefault();

                using (DocX document =
                    DocX.Create(saveDialog.FileName))
                {
                    document.InsertParagraph(
                        "Учебная ведомость")
                        .Bold()
                        .FontSize(20)
                        .Alignment = Alignment.center;

                    document.InsertParagraph(
                        $"Дата: {DateTime.Now:dd.MM.yyyy HH:mm}")
                        .Italic()
                        .FontSize(12)
                        .Alignment = Alignment.center;

                    document.InsertParagraph("");

                    var properties =
                        firstItem.GetType().GetProperties();

                    var table =
                        document.AddTable(1, properties.Length);

                    for (int i = 0; i < properties.Length; i++)
                    {
                        table.Rows[0].Cells[i]
                            .Paragraphs[0]
                            .Append(properties[i].Name)
                            .Bold();
                    }

                    foreach (var item in items)
                    {
                        var row = table.InsertRow();

                        for (int i = 0; i < properties.Length; i++)
                        {
                            row.Cells[i]
                                .Paragraphs[0]
                                .Append(
                                properties[i]
                                .GetValue(item)?
                                .ToString() ?? "");
                        }
                    }

                    table.Design = TableDesign.LightGrid;

                    document.InsertTable(table);

                    document.Save();
                }

                MessageBox.Show(
                    "DOCX файл успешно сохранён.",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}