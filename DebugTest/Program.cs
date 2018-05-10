﻿using System;
using System.Collections.Generic;
using CryptoApi;
using TransactionManager;
using WalletManager;
namespace DebugTest
{
    class Program
    {
        private static Dictionary<string, TransactionOutput> utxos = new Dictionary<string, TransactionOutput>();
        private static Dictionary<string, TransactionOutput> utxosp { get; set; } = new Dictionary<string, TransactionOutput>();
        private static Dictionary<string, TransactionOutput> utxosm { get; set; } = new Dictionary<string, TransactionOutput>();
        static void Main(string[] args)
        {
            AesFileEncryptionPrivider.Create("Xunit");
            WalletManager.Utilities.PrivateKeyFilePath = "key0.dat";
            WalletManager.Utilities.TransactionVesion = 1;
            TransactionManager.Utilities.BlockRewardForThisVersion = 10;
            var W0 = new Wallet();
            WalletManager.Utilities.PrivateKeyFilePath = "key1.dat";
            var W1 = new Wallet();
            WalletManager.Utilities.PrivateKeyFilePath = "key2.dat";
            var W2 = new Wallet();
            WalletManager.Utilities.PrivateKeyFilePath = "key3.dat";
            var W3 = new Wallet();
            var T = new Transaction(WalletManager.Utilities.TransactionVesion, W0.PublicKey, W1.PublicKey, 100, 0,
                Guid.NewGuid().ToString().GetHashString());
            T.GenerateSignture("key0.dat");
            T.TransactionOutputs.Add(new TransactionOutput(T.Reciepient, T.Amount, T.TransactionHash));
            utxos.Add(T.TransactionOutputs[0].HashString, T.TransactionOutputs[0]);
            Console.WriteLine(T.Process(x => true, null, null, Cleaner));
            W1.Refresh(refresh);
            Console.WriteLine(W1.Balance);
            var T2 = W1.IsuueNewTransaction(Validator, Clean, W2.PublicKey, 15, 2, "h");
            W1.Refresh(refresh);
            W2.Refresh(refresh);
            Console.WriteLine(W1.Balance);
            Console.WriteLine(W2.Balance);
            var TF1 = new Transaction(3, W3.PublicKey, 10);
            TF1.GenerateSignture("key3.dat");
            TF1.TransactionOutputs.Add(new TransactionOutput(TF1.Reciepient, TF1.Amount, TF1.TransactionHash));
            utxos.Add(TF1.TransactionOutputs[0].HashString, TF1.TransactionOutputs[0]);
            Console.WriteLine(T2.Process(x => false, null, CFA, Cleaner));
            Console.WriteLine(TF1.Process(x => false, checkforminerreward, CFA, Cleaner));
            W1.Refresh(refresh);
            W2.Refresh(refresh);
            W3.Refresh(refresh);
            Console.WriteLine(W1.Balance);
            Console.WriteLine(W2.Balance);
            Console.WriteLine(W3.Balance);
            //
            //W1.KeyPair.ExportPrivateKey("key1.dat");
            var T3 = W1.IsuueNewTransaction(Validator, Clean, W2.PublicKey, 15, 3, "h2");
            W1.Refresh(refresh);
            W2.Refresh(refresh);
            Console.WriteLine(W1.Balance);
            Console.WriteLine(W2.Balance);
            var TF2 = new Transaction(3, W3.PublicKey, 10);
            TF2.TransactionOutputs.Add(new TransactionOutput(TF2.Reciepient, TF2.Amount, TF2.TransactionHash));
            utxos.Add(TF2.TransactionOutputs[0].HashString, TF2.TransactionOutputs[0]);
            Console.WriteLine(T3.Process(x => false, null, CFA, Cleaner));
            //W3.KeyPair.ExportPrivateKey("key3.dat");
            TF2.GenerateSignture("key3.dat");
            Console.WriteLine(TF2.Process(x => false, checkforminerreward, CFA, Cleaner));
            W1.Refresh(refresh);
            W2.Refresh(refresh);
            W3.Refresh(refresh);
            Console.WriteLine(W1.Balance);
            Console.WriteLine(W2.Balance);
            Console.WriteLine(W3.Balance);
            Console.ReadKey();
        }

        private static bool checkforminerreward(Transaction t)
        {
            //in reall app remember to make utxo work
            var res = true;
            foreach (var item in t.TransactionOutputs)
            {
                if (!utxosm.ContainsKey(item.HashString))
                {
                    res = false;
                    break;
                }
            }
            return res;
        }
        private static bool Validator(Dictionary<string, TransactionOutput> ut, string publickey)
        {
            var res = true;
            foreach (var item in ut)
            {
                if (!utxos.ContainsKey(item.Key))
                {
                    res = false;
                    break;
                }
                if (!item.Value.IsMine(publickey))
                {
                    res = false;
                    break;
                }
            }
            return res;
        }
        private static bool CFA(List<TransactionInput> p)
        {
            var res = true;
            foreach (var item in p)
            {
                if (!utxosp.ContainsKey(item.TransactionOutputHash))
                {
                    res = false;
                    break;
                }
                else
                {
                    item.UTXO = utxosp[item.TransactionOutputHash];
                }
            }
            return res;
        }
        private static bool Clean(List<TransactionOutput> outp)
        {
            foreach (var item in outp)
            {
                if (utxos.ContainsKey(item.HashString))
                    utxos.Remove(item.HashString);
                else if (utxosm.ContainsKey(item.HashString))
                    utxosp.Remove(item.HashString);
                utxosp.Add(item.HashString, item);

            }
            return true;
        }
        private static bool Cleaner(List<TransactionInput> inp, List<TransactionOutput> outp)
        {
            try
            {
                foreach (var item in inp)
                {
                    if (item.UTXO != null)
                        utxosp.Remove(item.UTXO.HashString);
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
        private static Dictionary<string, TransactionOutput> refresh(string key)
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
