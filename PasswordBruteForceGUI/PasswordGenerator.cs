using System;
using System.Text;

namespace PasswordBruteForceGUI
{
    public class PasswordGenerator
    {
        private readonly Random random = new Random();
        private readonly string characters;

        public PasswordGenerator(string characters)
        {
            this.characters = characters;
        }

        public string GeneratePassword()
        {
            int length = random.Next(4, 6); // [4-6), so length is 4 or 5
            StringBuilder password = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                int index = random.Next(characters.Length);
                password.Append(characters[index]);
            }

            return password.ToString();
        }
    }
}