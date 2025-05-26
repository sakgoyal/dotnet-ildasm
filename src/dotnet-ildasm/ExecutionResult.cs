namespace DotNet.Ildasm
{
    public class ExecutionResult(bool succeeded, string message = "")
    {
        public bool Succeeded { get; } = succeeded;

        public string Message { get; } = message;
    }
}
