using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sharpie;

public class ConsoleUI
{
    private int loadingCount = 0;
    private readonly SharpieWrapper sharpie;

    public ConsoleUI()
    {
        sharpie = new SharpieWrapper();
    }

    public void UpdateLoadCount(int count)
    {
        loadingCount = count;
        sharpie.MoveCursorUp();
        sharpie.EraseLine();
        sharpie.WriteLine($"File Queue: {loadingCount}");
    }

    public void DisplayResults(List<string> results)
    {
        sharpie.Clear();
        foreach (string result in results)
        {
            sharpie.WriteLine(result);
        }
        sharpie.WaitForKey(ConsoleKey.UpArrow, ConsoleKey.DownArrow);
    }
}

public class SharpieWrapper
{
    public void WriteLine(string value)
    {
        Console.WriteLine(value);
    }

    public void MoveCursorUp()
    {
        Console.CursorTop--;
    }

    public void EraseLine()
    {
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, Console.CursorTop);
    }

    public void Clear()
    {
        Console.Clear();
    }

    public void WaitForKey(params ConsoleKey[] keys)
    {
        Task.Run(() =>
        {
            ConsoleKeyInfo keyInfo;
            do
            {
                keyInfo = Console.ReadKey(true);
            } while (Array.IndexOf(keys, keyInfo.Key) == -1);
        }).Wait();
    }
}
