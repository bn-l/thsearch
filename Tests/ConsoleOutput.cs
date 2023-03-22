namespace Tests.Helpers;

using System;
using System.IO;


class ConsoleOutput : IDisposable
{
    private StringWriter _stringWriter;
    private TextWriter _originalOutput;

    public ConsoleOutput()
    {
        _stringWriter = new StringWriter();
        _originalOutput = Console.Out;
        // now redirect output to string writer
        Console.SetOut(_stringWriter);
    }

    public string GetOutput()
    {
        return _stringWriter.ToString();
    }

    public void Dispose()
    {
        Console.SetOut(_originalOutput);
        _stringWriter.Dispose();
    }
}


