using System;
using System.Collections.Generic;
using System.Linq;
using SchedulePlanner.Core.Models;

namespace SchedulePlanner.Core.Services
{
    public class ScheduleService
    {
        private readonly List<Lesson> _lessons;

        public ScheduleService(List<Lesson> lessons)
        {
            _lessons = lessons;
        }

        public List<Lesson> Filter(DayOfWeek? day, string query)
        {
            IEnumerable<Lesson> q = _lessons;

            if (day != null)
                q = q.Where(x => x.Day == day.Value);

            if (!string.IsNullOrWhiteSpace(query))
            {
                string s = query.ToLower();
                q = q.Where(x =>
                    x.Subject.ToLower().Contains(s) ||
                    x.Teacher.ToLower().Contains(s) ||
                    x.Group.ToLower().Contains(s));
            }

            return q.OrderBy(x => x.Day).ThenBy(x => x.Start).ToList();
        }

        public void Add(Lesson lesson)
        {
            lesson.Validate();
            EnsureNoConflict(lesson, null);
            _lessons.Add(lesson);
        }

        public void Update(Lesson lesson)
        {
            lesson.Validate();

            var existing = _lessons.First(x => x.Id == lesson.Id);
            EnsureNoConflict(lesson, lesson.Id);

            existing.Day = lesson.Day;
            existing.Start = lesson.Start;
            existing.End = lesson.End;
            existing.Subject = lesson.Subject;
            existing.Teacher = lesson.Teacher;
            existing.Room = lesson.Room;
            existing.Group = lesson.Group;
            existing.Type = lesson.Type;
        }

        public void Remove(Guid id)
        {
            _lessons.RemoveAll(x => x.Id == id);
        }

        private void EnsureNoConflict(Lesson lesson, Guid? ignoreId)
        {
            foreach (var other in _lessons)
            {
                if (ignoreId != null && other.Id == ignoreId)
                    continue;

                if (!string.Equals(other.Group, lesson.Group, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (lesson.Overlaps(other))
                    throw new InvalidOperationException(
                        $"Конфликт расписания для группы {lesson.Group}.");
            }
        }
    }
}