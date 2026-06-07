using System;

namespace PasswordBruteForceGUI
{
    public class AttackResult
    {
        public string Mode { get; set; }
        public string FoundPassword { get; set; }
        public long Attempts { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public int ThreadsUsed { get; set; }
        public bool Stopped { get; set; }
    }
}