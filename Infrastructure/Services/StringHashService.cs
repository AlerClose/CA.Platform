using System.Security.Cryptography;
using System.Text;
using CA.Platform.Application.Interfaces;

namespace CA.Platform.Infrastructure.Services
{
    public class StringHashService: IStringHashService
    {
        public string GetHash(string password)
        {
            using MD5 md5Hash = MD5.Create();
            return GetMd5Hash(md5Hash, password);
        }
            
        private string GetMd5Hash(MD5 md5Hash, string input)
        {

            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            StringBuilder sBuilder = new StringBuilder();

            foreach (var symbol in data)
            {
                sBuilder.Append(symbol.ToString("x2"));
            }
                
            return sBuilder.ToString();
        }
    }
}