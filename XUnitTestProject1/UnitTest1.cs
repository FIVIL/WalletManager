using System;
using Xunit;
using CryptoApi;
using WalletManager;
using TransactionManager;
using System.Collections.Generic;

namespace XUnitTestProject1
{
    public class UnitTest1
    {
        private static Dictionary<string, TransactionOutput> utxos = new Dictionary<string, TransactionOutput>();
        //private static Dictionary<string, TransactionOutput> utxosInp = new Dictionary<string, TransactionOutput>();
        [Fact]
        public void Test1()
        {
            AesFileEncryptionPrivider.Create("Xunit");
            var W1 = new Wallet();
            var W2 = new Wallet();
            W1.KeyPair.ExportPrivateKey("key.dat");
            var W3 = new Wallet("key.dat");
            var T = new Transaction(0, W1.PublicKey, W1.PublicKey, 100, 0, Guid.NewGuid().ToString().GetHashString());
            Assert.True(T.Process(x => true, x => true, CFA, Cleaner));
            W1.Refresh(refresh);
            Assert.Equal((double)100, W1.Balance);
        }
        private bool CFA(List<TransactionInput> p)
        {
            var res = true;
            foreach (var item in p)
            {
                if (!utxos.ContainsKey(item.TransactionOutputHash))
                {
                    res = false;
                    break;
                }
            }
            return res;
        }
        private bool Cleaner(List<TransactionInput> inp, List<TransactionOutput> outp)
        {
            try
            {
                foreach (var item in inp)
                {
                    if (item.UTXO != null)
                        utxos.Remove(item.UTXO.HashString);
                }
                foreach (var item in outp)
                {
                    item.IsProcessing = false;
                    utxos.Add(item.HashString, item);
                }
            }
            catch { }
            return true;
        }
        private Dictionary<string, TransactionOutput> refresh(string key)
        {
            var p = new Dictionary<string, TransactionOutput>();
            foreach (var item in utxos.Values)
            {
                if (item.IsMine(key))
                    p.Add(item.HashString, item);
            }
            return p;
        }
    }
}
