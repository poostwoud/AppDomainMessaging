# AppDomainMessaging
AppDomain messaging library - First Draft

Basically the idea is that communication between application and AppDomains are equal to HTTP communication.

The AppDomainClient offers a simple set of methods (similar to HttpClient) that allows users to execute libraries in other AppDomains by using a URI's.

The client uses an internal nameserver (sort of DNS) that links host names to DLL's. After setting up your service library it is simply a matter of calling the right URI just like calling a WebAPI.

To be able to communicate AppDomainToolkit used the OperationMessaging repository. The service library requires this as well to be able to communicate. Of course this can be used in combination with facade pattern.

Please note that this is just a first draft. It still requires a lot of work. But hopefully the idea is clear! :)

Credits to Jeremy Duvall for his AppDomainToolkit which gave a lot of insights in the workings of AppDomain.

### Create a service library

The Calculator class is just an example class.

The Service class is based on the OperationService class. The OperationService class is part of the OperationMessaging library that requires to be shared in the hosted library.


```csharp
public class Calculator
{
    public int Add(int operand1, int operand2)
    {
        return operand1 + operand2;
    }

    public int Subtract(int operand1, int operand2)
    {
        return operand1 - operand2;
    }

    public int Multiply(int operand1, int operand2)
    {
        return operand1 * operand2;
    }
}

public class Service : OperationService
{
    public override OperationResponse Execute(OperationRequest request)
    {
        var types = Assembly.GetExecutingAssembly().GetTypes();

        var executionType = types.FirstOrDefault(type => type.Name.ToLower().Equals(request.ClassName.ToLower()));
        if (executionType == null) return new OperationResponse {Succes = false, Result = $"Type {request.ClassName} not found in {Assembly.GetExecutingAssembly().FullName}"};

        var executeMethod = executionType.GetMethods().FirstOrDefault(method => method.Name.ToLower().Equals(request.MethodName.ToLower()));
        if (executeMethod == null) return new OperationResponse() { Succes = false, Result = $"Method {request.MethodName} not found in class {request.ClassName}" };

        var executeParameters = executeMethod.GetParameters();
        var castValues = new List<object>();
        foreach (var executeParameter in executeParameters)
        {
            var requestValue = request.Parameters[executeParameter.Position];
            castValues.Add(Convert.ChangeType(requestValue, executeParameter.ParameterType));
        }

        var instance = Activator.CreateInstance(executionType);
        var result = executeMethod.Invoke(instance, BindingFlags.InvokeMethod, null, castValues.ToArray(), CultureInfo.InvariantCulture);

        return new OperationResponse
        {
            Succes = true,
            Result = result
        };
    }
}
```

### Console application example

After setting up the hosted library

```csharp
class Program
{
	static void Main(string[] args)
	{
		//***** Create a client;
		using (var client = new AppDomainClient())
		{
			//***** Add hostname and assembly location to nameserver;
			client.NameServer.Add("math", @"<path>\CalculatorPlugin.dll");

			//***** Calculate the sum of 40 and 2;
			var response = client.Get("adp://math/calculator/add/40/2");

			//***** Show result;
			if (response.IsSuccess)
				Console.WriteLine(response.Content.Content);

			//*****
			Console.Read();
		}
	}
}
```
