using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace Block_Chain_Example_1.Models
{
    public class Block
    {
        [Key]
        public int Index { get; set; }
        public string Data { get; set; }
        public string PrevHash { get; set; }
        public string Hash { get; set; }
        public DateTime Timestamp { get; set; }

        public Block() { }  // Конструктор без параметрів
        public Block(int index, string data, string prevHash)
        {
            Index = index;
            Data = data;
            PrevHash = prevHash;
            Timestamp = DateTime.Now;
            Hash = ComputeHash();
        }

        public string ComputeHash()
        {
            var raw = Index + Data + PrevHash + Timestamp.ToString("O");
            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
                return BitConverter.ToString(bytes).Replace("-", "");
            }
        }

        public bool IsHashValid()
        {
            return Hash == ComputeHash();
        }

    }
}
