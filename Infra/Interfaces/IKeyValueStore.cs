namespace Infra.Interfaces;

public interface IKeyValueStore
{
    T Get<T>(string key) { throw new NotImplementedException(); }
    bool Put<T>(string key, T value) { throw new NotImplementedException(); }

    string GetString(string key) { throw new NotImplementedException(); }
    bool PutString(string key, string value) { throw new NotImplementedException(); }

    void Reset();
}

