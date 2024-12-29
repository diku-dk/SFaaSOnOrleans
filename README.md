# SFaaSOnOrleans

SFaaSOnOrleans is a Stateful Function as a Service (SFaaS) platform on top of Microsoft Orleans.

## Table of Contents
- [Getting Started](#getting-started)
    * [Prerequisites](#prerequisites)
    * [New Orleans Users](#orleans)
    * [How to Run](#run)
- [Assignment](#assignment)
    * [Description](#description)
    * [Basic APIs](#basic-apis)
    * [Troubleshooting](#troubleshooting)

## <a name="getting-started"></a>Getting Started

### <a name="prerequisites"></a>Prerequisites

- [.NET Framework 7](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- IDE: [Visual Studio](https://visualstudio.microsoft.com/vs/community/) or [VSCode](https://code.visualstudio.com/)

### <a name="orleans"></a>New Orleans Users

[Orleans](https://learn.microsoft.com/en-us/dotnet/orleans/) framework provides facilities to program distributed applications at scale using the virtual actor model. We highly recommend starting from the [Orleans Documentation](https://learn.microsoft.com/en-us/dotnet/orleans/overview) to further understand the model.

### <a name="run"></a>How to Run

As any real-world application, we need to make sure the server is up and running before any client interation.
Therefore, in the project's root folder, run the following command:

```
dotnet run --project Server
```

This command will start up the Orleans server (aka silo).

Next, we can initialize the client program. In another console, run:

```
dotnet run --project DynamicCodeApi
```

## <a name="exercise"></a>Assignment

### <a name="description"></a>Description

In this programming task, you build upon the application scenario and your Kafka processing logic from your previous assignment. You are asked to design and implement a SFaaS platform on top of Orleans. You are provided with basic APIs to register and call functions and the remaining functionalities must be implemented, as follows.

Refer to the assignment description in Absalon for a complete

### <a name="basic-apis"></a>Basic APIs

Your client program offers an HTTP server so you can perform basci operations in your SFaaS platform.

To register a function, submit a POST request (http://localhost:5244/register) with the following payload:
```
{
    "FunctionName": "AddNumbers",
    "Code": "return (System.Int64)args[0] + (System.Int64)args[1];"
}
```

To invoke a function, submit a POST request (http://localhost:5244/execute) with the following payload:
```
{
    "FunctionName": "AddNumbers",
    "Parameters": [10, 20]
}
```

### <a name="troubleshooting"></a>Troubleshooting

**Q: There are compilation errors**

**A:** Make sure you have installed .NET Framework 7 correctly. Besides, make sure you have not modified the original code.

**Q: How to debug?**

**A:** Use an IDEA. For instance, to open the project in Visual Studio, make sure to select the BDSOnlineShop.sln as the solution file, so Visual Studio will recognize the solution as a whole and allow you to debug your application.

**Q: The project is throwing exceptions.**

**A:** You are supposed to complete the application according to the assignment description, removing the exceptions thrown in the way.
