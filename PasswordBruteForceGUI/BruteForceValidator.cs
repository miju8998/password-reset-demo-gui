namespace PasswordBruteForceGUI
{
    public class BruteForceValidator
    {
        private readonly string targetHash;
        private readonly PasswordHasher hasher;

        public BruteForceValidator(string targetHash, PasswordHasher hasher)
        {
            this.targetHash = targetHash;
            this.hasher = hasher;
        }

        public bool IsValid(string candidate)
        {
            string candidateHash = hasher.ComputeSha256Hash(candidate);
            return candidateHash == targetHash;
        }
    }
}