using Block_Chain_Example_1.Models;
using System.Security.Cryptography;

namespace Block_Chain_Example_1.Services
{
    public class BlockChainService
    {
        public List<Block> Chain { get; set; } = new List<Block>();
        private readonly RSAParameters _privateKey;
        private readonly string _publickKeyXml;

        public int Difficulty { get; set; } = 3; // Визначає складність майнінгу (кількість провідних нулів у хеші)

        public BlockChainService()
        {
            var rsa = RSA.Create();                     //  створюю нову пару ключів: приватний і публічний
            _privateKey = rsa.ExportParameters(true);   // Зберігаю приватний ключ для підпису блоків
            _publickKeyXml = rsa.ToXmlString(false);    // Зберігаю публічний ключ у форматі XML для зберігання в блоці

            var block = new Block(0, "Генезіс-блок", "");   // Створюю генезіс-блок з індексом 0, даними "Генезіс-блок" і порожнім PrevHash
            block.Mine(Difficulty);                         // Генерую генезіс-блок з заданою складністю
            block.Sign(_privateKey, _publickKeyXml);        // Підписую генезіс-блок за допомогою приватного ключа
            Chain.Add(block);                               // Додаю генезіс-блок до ланцюжка
        }


        public long AddBlock(string data)
        {
            var prevBlock = Chain[Chain.Count - 1]; // Отримую останній блок у ланцюжку
            var newBlock = new Block(Chain.Count, data, prevBlock.Hash); // Створюю новий блок: індекс = порядковий номер, дані = передані дані, PrevHash = хеш останнього блоку
            newBlock.Mine(Difficulty);                  // Генерую новий блок з заданою складністю
            newBlock.Sign(_privateKey, _publickKeyXml); // Підписую новий блок за допомогою приватного ключа, зберігаю публічний ключ у блоці для подальшої перевірки
            Chain.Add(newBlock);                        // Додаю новий блок до ланцюжка
            return newBlock.MiningDurationMs;           // Повертаю час майнінгу нового блоку
        }

        public bool IsValid()
        {
            for (int i = 1; i < Chain.Count; i++)                        // Починаю перевірку з другого блоку (індекс 1)
            {
                var current = Chain[i];                                  // Поточний блок
                var prevBlock = Chain[i - 1];                            // Попередній блок

                if (current.PrevHash != prevBlock.Hash) return false;    // Перевірка відповідності хешів
                if (current.Hash != current.ComputeHash()) return false; // Перевірка валідності хешу
                if (!current.Verify()) return false;                     // Перевірка валідності підпису
                if(!current.Hash.StartsWith(new string('0', current.Difficulty))) return false; // Перевірка складності майнінгу
            }
            return true;
        }


        public int? GetFirstInvalidIndex()
        {
            for (int i = 1; i < Chain.Count; i++)
            {
                var current = Chain[i];
                var prevBlock = Chain[i - 1];

                bool hashIsMatch = current.PrevHash == prevBlock.Hash;    // Перевірка відповідності хешів
                bool hashIsValid = current.Hash == current.ComputeHash(); // Перевірка валідності хешу
                bool signatureIsValid = current.Verify();                 // Перевірка валідності підпису

                if (!hashIsMatch || !hashIsValid || !signatureIsValid)
                    return i;
            }

            return null;
        }

        public Block FindBlock(string query)  // Пошук блоку за хешем або індексом
        {
            if (int.TryParse(query, out int index))
            {
                return Chain.FirstOrDefault(b => b.Index == index);
            }
            else
            {
                return Chain.FirstOrDefault(b => b.Hash.Equals(query, StringComparison.OrdinalIgnoreCase));
            }
        }

    }
}
