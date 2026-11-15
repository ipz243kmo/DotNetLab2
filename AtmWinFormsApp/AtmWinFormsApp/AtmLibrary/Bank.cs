using System.Collections.Generic;
using System.Linq;

namespace AtmLibrary
{
    public class Bank
    {
        public string Name { get; }
        public List<AutomatedTellerMachine> Atms { get; } = new List<AutomatedTellerMachine>();
        private readonly List<Account> _accounts = new List<Account>();

        public Bank(string name)
        {
            Name = name;
        }

        public void AddAccount(Account account)
        {
            _accounts.Add(account);
        }

        public void AddAtm(AutomatedTellerMachine atm)
        {
            Atms.Add(atm);
        }

        public Account FindAccount(string cardNumber)
        {
            return _accounts.FirstOrDefault(a => a.CardNumber == cardNumber);
        }

        public static Bank CreateSampleBank()
        {
            Bank bank = new Bank("ЖитомирБанк");

            bank.AddAccount(new Account("1111222233334444", "1234", "Шевченко Тарас", 15000m, "taras@example.com"));
            bank.AddAccount(new Account("5555666677778888", "4321", "Косач Лариса", 8500m, "lesya@example.com"));
            bank.AddAccount(new Account("9999000011112222", "0000", "Франко Іван", 3200m, "ivan@example.com"));

            AutomatedTellerMachine atm = new AutomatedTellerMachine("ATM-ZT-01", "м. Житомир, вул. Чуднівська, 103", 50000m, bank);
            bank.AddAtm(atm);

            return bank;
        }
    }
}