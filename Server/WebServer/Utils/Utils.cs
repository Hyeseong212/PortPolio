using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace WebServer
{
    public static class Utils
    {
        /// <summary>
        /// 비밀번호를 SHA-256 알고리즘을 사용하여 해시합니다.
        /// </summary>
        /// <param name="password">해싱할 비밀번호</param>
        /// <returns>해시된 비밀번호 문자열</returns>
        public static string HashPassword(string password)
        {
            using SHA256 sha256Hash = SHA256.Create();
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }

        /// <summary>
        /// 입력된 비밀번호가 해시된 비밀번호와 일치하는지 확인합니다.
        /// </summary>
        /// <param name="password">입력된 비밀번호</param>
        /// <param name="hashedPassword">해시된 비밀번호</param>
        /// <returns>비밀번호가 일치하면 true, 그렇지 않으면 false</returns>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            string hashOfInput = HashPassword(password);
            return StringComparer.OrdinalIgnoreCase.Compare(hashOfInput, hashedPassword) == 0;
        }

        /// <summary>
        /// 구성 파일에서 DB 설정을 읽어 연결 문자열을 반환합니다.
        /// </summary>
        /// <param name="filePath">구성 파일 경로</param>
        /// <returns>DB 연결 문자열</returns>
        public static (string, string) GetMySQLDbConfig(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Configuration file not found", filePath);
            }

            string[] lines = File.ReadAllLines(filePath);
            string ip = string.Empty;
            string port = string.Empty;

            foreach (string line in lines)
            {
                if (line.StartsWith("MySQL ip:"))
                {
                    ip = line.Split(':')[1].Trim();
                }
                else if (line.StartsWith("MySQL port:"))
                {
                    port = line.Split(':')[1].Trim();
                }
            }

            if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(port))
            {
                throw new InvalidDataException("Invalid configuration file format.");
            }

            return (ip, port);
        }
        public static (string, string) GetRedisDbConfig(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Configuration file not found", filePath);
            }

            string[] lines = File.ReadAllLines(filePath);
            string ip = string.Empty;
            string port = string.Empty;

            foreach (string line in lines)
            {
                if (line.StartsWith("Redis ip:"))
                {
                    ip = line.Split(':')[1].Trim();
                }
                else if (line.StartsWith("Redis port:"))
                {
                    port = line.Split(':')[1].Trim();
                }
            }

            if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(port))
            {
                throw new InvalidDataException("Invalid configuration file format.");
            }

            return (ip, port);
        }
        public static int GetLength(float data)
        {
            return BitConverter.GetBytes(data).Length;
        }
        public static int GetLength(int data)
        {
            return BitConverter.GetBytes(data).Length;
        }
        public static int GetLength(string data)
        {
            return Encoding.UTF8.GetBytes(data).Length;
        }
        public static int GetLength(long data)
        {
            return BitConverter.GetBytes(data).Length;
        }
        public static int GetLength(bool data)
        {
            return BitConverter.GetBytes(data).Length;
        }
        public static int GetLength(byte data)
        {
            return 1;
        }
    }
}