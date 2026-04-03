using System.Reflection;
using ORLtrack.Tests.TestingFramework;

var assembly = Assembly.GetExecutingAssembly();
var testMethods = assembly
    .GetTypes()
    .Where(type => type.IsClass && !type.IsAbstract)
    .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        .Where(method => method.GetCustomAttribute<FactAttribute>() != null)
        .Select(method => (Type: type, Method: method)))
    .OrderBy(item => item.Type.Name)
    .ThenBy(item => item.Method.Name)
    .ToList();

var failures = new List<string>();

Console.WriteLine($"ORLtrack.Tests: найдено тестов {testMethods.Count}");

foreach (var test in testMethods)
{
    var displayName = $"{test.Type.Name}.{test.Method.Name}";

    try
    {
        var instance = Activator.CreateInstance(test.Type)
            ?? throw new InvalidOperationException($"Не удалось создать экземпляр {test.Type.Name}.");

        var result = test.Method.Invoke(instance, Array.Empty<object>());
        if (result is Task task)
        {
            await task;
        }

        Console.WriteLine($"[PASS] {displayName}");
    }
    catch (Exception ex)
    {
        var rootCause = ex is TargetInvocationException tie && tie.InnerException != null
            ? tie.InnerException
            : ex;

        failures.Add($"{displayName}: {rootCause.Message}");
        Console.WriteLine($"[FAIL] {displayName}");
        Console.WriteLine($"       {rootCause.Message}");
    }
}

if (failures.Count > 0)
{
    Console.WriteLine();
    Console.WriteLine("Сводка проваленных тестов:");
    foreach (var failure in failures)
    {
        Console.WriteLine($" - {failure}");
    }

    Environment.ExitCode = 1;
    return;
}

Console.WriteLine("Все тесты прошли успешно.");
