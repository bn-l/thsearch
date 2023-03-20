namespace thsearch;
using System;
using System.Collections.Generic;

public class ConsoleUI
{
    private int loadingCount;

    public void UpdateLoadCount(int count)
    {
        loadingCount = count;
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write($"File Queue: {loadingCount}");
    }

    public void DisplayResults(List<string> results)
    {
        Console.Clear();
        Console.WriteLine("Results:");
        Console.WriteLine();

        int index = 0;
        int pageSize = Console.WindowHeight - 3;

        while (true)
        {
            for (int i = 0; i < pageSize && index < results.Count; i++, index++)
            {
                Console.WriteLine(results[index]);
            }

            Console.SetCursorPosition(0, Console.WindowHeight - 2);
            Console.Write($"Page {index / pageSize + 1} / {results.Count / pageSize + 1}     ");

            ConsoleKeyInfo key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.UpArrow)
            {
                if (index > 0)
                {
                    index--;
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                }
            }
            else if (key.Key == ConsoleKey.DownArrow)
            {
                if (index < results.Count - 1)
                {
                    index++;
                    Console.SetCursorPosition(0, Console.CursorTop + 1);
                }
            }
            else if (key.Key == ConsoleKey.LeftArrow)
            {
                index -= pageSize;

                if (index < 0)
                {
                    index = 0;
                }
            }
            else if (key.Key == ConsoleKey.RightArrow)
            {
                index += pageSize;

                if (index >= results.Count)
                {
                    index = results.Count - 1;
                }
            }
            else if (key.Key == ConsoleKey.Escape || key.Modifiers == ConsoleModifiers.Control && key.Key == ConsoleKey.C)
            {
                break;
            }
        }

        Console.Clear();
        UpdateLoadCount(loadingCount);
    }
}
