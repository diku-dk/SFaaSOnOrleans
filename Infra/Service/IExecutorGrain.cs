namespace Infra.Service;

public interface IExecutorGrain : IGrainWithIntegerKey
{
    Task<object> Execute(string functionName, object[] parameters);
}

