using ETH_HUNTER;

namespace ETH_Generator.Controllers
{
    public static class IdController
    {
        private static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string GetId()
        {
            string fileName = Paths.id;

            // Check if the file exists
            if (File.Exists(fileName))
            {
                // File exists, so read its content
                return File.ReadAllText(fileName);
            }
            else
            {
                // File does not exist, generate a random string
                string randomString = GenerateRandomString(12);

                // Create the file and write the random string to it
                File.WriteAllText(fileName, randomString + "." + Environment.MachineName);
                return randomString;
            }
        }
    }
}
