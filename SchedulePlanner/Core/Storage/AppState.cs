using System.Collections.Generic;
using SchedulePlanner.Core.Models;

namespace SchedulePlanner.Core.Storage
{
    public class AppState
    {
        public List<Lesson> Lessons { get; set; } = new();
    }
}