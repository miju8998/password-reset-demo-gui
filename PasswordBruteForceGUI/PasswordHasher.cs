using System.Security.Cryptography;
using System.Text;

namespace PasswordBruteForceGUI
{
    public class PasswordHasher
    {
        public const string StaticSalt = "CSharpFinalTaskStaticSalt2026";

        public string ComputeSha256Hash(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(StaticSalt + password);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                StringBuilder builder = new StringBuilder();

                foreach (byte b in hashBytes)
                {
                    builder.Append(b.ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}