using System.Collections.Concurrent;
using System.Reflection;
using Infra.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;

namespace Infra.Service;

[Reentrant]
[StatelessWorker]
public class ExecutorGrain : Grain, IExecutorGrain
{
    public static readonly ConcurrentDictionary<string, (Assembly Assembly, Type Type)> CompiledCache = new();

    private readonly IKeyValueStore kvs;

    private readonly ILogger<ExecutorGrain> logger;

	public ExecutorGrain(IKeyValueStore kvs, ILogger<ExecutorGrain> logger)
	{
        this.kvs = kvs;
        this.logger = logger;
	}

    public Task Execute(string functionName, object[] parameters)
    {
        var code = (string) this.kvs.Get(functionName) ?? throw new Exception($"Function '{functionName}' not found.");
        return Task.FromResult(DoExecute(functionName, code, this.kvs, parameters));
    }

    // Dynamically compile and execute the function with registry context
    private static object DoExecute(string functionName, string code, IKeyValueStore kvs, object[] parameters)
    {
        if (!CompiledCache.TryGetValue(functionName, out var cached))
        {
            // Wrap user code in a class and method
            string wrappedCode = $@"
            using System;
            using System.Reflection;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Infra.Interfaces;

            public class DynamicClass
            {{
                private readonly IKeyValueStore kvs;
                public DynamicClass(IKeyValueStore kvs){{
                    this.kvs = kvs;
                }}
                public object Execute(params object[] args)
                {{
                    {code}
                }}
            }}";

            // Compile the assembly
            var assembly = CompileAssembly(functionName, wrappedCode);
            var type = assembly.GetType("DynamicClass");

            // Cache the compiled assembly and type
            cached = (assembly, type);
            CompiledCache[functionName] = cached;
        }

        // Use the cached assembly to execute the function
        var instance = Activator.CreateInstance(cached.Type, kvs);
        var method = cached.Type.GetMethod("Execute");
        return method.Invoke(instance, new object[] { parameters });
    }

	public static Assembly CompileAssembly(string functionName, string code)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code);

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic)
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .ToList();

        var compilation = CSharpCompilation.Create(
            $"{functionName}_Assembly",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);

        if (!result.Success)
        {
            var errors = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
            throw new InvalidOperationException(string.Join("\n", errors.Select(e => e.GetMessage())));
        }

        ms.Seek(0, SeekOrigin.Begin);
        return Assembly.Load(ms.ToArray());
    }

}
