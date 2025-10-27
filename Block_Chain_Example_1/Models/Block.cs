using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
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
        public string Timestamp { get; set; }     // DateTime замінено на string

        public string Signature { get; set; }
        public string PublickKeyXml { get; private set; }

        // POW
        public int Nonce { get; set; }              // Число, яке використовується для пошуку валідного хешу
        public int Difficulty { get; set; }         // Визначає складність пошуку валідного хешу: кількість нулів на початку блоку
        public long MiningDurationMs { get; set; }  // Час, витрачений на майнінг блоку в мілісекундах


        public Block() { }  // Конструктор без параметрів
        public Block(int index, string data, string prevHash)
        {
            Index = index;
            Data = data;
            PrevHash = prevHash;
            Timestamp = DateTime.UtcNow.ToString("O");
            Hash = ComputeHash();
        }

        public string ComputeHash()
        {
            var raw = Index + Data + PrevHash + Timestamp + Nonce + Difficulty;     // Комбіную всі властивості блоку в один рядок
            using (var sha = SHA256.Create())                                       // Використовую SHA256 для обчислення хешу
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));        // Обчислюю хеш у вигляді масиву байтів
                return BitConverter.ToString(bytes).Replace("-", "");               // Перетворюю байти в шістнадцятковий рядок без дефісів
            }
        }

        public void Sign(RSAParameters privateKey, string publicKey) // Підписати блок за допомогою приватного ключа
        {
            var rsa = RSA.Create();
            rsa.ImportParameters(privateKey);
            byte[] data = Encoding.UTF8.GetBytes(Hash);                                           // Підписую хеш блоку
            byte[] sig = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1); // Використовую SHA256 для хешування при підписі
            Signature = Convert.ToBase64String(sig);                                              // Підпис у форматі Base64
            PublickKeyXml = publicKey;
        }

        public bool Verify() // Перевірити підпис блоку за допомогою публічного ключа
        {
            if (string.IsNullOrWhiteSpace(Signature)) return false; // Якщо підпис відсутній, вважаю його недійсним
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

        public void Mine(int difficulty)                // Генерую хеш, до тих пір поки він не відповідатиме вимогам складності
        {
            Difficulty = difficulty;
            string target = new string('0', Difficulty); // Унікальна строка з необхідною кількістю нулів

            var sw = Stopwatch.StartNew();               // Вимірюю час майнінгу
            do
            {
                Nonce++;                                // Збільшую число Nonce для зміни хешу
                Hash = ComputeHash();                   // Обчислюю новий хеш з оновленим Nonce
            } while (!Hash.StartsWith(target, StringComparison.Ordinal)); // Перевіряю, чи починається хеш з необхідної кількості нулів

            sw.Stop();                                  // Зупиняю таймер
            MiningDurationMs = sw.ElapsedMilliseconds;  // Записую час майнінгу в мілісекундах
        }

        public bool HashValidProof()    // Перевіряю, чи відповідає хеш вимогам складності
        {
            string target = new string('0', Difficulty); // Унікальна строка з необхідною кількістю нулів
            return Hash == ComputeHash() && Hash.StartsWith(target, StringComparison.Ordinal); // Перевіряю відповідність хешу та вимогам складності
        }
    }
}

