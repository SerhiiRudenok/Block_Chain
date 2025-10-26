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
        
        public string Signature { get; set; }
        public string PublickKeyXml { get; private set; }

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

        public void Sign(RSAParameters privateKey, string publicKey) // Підписати блок за допомогою приватного ключа
        {
            var rsa = RSA.Create();
            rsa.ImportParameters(privateKey);
            byte[] data = Encoding.UTF8.GetBytes(Hash);     // Підписуємо хеш блоку
            byte[] sig = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1); // Використовуємо SHA256 для хешування при підписі
            Signature = Convert.ToBase64String(sig); // Зберігаємо підпис у форматі Base64
            PublickKeyXml = publicKey;
        }

        public bool Verify() // Перевірити підпис блоку за допомогою публічного ключа
        {
            if (string.IsNullOrWhiteSpace(Signature)) return false;     // Якщо підпис відсутній, вважаю його недійсним
            try
            {
                var rsa = RSA.Create();                             // Створюю новий екземпляр RSA для перевірки підпису
                rsa.FromXmlString(PublickKeyXml);                   // Імпортую публічний ключ з XML
                byte[] data = Encoding.UTF8.GetBytes(Hash);         // Отримую байти хешу блоку
                byte[] sig = Convert.FromBase64String(Signature);   // Декодую підпис з Base64
                return rsa.VerifyData(data, sig, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1); // Перевіряю підпис за допомогою SHA256
            }
            catch // Після редагування Signature, якщо виникає помилка під час перевірки - вважаю підпис недійсним
            {
                return false;
            }
        }
    }
}

