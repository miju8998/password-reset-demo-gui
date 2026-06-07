namespace PasswordBruteForceGUI
{
    public class CandidateGenerator
    {
        private readonly string characters;

        public CandidateGenerator(string characters)
        {
            this.characters = characters;
        }

        public long CountForLength(int length)
        {
            long count = 1;

            for (int i = 0; i < length; i++)
            {
                count *= characters.Length;
            }

            return count;
        }

        public long TotalCandidates(int maxLength)
        {
            long total = 0;

            for (int length = 1; length <= maxLength; length++)
            {
                total += CountForLength(length);
            }

            return total;
        }

        public string GetCandidate(long index, int length)
        {
            char[] result = new char[length];
            int baseSize = characters.Length;

            for (int position = length - 1; position >= 0; position--)
            {
                result[position] = characters[(int)(index % baseSize)];
                index /= baseSize;
            }

            return new string(result);
        }
    }
}