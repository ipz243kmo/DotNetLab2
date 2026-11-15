using System;

namespace AtmLibrary
{
    public class Account
    {
        public string CardNumber { get; }
        public string PinCode { get; private set; }
        public string OwnerFullName { get; }
        public string Email { get; }
        public decimal Balance { get; private set; }

        public Account(string cardNumber, string pinCode, string ownerFullName, decimal initialBalance, string email = "")
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
                throw new ArgumentException("Номер картки не може бути порожнім.", nameof(cardNumber));
            if (string.IsNullOrWhiteSpace(pinCode))
                throw new ArgumentException("ПІН-код не може бути порожнім.", nameof(pinCode));
            if (initialBalance < 0)
                throw new ArgumentException("Початковий баланс не може бути від'ємним.", nameof(initialBalance));

            CardNumber = cardNumber;
            PinCode = pinCode;
            OwnerFullName = ownerFullName;
            Balance = initialBalance;
            Email = email;
        }

        public bool VerifyPin(string pin)
        {
            return PinCode == pin;
        }

        public void Deposit(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Сума поповнення повинна бути більшою за нуль.");

            Balance += amount;
        }

        public void Withdraw(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Сума зняття повинна бути більшою за нуль.");
            if (amount > Balance)
                throw new InvalidOperationException("Недостатньо коштів на картці.");

            Balance -= amount;
        }
    }
}