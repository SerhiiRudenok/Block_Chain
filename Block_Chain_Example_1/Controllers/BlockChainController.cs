using Block_Chain_Example_1.Models;
using Block_Chain_Example_1.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Block_Chain_Example_1.Controllers
{
    public class BlockChainController : Controller
    {
        private static BlockChainService _blockChainService = new BlockChainService();
        // GET: BlockChainController


        public IActionResult Index()
        {
            ViewBag.Valid = _blockChainService.IsValid();                           // Перевірити валідність всього блокчейну
            ViewBag.FirstInvalidIndex = _blockChainService.GetFirstInvalidIndex();  // Отримати індекс першого невалідного блоку
            return View(_blockChainService.Chain);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(string data, string privateKeyBase) // ДОДАЮ новий блоку з підписом користувача
        {
            try
            {
                var prevBlock = _blockChainService.Chain.Last();    // Отримую останній блок у ланцюжку
                var newBlock = new Block(_blockChainService.Chain.Count, data, prevBlock.Hash); // Створюю новий блок: індекс = порядковий номер, дані = передані дані, PrevHash = хеш останнього блоку

                // 1. Декодую Base64-рядок у байти
                byte[] keyBytes = Convert.FromBase64String(privateKeyBase);

                // 2. Імпортую приватний ключ
                var rsa = RSA.Create();
                rsa.ImportRSAPrivateKey(keyBytes, out _); // PKCS#1 формат

                // 3. Експортую параметри для підпису
                var privateKey = rsa.ExportParameters(true);
                var publicKeyXml = rsa.ToXmlString(false); // зберігаю публічний ключ у XML

                // 4. Підписую блок
                newBlock.Sign(privateKey, publicKeyXml);
                _blockChainService.Chain.Add(newBlock);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Помилка при додаванні блоку: " + ex.Message;
                return View("Index", _blockChainService.Chain);
            }
        }


        // GET: BlockChainController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: BlockChainController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: BlockChainController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: BlockChainController/Edit/5
        public ActionResult Edit(int id)
        {
            var block = _blockChainService.Chain.FirstOrDefault(b => b.Index == id);
            if (block == null)
                return NotFound();

            return View(block);
        }

        // POST: BlockChainController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Block_Chain_Example_1.Models.Block updatedBlock)
        {
            var block = _blockChainService.Chain.FirstOrDefault(b => b.Index == id);
            if (block == null)
                return NotFound();

            block.Data = updatedBlock.Data;
            block.Signature = updatedBlock.Signature; // оновити підпис
            block.Hash = block.ComputeHash(); // оновити хеш після зміни
            
            ViewBag.Valid = _blockChainService.IsValid();

            return RedirectToAction(nameof(Index));
        }

        // GET: BlockChainController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: BlockChainController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }


        // Пошук блоку за індексом або хешем
        [HttpGet]
        public IActionResult Search()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Search(string query)
        {
            var block = _blockChainService.FindBlock(query);
            ViewBag.FoundBlock = block;
            ViewBag.IsPost = true;
            return View();
        }


        // Генерація нової нового RSA-ключа
        [HttpPost]
        public IActionResult GenerateKey()
        {
            // Генерація RSA-ключа
            using var rsa = RSA.Create(512);
            byte[] privateKeyBytes = rsa.ExportRSAPrivateKey();
            string base64Key = Convert.ToBase64String(privateKeyBytes);

            ViewBag.GeneratedKey = base64Key;

            // Повернення на Index.cshtml
            return View("Index", _blockChainService.Chain);
        }



    }
}
