using System.Collections.Generic;
using System.Text;

namespace XProxy.Core
{
    public class HintBuilder
    {
        public class HintLine
        {
            public string Left = string.Empty;
            public string Center = string.Empty;
            public string Right = string.Empty;

            public bool AllEmpty => string.IsNullOrEmpty(Left) && string.IsNullOrEmpty(Center) && string.IsNullOrEmpty(Right);
        }

        public HintBuilder()
        {
            for (int x = 0; x < 33; x++)
            {
                Lines.Add(new HintLine());
            }
        }

        public List<HintLine> Lines = new List<HintLine>();

        public void SetRightLine(int line, string text)
        {
            Lines[line - 1].Right = text;
        }

        public void SetLeftLine(int line, string text)
        {
            Lines[line - 1].Left = text;
        }

        public void SetCenterLine(int line, string text)
        {
            Lines[line - 1].Center = text;
        }

        public string Build()
        {
            StringBuilder builder = new StringBuilder();

            for (int x = 0; x < Lines.Count; x++)
            {
                HintLine line = Lines[x];

                if (line.AllEmpty)
                {
                    builder.AppendLine(string.Empty);
                    continue;
                }

                if (!string.IsNullOrEmpty(line.Center))
                {
                    builder.AppendLine($"<align=center>{line.Center}</align>");
                    continue;
                }

                if (!string.IsNullOrEmpty(line.Right) && string.IsNullOrEmpty(line.Left))
                {
                    builder.AppendLine($"<align=right>{line.Right}</align>");
                    continue;
                }

                if (string.IsNullOrEmpty(line.Right) && !string.IsNullOrEmpty(line.Left))
                {
                    builder.AppendLine($"<align=left>{line.Left}</align>");
                    continue;
                }

                if (!string.IsNullOrEmpty(line.Right) && !string.IsNullOrEmpty(line.Left))
                {
                    builder.AppendLine($"<align=left>{line.Left}<line-height=0>");
                    builder.AppendLine($"<align=right>{line.Right}<line-height=1em></align>");
                    continue;
                }
            }

            return builder.ToString();
        }

        public override string ToString() => Build();
    }
}
