using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SchedulePlanner.Core.Models;
using SchedulePlanner.Core.Services;
using SchedulePlanner.Core.Storage;

namespace SchedulePlanner.UI
{
    public class MainForm : Form
    {
        private readonly IStorage _storage = new JsonStorage();
        private AppState _state = new AppState();
        private ScheduleService _service;

        private readonly DataGridView _grid = new();
        private readonly ComboBox _cbDay = new();
        private readonly TextBox _tbSearch = new();

        private readonly Button _btnAdd = new();
        private readonly Button _btnEdit = new();
        private readonly Button _btnDelete = new();
        private readonly Button _btnSave = new();
        private readonly Button _btnLoad = new();

        private const string FilePath = "расписание.json";

        private static readonly Dictionary<string, DayOfWeek?> DayMap = new()
        {
            ["Все"] = null,
            ["Понедельник"] = DayOfWeek.Monday,
            ["Вторник"] = DayOfWeek.Tuesday,
            ["Среда"] = DayOfWeek.Wednesday,
            ["Четверг"] = DayOfWeek.Thursday,
            ["Пятница"] = DayOfWeek.Friday,
            ["Суббота"] = DayOfWeek.Saturday,
            ["Воскресенье"] = DayOfWeek.Sunday
        };

        public MainForm()
        {
            Text = "Планировщик учебного расписания";
            Width = 1000;
            Height = 600;
            StartPosition = FormStartPosition.CenterScreen;

            _service = new ScheduleService(_state.Lessons);

            BuildUI();
            SetupGrid();
            RefreshGrid();
        }

        private void BuildUI()
        {
            var top = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 48,
                Padding = new Padding(8),
                WrapContents = false,
                AutoSize = false
            };

            var lblDay = new Label { Text = "День:", AutoSize = true, Margin = new Padding(0, 10, 6, 0) };

            _cbDay.DropDownStyle = ComboBoxStyle.DropDownList;
            _cbDay.Width = 160;
            _cbDay.Margin = new Padding(0, 6, 18, 0);
            _cbDay.Items.AddRange(DayMap.Keys.ToArray());
            _cbDay.SelectedItem = "Все";
            _cbDay.SelectedIndexChanged += (_, __) => RefreshGrid();

            var lblSearch = new Label { Text = "Поиск:", AutoSize = true, Margin = new Padding(0, 10, 6, 0) };

            _tbSearch.Width = 220;
            _tbSearch.Margin = new Padding(0, 6, 18, 0);
            _tbSearch.TextChanged += (_, __) => RefreshGrid();

            ConfigureButton(_btnAdd, "Добавить", OnAdd);
            ConfigureButton(_btnEdit, "Изменить", OnEdit);
            ConfigureButton(_btnDelete, "Удалить", OnDelete);
            ConfigureButton(_btnSave, "Сохранить", OnSave);
            ConfigureButton(_btnLoad, "Загрузить", OnLoad);

            top.Controls.Add(lblDay);
            top.Controls.Add(_cbDay);
            top.Controls.Add(lblSearch);
            top.Controls.Add(_tbSearch);
            top.Controls.Add(_btnAdd);
            top.Controls.Add(_btnEdit);
            top.Controls.Add(_btnDelete);
            top.Controls.Add(_btnSave);
            top.Controls.Add(_btnLoad);

            _grid.Dock = DockStyle.Fill;
            _grid.ReadOnly = true;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.MultiSelect = false;
            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _grid.CellDoubleClick += (_, __) => OnEdit();

            Controls.Add(_grid);
            Controls.Add(top);
        }

        private static void ConfigureButton(Button b, string text, Action onClick)
        {
            b.Text = text;
            b.Width = 90;
            b.Height = 28;
            b.Margin = new Padding(0, 6, 8, 0);
            b.Click += (_, __) => onClick();
        }

        private void SetupGrid()
        {
            _grid.Columns.Clear();

            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", Visible = false });

            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Day", HeaderText = "День" });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Start", HeaderText = "Начало" });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "End", HeaderText = "Окончание" });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Type", HeaderText = "Тип занятия" });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Subject", HeaderText = "Предмет" });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Teacher", HeaderText = "Преподаватель" });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Room", HeaderText = "Аудитория" });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Group", HeaderText = "Группа" });
        }

        private void RefreshGrid()
        {
            var selected = _cbDay.SelectedItem?.ToString() ?? "Все";
            DayOfWeek? day = DayMap.TryGetValue(selected, out var d) ? d : null;

            string query = _tbSearch.Text;

            List<Lesson> data = _service.Filter(day, query);

            _grid.Rows.Clear();
            foreach (var l in data)
            {
                _grid.Rows.Add(
                    l.Id.ToString(),
                    DayToRu(l.Day),
                    l.Start.ToString(@"hh\:mm"),
                    l.End.ToString(@"hh\:mm"),
                    TypeToRu(l.Type),
                    l.Subject,
                    l.Teacher,
                    l.Room,
                    l.Group
                );
            }
        }

        private static string DayToRu(DayOfWeek day) => day switch
        {
            DayOfWeek.Monday => "Понедельник",
            DayOfWeek.Tuesday => "Вторник",
            DayOfWeek.Wednesday => "Среда",
            DayOfWeek.Thursday => "Четверг",
            DayOfWeek.Friday => "Пятница",
            DayOfWeek.Saturday => "Суббота",
            DayOfWeek.Sunday => "Воскресенье",
            _ => day.ToString()
        };

        private static string TypeToRu(LessonType t) => t switch
        {
            LessonType.Lecture => "Лекция",
            LessonType.Practice => "Практика",
            LessonType.Lab => "Лабораторная",
            _ => t.ToString()
        };

        private Lesson? GetSelectedLesson()
        {
            if (_grid.SelectedRows.Count == 0) return null;

            var idStr = _grid.SelectedRows[0].Cells["Id"].Value?.ToString();
            if (idStr == null) return null;

            if (!Guid.TryParse(idStr, out var id)) return null;
            return _state.Lessons.FirstOrDefault(x => x.Id == id);
        }

        private void OnAdd()
        {
            var dlg = new LessonEditForm("Добавить пару");
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                _service.Add(dlg.Result);
                RefreshGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnEdit()
        {
            var selected = GetSelectedLesson();
            if (selected == null)
            {
                MessageBox.Show("Сначала выберите пару в таблице.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var dlg = new LessonEditForm("Изменить пару", selected);
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                _service.Update(dlg.Result);
                RefreshGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnDelete()
        {
            var selected = GetSelectedLesson();
            if (selected == null)
            {
                MessageBox.Show("Сначала выберите пару в таблице.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var ok = MessageBox.Show("Удалить выбранную пару?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (ok != DialogResult.Yes) return;

            _service.Remove(selected.Id);
            RefreshGrid();
        }

        private void OnSave()
        {
            try
            {
                _storage.Save(FilePath, _state);
                MessageBox.Show($"Расписание сохранено в файл: {FilePath}", "Сохранение",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка сохранения", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnLoad()
        {
            try
            {
                _state = _storage.Load(FilePath);
                _service = new ScheduleService(_state.Lessons);
                RefreshGrid();
                MessageBox.Show($"Расписание загружено из файла: {FilePath}", "Загрузка",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка загрузки", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}