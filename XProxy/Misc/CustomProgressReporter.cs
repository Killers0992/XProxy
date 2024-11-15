using XProxy.Misc;

public class CustomProgressReporter : IProgress<float>
{
    private int current = 0;

    public CustomProgressReporter(string text, string tag)
    {
        Text = text;
    }

    public string Text { get; }

    public void Report(float value)
    {
        int percentage = (int)(100 * value) / 1;

        int reportEveryNum = percentage/20;

        if (current != reportEveryNum) 
        {
            current = reportEveryNum;
            ConsoleLogger.Info(Text.Replace("%percentage%", percentage.ToString()), "XProxy");
        }
    }
}

