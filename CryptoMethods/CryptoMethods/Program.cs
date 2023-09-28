using System;
using CryptoMethods.CryptoMethods;

namespace CryptoMethods // Note: actual namespace depends on the project name.
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // считывание с консоли
            var encryptionString = Console.ReadLine();
            while (true)
            {
                // вдруг пользователь хитрый или дурак
                if (!string.IsNullOrWhiteSpace(encryptionString))
                {
                    break;
                }
                Console.WriteLine("Плохая строка, давай еще");
                encryptionString = Console.ReadLine();
            }

            // инстанцируем наш крутой шифратор на основе линейного конгруентного генератора
            var linearCongruentGeneratorCipher = new LinearCongruentGeneratorCipher();
            
            //логи
            Console.WriteLine("строка пользователя = " + encryptionString + "\n");
            Console.WriteLine("зашифрованная строка = " + linearCongruentGeneratorCipher.EncryptString(encryptionString) + "\n");
            Console.WriteLine("дешифрованная строка = " + linearCongruentGeneratorCipher.DecryptString() + "\n");
            
            Console.ReadLine();// чтобы не закрывалась консоль сразу
        }
    }
}