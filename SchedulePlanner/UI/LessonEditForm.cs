using System;
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

        public LessonEditForm(string title, Lesson? existing = null)
        {
            Text = title;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Width = 420;
            Height = 420;

            Result = existing != null ? Clone(existing) : new Lesson();

            BuildUi();
            LoadFromResult();
        }

        private void BuildUi()
        {
            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 9,
                Padding = new Padding(10),
                AutoSize = true
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));

            _cbDay.DropDownStyle = ComboBoxStyle.DropDownList;
            _cbDay.Items.AddRange(Enum.GetNames(typeof(DayOfWeek)));

            _cbType.DropDownStyle = ComboBoxStyle.DropDownList;
            _cbType.Items.AddRange(Enum.GetNames(typeof(LessonType)));

            _tbStart.PlaceholderText = "HH:mm";
            _tbEnd.PlaceholderText = "HH:mm";

            AddRow(table, "День:", _cbDay);
            AddRow(table, "Тип:", _cbType);
            AddRow(table, "Начало:", _tbStart);
            AddRow(table, "Конец:", _tbEnd);
            AddRow(table, "Предмет:", _tbSubject);
            AddRow(table, "Преподаватель:", _tbTeacher);
            AddRow(table, "Аудитория:", _tbRoom);
            AddRow(table, "Группа:", _tbGroup);

            var panelButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true
            };

            _btnOk.Text = "Применить";
            _btnOk.Width = 90;
            _btnOk.Click += (_, __) => OnOk();

            _btnCancel.Text = "Отменить";
            _btnCancel.Width = 90;
            _btnCancel.Click += (_, __) => { DialogResult = DialogResult.Cancel; Close(); };

            panelButtons.Controls.Add(_btnOk);
            panelButtons.Controls.Add(_btnCancel);

            table.Controls.Add(panelButtons, 0, table.RowCount - 1);
            table.SetColumnSpan(panelButtons, 2);

            Controls.Add(table);
        }

        private static void AddRow(TableLayoutPanel table, string labelText, Control input)
        {
            int row = table.Controls.Count / 2;

            if (row >= table.RowCount - 1)
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
            _cbDay.SelectedItem = Result.Day.ToString();
            _cbType.SelectedItem = Result.Type.ToString();

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
                MessageBox.Show(ex.Message, "Validation error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ApplyToResult()
        {
            Result.Day = Enum.Parse<DayOfWeek>(_cbDay.SelectedItem?.ToString() ?? DayOfWeek.Monday.ToString());
            Result.Type = Enum.Parse<LessonType>(_cbType.SelectedItem?.ToString() ?? LessonType.Lecture.ToString());

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

            throw new ArgumentException("Time must be in HH:mm format (e.g. 09:30).");
        }

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