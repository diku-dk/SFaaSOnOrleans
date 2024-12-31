using DynamicCodeApi;
using Infra.Interfaces;
using Infra.Service;
using Microsoft.AspNetCore.Mvc;

namespace Controller;

[ApiController]
[Route("api/[controller]")]
public class CodeController : ControllerBase
{
    private readonly IKeyValueStore kvs;
    private readonly IClusterClient client;

    public CodeController(IKeyValueStore kvs)
    {
        this.kvs = kvs;
        this.client = OrleansClientManager.GetClient().Result;
    }

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

    // Register function
    [HttpPost("register")]
    public IActionResult RegisterFunction([FromBody] CodeRegistrationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FunctionName) || string.IsNullOrWhiteSpace(request.Code))
            return BadRequest("Function name and code must be provided.");

        this.kvs.Put(request.FunctionName, request.Code);
        
        return Ok($"Function '{request.FunctionName}' registered successfully.");
    }

    // TODO Register function composition
    [HttpPost("compose")]
    public IActionResult RegisterComposition()
    {
        throw new NotImplementedException();
    }

    // Execute function
    [HttpPost("execute")]
    public async Task<IActionResult> ExecuteFunction([FromBody] FunctionExecutionRequest request)
    {
        if (this.kvs.GetString(request.FunctionName) == null){
            return NotFound($"Function '{request.FunctionName}' not found.");
        }
        try
        {
            var result = await this.Dispatch(request.FunctionName, request.Parameters);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error executing function: {ex}");
        }
    }

    // Test function
    [HttpPost("test")]
    public async Task<IActionResult> TestFunction([FromBody] FunctionExecutionRequest request)
    {
        if (this.kvs.GetString(request.FunctionName) == null){
            return NotFound($"Function '{request.FunctionName}' not found.");
        }
        try
        {
            // Example below should only be used for testing purposes
            var worker = this.client.GetGrain<IExecutorGrain>(0);
            return Ok(await worker.Execute(request.FunctionName, request.Parameters));
        }
        catch (Exception ex)
        {
            return BadRequest($"Error executing function: {ex}");
        }
    }

    // Dispatch the function workflow request for execution
    private Task<object> Dispatch(string functionName, object[] parameters)
    {
        // TODO pick a well-defind MediatorGrain and call InitWorkflow
        throw new NotImplementedException();
    }

}
