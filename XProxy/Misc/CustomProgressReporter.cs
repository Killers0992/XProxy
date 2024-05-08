using System;
using XProxy;

public class CustomProgressReporter : IProgress<float>
{
    private int reportEvery = 20;
    private int current = 0;

    public CustomProgressReporter(string text, string tag)
    {
        Text = text;
        Tag = tag;
    }

    public string Tag { get; }
    public string Text { get; }


    public void Report(float value)
    {
        int percentage = (int)(100 * value) / 1;

        int reportEveryNum = percentage/20;

        if (current != reportEveryNum) 
        {
            current = reportEveryNum;
            Logger.Info(Text.Replace("%percentage%", percentage.ToString()), Tag);
        }
    }
}

