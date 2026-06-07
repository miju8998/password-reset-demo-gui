using System;
using System.Text;

namespace PasswordBruteForceGUI
{
    public class PerformanceLogger
    {
        private readonly StringBuilder builder = new StringBuilder();

        public void Clear()
        {
            builder.Clear();
        }

        public void Add(string message)
        {
            builder.AppendLine(DateTime.Now.ToString("HH:mm:ss") + " - " + message);
        }

        public override string ToString()
        {
            return builder.ToString();
        }
    }
}