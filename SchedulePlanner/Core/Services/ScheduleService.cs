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

        public IReadOnlyList<Lesson> GetAll() => _lessons;

        public void Add(Lesson lesson)
        {
            lesson.Validate();
            EnsureNoConflict(lesson, ignoreId: null);
            _lessons.Add(lesson);
        }

        public void Update(Lesson lesson)
        {
            lesson.Validate();

            var existing = _lessons.FirstOrDefault(x => x.Id == lesson.Id);
            if (existing == null)
                throw new InvalidOperationException("Пара не найдена.");

            EnsureNoConflict(lesson, ignoreId: lesson.Id);

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
            var existing = _lessons.FirstOrDefault(x => x.Id == id);
            if (existing != null)
                _lessons.Remove(existing);
        }

        public List<Lesson> Filter(DayOfWeek? day, string query)
        {
            IEnumerable<Lesson> q = _lessons;

            if (day != null)
                q = q.Where(x => x.Day == day.Value);

            if (!string.IsNullOrWhiteSpace(query))
            {
                string s = query.Trim().ToLowerInvariant();
                q = q.Where(x =>
                    x.Subject.ToLowerInvariant().Contains(s) ||
                    x.Teacher.ToLowerInvariant().Contains(s) ||
                    x.Room.ToLowerInvariant().Contains(s) ||
                    x.Group.ToLowerInvariant().Contains(s));
            }

            return q.OrderBy(x => x.Day).ThenBy(x => x.Start).ToList();
        }

        private void EnsureNoConflict(Lesson lesson, Guid? ignoreId)
        {
            foreach (var other in _lessons)
            {
                if (ignoreId != null && other.Id == ignoreId.Value)
                    continue;

                if (!string.Equals(other.Group, lesson.Group, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (lesson.Overlaps(other))
                {
                    throw new InvalidOperationException(
                        $"Conflict for group '{lesson.Group}' on {lesson.Day}: " +
                        $"{lesson.Start:hh\\:mm}-{lesson.End:hh\\:mm} overlaps " +
                        $"{other.Start:hh\\:mm}-{other.End:hh\\:mm} ({other.Subject}).");
                }
            }
        }
    }
}