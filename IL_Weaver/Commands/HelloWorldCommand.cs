using CommandLine;

namespace IL_Weaver.Commands;

[Verb("print", HelpText = "Print provided message or 'Hello World' to the console.")]
public class HelloWorldCommand
{
    [Option('m', "message", Default = "Hello world!", Required = false, HelpText = "Message to print.")]
    public string Message { get; set; }
}