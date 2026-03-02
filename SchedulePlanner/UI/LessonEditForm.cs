using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using SchedulePlanner.Core.Models;

namespace SchedulePlanner.UI
{
    public class LessonEditForm : Form
    {
        private readonly ComboBox _cbDay = new();
        private readonly ComboBox _cbType = new();
        private readonly TextBox _tbStart = new();
        private readonly TextBox _tbEnd = new();
        private readonly TextBox _tbSubject = new();
        private readonly TextBox _tbTeacher = new();
        private readonly TextBox _tbRoom = new();
        private readonly TextBox _tbGroup = new();
        private readonly Button _btnOk = new();
        private readonly Button _btnCancel = new();

        public Lesson Result { get; private set; }

        private static readonly Dictionary<string, DayOfWeek> DayMap = new()
        {
            ["Понедельник"] = DayOfWeek.Monday,
            ["Вторник"] = DayOfWeek.Tuesday,
            ["Среда"] = DayOfWeek.Wednesday,
            ["Четверг"] = DayOfWeek.Thursday,
            ["Пятница"] = DayOfWeek.Friday,
            ["Суббота"] = DayOfWeek.Saturday,
            ["Воскресенье"] = DayOfWeek.Sunday
        };

        private static readonly Dictionary<string, LessonType> TypeMap = new()
        {
            ["Лекция"] = LessonType.Lecture,
            ["Практика"] = LessonType.Practice,
            ["Лабораторная"] = LessonType.Lab
        };

        public LessonEditForm(string title, Lesson? existing = null)
        {
            Text = title;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Width = 430;
            Height = 420;

            Result = existing != null ? Clone(existing) : new Lesson
            {
                Day = DayOfWeek.Monday,
                Type = LessonType.Lecture,
                Start = new TimeSpan(9, 0, 0),
                End = new TimeSpan(10, 30, 0)
            };

            BuildUi();
            LoadFromResult();
        }

        private void BuildUi()
        {
            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(10),
                AutoSize = true
            };

            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));

            _cbDay.DropDownStyle = ComboBoxStyle.DropDownList;
            _cbDay.Items.AddRange(new[]
            {
                "Понедельник","Вторник","Среда","Четверг","Пятница","Суббота","Воскресенье"
            });

            _cbType.DropDownStyle = ComboBoxStyle.DropDownList;
            _cbType.Items.AddRange(new[] { "Лекция", "Практика", "Лабораторная" });

            _tbStart.PlaceholderText = "ЧЧ:ММ (например 09:00)";
            _tbEnd.PlaceholderText = "ЧЧ:ММ (например 10:30)";

            AddRow(table, "День недели:", _cbDay);
            AddRow(table, "Тип занятия:", _cbType);
            AddRow(table, "Время начала:", _tbStart);
            AddRow(table, "Время окончания:", _tbEnd);
            AddRow(table, "Предмет:", _tbSubject);
            AddRow(table, "Преподаватель:", _tbTeacher);
            AddRow(table, "Аудитория:", _tbRoom);
            AddRow(table, "Группа:", _tbGroup);

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true
            };

            _btnOk.Text = "Сохранить";
            _btnOk.Width = 110;
            _btnOk.Click += (_, __) => OnOk();

            _btnCancel.Text = "Отмена";
            _btnCancel.Width = 110;
            _btnCancel.Click += (_, __) => { DialogResult = DialogResult.Cancel; Close(); };

            buttons.Controls.Add(_btnOk);
            buttons.Controls.Add(_btnCancel);

            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            table.Controls.Add(buttons, 0, table.RowCount);
            table.SetColumnSpan(buttons, 2);

            Controls.Add(table);
        }

        private static void AddRow(TableLayoutPanel table, string labelText, Control input)
        {
            int row = table.RowCount;
            table.RowCount++;

            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

            var lbl = new Label
            {
                Text = labelText,
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };

            input.Dock = DockStyle.Fill;

            table.Controls.Add(lbl, 0, row);
            table.Controls.Add(input, 1, row);
        }

        private void LoadFromResult()
        {
            _cbDay.SelectedItem = DayToRu(Result.Day);
            _cbType.SelectedItem = TypeToRu(Result.Type);

            _tbStart.Text = Result.Start.ToString(@"hh\:mm");
            _tbEnd.Text = Result.End.ToString(@"hh\:mm");

            _tbSubject.Text = Result.Subject;
            _tbTeacher.Text = Result.Teacher;
            _tbRoom.Text = Result.Room;
            _tbGroup.Text = Result.Group;
        }

        private void OnOk()
        {
            try
            {
                ApplyToResult();
                Result.Validate();

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка ввода",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ApplyToResult()
        {
            var dayStr = _cbDay.SelectedItem?.ToString();
            if (dayStr == null || !DayMap.ContainsKey(dayStr))
                throw new ArgumentException("Выберите день недели.");
            Result.Day = DayMap[dayStr];

            var typeStr = _cbType.SelectedItem?.ToString();
            if (typeStr == null || !TypeMap.ContainsKey(typeStr))
                throw new ArgumentException("Выберите тип занятия.");
            Result.Type = TypeMap[typeStr];

            Result.Start = ParseTime(_tbStart.Text);
            Result.End = ParseTime(_tbEnd.Text);

            Result.Subject = _tbSubject.Text.Trim();
            Result.Teacher = _tbTeacher.Text.Trim();
            Result.Room = _tbRoom.Text.Trim();
            Result.Group = _tbGroup.Text.Trim();
        }

        private static TimeSpan ParseTime(string text)
        {
            if (TimeSpan.TryParseExact(text.Trim(), @"hh\:mm", CultureInfo.InvariantCulture, out var t))
                return t;

            throw new ArgumentException("Время нужно вводить в формате ЧЧ:ММ (например 09:30).");
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

        private static Lesson Clone(Lesson l) => new Lesson
        {
            Id = l.Id,
            Day = l.Day,
            Start = l.Start,
            End = l.End,
            Subject = l.Subject,
            Teacher = l.Teacher,
            Room = l.Room,
            Group = l.Group,
            Type = l.Type
        };
    }
}