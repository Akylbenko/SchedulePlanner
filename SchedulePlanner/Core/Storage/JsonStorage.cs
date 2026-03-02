using System.IO;
using System.Text.Json;

namespace SchedulePlanner.Core.Storage
{
    public class JsonStorage : IStorage
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true
        };

        public void Save(string path, AppState state)
        {
            var json = JsonSerializer.Serialize(state, Options);
            File.WriteAllText(path, json);
        }

        public AppState Load(string path)
        {
            if (!File.Exists(path))
                return new AppState();

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<AppState>(json, Options) ?? new AppState();
        }
    }
}