class ConsoleProgress : IProgress<float>
{
    private int BarWidth = 20;

    public void Report(float value)
    {
        int progress = (int)(value * 100);
        int completed = (int)(value * BarWidth);
        int remaining = BarWidth - completed;
        string progressBar = $"[{new string('#', completed)}{new string('-', remaining)}]";

        Console.Write($"\r{progressBar} {progress}%");
    }

    public void Finished()
    {
        // Clear the loading bar
    }
}