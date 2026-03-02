namespace SchedulePlanner.Core.Storage
{
    public interface IStorage
    {
        void Save(string path, AppState state);
        AppState Load(string path);
    }
}