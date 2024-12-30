using Infra.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Controller;

[ApiController]
[Route("api/[controller]")]
public class CodeController : ControllerBase
{
    private readonly IKeyValueStore kvs;

    public CodeController(IKeyValueStore kvs)
    {
        this.kvs = kvs;
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

    // Register code
    [HttpPost("register")]
    public IActionResult RegisterFunction([FromBody] CodeRegistrationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FunctionName) || string.IsNullOrWhiteSpace(request.Code))
            return BadRequest("Function name and code must be provided.");

        this.kvs.Put(request.FunctionName, request.Code);
        
        return Ok($"Function '{request.FunctionName}' registered successfully.");
    }

    // Execute code
    [HttpPost("execute")]
    public IActionResult ExecuteFunction([FromBody] FunctionExecutionRequest request)
    {
        if (this.kvs.Get(request.FunctionName) == null){
            return NotFound($"Function '{request.FunctionName}' not found.");
        }

        try
        {
            var result = this.Dispatch(request.FunctionName, request.Parameters);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error executing function: {ex}");
        }
    }

    // TODO Dispatch the function workflow
    private object Dispatch(string functionName, object[] parameters)
    {
        throw new NotImplementedException();
    }

    // TODO Register function composition
    // [HttpPost("compose")]
    // public IActionResult RegisterComposition(...

}
