using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Controller;

[ApiController]
[Route("api/[controller]")]
public class CodeController : ControllerBase
{
    public static readonly ConcurrentDictionary<string, string> FunctionStore = new();
    public static readonly ConcurrentDictionary<string, (Assembly Assembly, Type Type)> CompiledCache = new();

    public class CodeRegistrationRequest
    {
        public string FunctionName { get; set; }
        public string Code { get; set; }
    }

    public class FunctionExecutionRequest
    {
        public string FunctionName { get; set; }
        public object[] Parameters { get; set; }
    }

    // Register code
    [HttpPost("register")]
    public IActionResult RegisterFunction([FromBody] CodeRegistrationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FunctionName) || string.IsNullOrWhiteSpace(request.Code))
            return BadRequest("Function name and code must be provided.");

        // Store the function code durably and invalidate the cache
        FunctionStore[request.FunctionName] = request.Code;
        CompiledCache.TryRemove(request.FunctionName, out _);

        return Ok($"Function '{request.FunctionName}' registered successfully.");
    }

    // Execute code
    [HttpPost("execute")]
    public IActionResult ExecuteFunction([FromBody] FunctionExecutionRequest request)
    {
        if (!FunctionStore.TryGetValue(request.FunctionName, out var code)){
            return NotFound($"Function '{request.FunctionName}' not found.");
        }

        try
        {
            // Dynamically compile and execute the function with registry context
            var result = CompileAndExecute(request.FunctionName, code, request.Parameters);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error executing function: {ex}");
        }
    }

    private static object CompileAndExecute(string functionName, string code, object[] parameters)
    {
        if (!CompiledCache.TryGetValue(functionName, out var cached))
        {
            // Wrap user code in a class and method
            string wrappedCode = $@"
            using System;
            using System.Collections.Concurrent;
            using System.Reflection;
            using Microsoft.AspNetCore.Mvc;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Controller;

            public class DynamicClass
            {{
                public DynamicClass(){{ }}
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
        var instance = Activator.CreateInstance(cached.Type);
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
