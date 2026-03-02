using System;

namespace SchedulePlanner.Core.Models
{
    public class Lesson
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public DayOfWeek Day { get; set; }
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }

        public string Subject { get; set; } = "";
        public string Teacher { get; set; } = "";
        public string Room { get; set; } = "";
        public string Group { get; set; } = "";

        public LessonType Type { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Subject))
                throw new ArgumentException("Не указан предмет.");

            if (string.IsNullOrWhiteSpace(Teacher))
                throw new ArgumentException("Не указан преподаватель.");

            if (string.IsNullOrWhiteSpace(Room))
                throw new ArgumentException("Не указана аудитория.");

            if (string.IsNullOrWhiteSpace(Group))
                throw new ArgumentException("Не указана группа.");

            if (End <= Start)
                throw new ArgumentException("Время окончания должно быть позже времени начала.");
        }

        public bool Overlaps(Lesson other)
        {
            if (Day != other.Day) return false;

            return Start < other.End && other.Start < End;
        }
    }
}