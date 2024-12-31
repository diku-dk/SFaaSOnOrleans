namespace Infra.Interfaces;

public interface IKeyValueStore
{
    T Get<T>(string key) { throw new NotImplementedException(); }
    string GetString(string key) { throw new NotImplementedException(); }
    void Put<T>(string key, T value) { throw new NotImplementedException(); }
}

