namespace MmorpgPrototype
{
    public interface ISaveStorage
    {
        bool Exists(string slot);

        void Save(string slot, string json);

        string Load(string slot);

        void Delete(string slot);
    }
}
