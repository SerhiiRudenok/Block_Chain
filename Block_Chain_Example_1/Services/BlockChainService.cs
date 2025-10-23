using Block_Chain_Example_1.Models;

namespace Block_Chain_Example_1.Services
{
    public class BlockChainService
    {
        public List<Block> Chain { get; set; } = new List<Block>();

        public BlockChainService()
        {
            Chain.Add(new Block(0, "Генезіс-блок", "0"));
        }

        public void AddBlock(string data)
        {
            var prevBlock = Chain[Chain.Count - 1];
            var newBlock = new Block(Chain.Count, data, prevBlock.Hash);
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
            }
            return true;
        }


        public List<bool> GetValidityMap()  // Повертає список валідності для кожного блоку
        {
            var validity = new List<bool>();

            for (int i = 0; i < Chain.Count; i++)
            {
                if (i == 0)
                {
                    validity.Add(true); // Генезіс-блок завжди валідний
                    continue;
                }

                var current = Chain[i];
                var previous = Chain[i - 1];

                bool isValid = current.PrevHash == previous.Hash && current.IsHashValid();
                validity.Add(isValid);
            }

            return validity;
        }

        public int? GetFirstInvalidIndex()  // Повертає індекс першого невалідного блоку або null, якщо всі валідні
        {
            for (int i = 1; i < Chain.Count; i++)
            {
                var current = Chain[i];
                var previous = Chain[i - 1];

                if (current.PrevHash != previous.Hash || !current.IsHashValid())
                    return i - 1;
            }
            return null;
        }

        public string CheckHashIntegrityMessage()  // Повідомлення "Порушено цілісність ланцюга блоків."
        {
            for (int i = 1; i < Chain.Count; i++)
            {
                var current = Chain[i];
                var previous = Chain[i - 1];

                bool hashesMatch = current.PrevHash == previous.Hash;
                bool hashIsValid = current.Hash == current.ComputeHash();

                if (!hashesMatch || !hashIsValid)
                {
                    return "Порушено цілісність ланцюга блоків.";
                }
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
