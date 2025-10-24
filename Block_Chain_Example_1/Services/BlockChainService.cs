using Block_Chain_Example_1.Models;
using System.Security.Cryptography;

namespace Block_Chain_Example_1.Services
{
    public class BlockChainService
    {
        public List<Block> Chain { get; set; } = new List<Block>();
        private readonly RSAParameters _privateKey;
        private readonly RSAParameters _publicKey;
        private readonly string _publickKeyXml;


        public BlockChainService()
        {
            var rsa = RSA.Create();
            _privateKey = rsa.ExportParameters(true);
            _publicKey = rsa.ExportParameters(false);
            _publickKeyXml = rsa.ToXmlString(false);

            var block = new Block(0, "Генезіс-блок", "");

            block.Sign(_privateKey, _publickKeyXml);
            Chain.Add(block);
        }


        public void AddBlock(string data)
        {
            var prevBlock = Chain[Chain.Count - 1];
            var newBlock = new Block(Chain.Count, data, prevBlock.Hash);
            newBlock.Sign(_privateKey, _publickKeyXml);
            Chain.Add(newBlock);

        }

        public bool IsValid()
        {
            for (int i = 1; i < Chain.Count; i++)
            {
                var current = Chain[i];
                var prevBlock = Chain[i - 1];

                if (current.PrevHash != prevBlock.Hash) return false;
                if (current.Hash != current.ComputeHash()) return false;
                if (!current.Verify()) return false;
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
