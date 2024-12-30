namespace Infra.Interfaces;

/**
 * TODO Implement this interface
 */
public interface IKeyValueStore
{
    void BeginTransaction(){ throw new NotImplementedException(); }
    void Commit() { throw new NotImplementedException(); }

    object Get(string key) { throw new NotImplementedException(); }
    void Put(string key, object value) { throw new NotImplementedException(); }
}


