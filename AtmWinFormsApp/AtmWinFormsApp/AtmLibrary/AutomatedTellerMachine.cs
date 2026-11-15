using System;

namespace AtmLibrary
{
    public class AutomatedTellerMachine
    {
        public string AtmId { get; }
        public string Address { get; }
        public decimal CashAvailable { get; private set; }

        private readonly Bank _bank;
        private Account _currentAccount;

        public event AtmEventHandler OnAuthenticated;
        public event AtmEventHandler OnBalanceChecked;
        public event AtmEventHandler OnCashWithdrawn;
        public event AtmEventHandler OnCashDeposited;
        public event AtmEventHandler OnFundsTransferred;

        public AutomatedTellerMachine(string atmId, string address, decimal initialCash, Bank bank)
        {
            AtmId = atmId;
            Address = address;
            CashAvailable = initialCash;
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
        }

        public bool IsAuthenticated
        {
            get { return _currentAccount != null; }
        }

        public string CurrentUserName
        {
            get { return _currentAccount != null ? _currentAccount.OwnerFullName : null; }
        }

        public string CurrentCardNumber
        {
            get { return _currentAccount != null ? _currentAccount.CardNumber : null; }
        }

        public bool Authenticate(string cardNumber, string pin)
        {
            Account account = _bank.FindAccount(cardNumber);
            if (account != null && account.VerifyPin(pin))
            {
                _currentAccount = account;
                if (OnAuthenticated != null)
                {
                    OnAuthenticated(this, new AtmEventArgs(
                        string.Format("Успішна аутентифікація: {0} (Картка: {1}).", account.OwnerFullName, cardNumber), true));
                }
                return true;
            }

            if (OnAuthenticated != null)
            {
                OnAuthenticated(this, new AtmEventArgs(
                    string.Format("Помилка аутентифікації для картки {0}: невірний номер або ПІН-код.", cardNumber), false));
            }
            return false;
        }

        public void Logout()
        {
            _currentAccount = null;
        }

        public decimal CheckBalance()
        {
            EnsureAuthenticated();
            decimal balance = _currentAccount.Balance;
            if (OnBalanceChecked != null)
            {
                OnBalanceChecked(this, new AtmEventArgs(
                    string.Format("Баланс картки {0}: {1:N2} грн.", _currentAccount.CardNumber, balance)));
            }
            return balance;
        }

        public void Withdraw(decimal amount)
        {
            EnsureAuthenticated();

            if (amount <= 0)
            {
                if (OnCashWithdrawn != null)
                    OnCashWithdrawn(this, new AtmEventArgs("Сума зняття повинна бути більшою за 0.", false));
                return;
            }

            if (amount > CashAvailable)
            {
                if (OnCashWithdrawn != null)
                {
                    OnCashWithdrawn(this, new AtmEventArgs(
                        string.Format("У банкоматі ({0}) недостатньо готівки. Доступно: {1:N2} грн.", AtmId, CashAvailable), false));
                }
                return;
            }

            try
            {
                _currentAccount.Withdraw(amount);
                CashAvailable -= amount;
                if (OnCashWithdrawn != null)
                {
                    OnCashWithdrawn(this, new AtmEventArgs(
                        string.Format("Видано готівку: {0:N2} грн. Залишок на картці: {1:N2} грн.", amount, _currentAccount.Balance)));
                }
            }
            catch (Exception ex)
            {
                if (OnCashWithdrawn != null)
                    OnCashWithdrawn(this, new AtmEventArgs("Помилка зняття коштів: " + ex.Message, false));
            }
        }

        public void Deposit(decimal amount)
        {
            EnsureAuthenticated();

            try
            {
                _currentAccount.Deposit(amount);
                CashAvailable += amount;
                if (OnCashDeposited != null)
                {
                    OnCashDeposited(this, new AtmEventArgs(
                        string.Format("Зараховано {0:N2} грн. Новий баланс: {1:N2} грн.", amount, _currentAccount.Balance)));
                }
            }
            catch (Exception ex)
            {
                if (OnCashDeposited != null)
                    OnCashDeposited(this, new AtmEventArgs("Помилка зарахування: " + ex.Message, false));
            }
        }

        public void Transfer(string targetCardNumber, decimal amount)
        {
            EnsureAuthenticated();

            if (_currentAccount.CardNumber == targetCardNumber)
            {
                if (OnFundsTransferred != null)
                    OnFundsTransferred(this, new AtmEventArgs("Неможливо переказати кошти на ту саму картку.", false));
                return;
            }

            Account targetAccount = _bank.FindAccount(targetCardNumber);
            if (targetAccount == null)
            {
                if (OnFundsTransferred != null)
                    OnFundsTransferred(this, new AtmEventArgs(string.Format("Картку отримувача {0} не знайдено.", targetCardNumber), false));
                return;
            }

            try
            {
                _currentAccount.Withdraw(amount);
                targetAccount.Deposit(amount);
                if (OnFundsTransferred != null)
                {
                    OnFundsTransferred(this, new AtmEventArgs(
                        string.Format("Переказано {0:N2} грн на картку {1} ({2}). Залишок: {3:N2} грн.",
                        amount, targetCardNumber, targetAccount.OwnerFullName, _currentAccount.Balance)));
                }
            }
            catch (Exception ex)
            {
                if (OnFundsTransferred != null)
                    OnFundsTransferred(this, new AtmEventArgs("Помилка переказу: " + ex.Message, false));
            }
        }

        private void EnsureAuthenticated()
        {
            if (_currentAccount == null)
                throw new InvalidOperationException("Операція неможлива: користувач не аутентифікований.");
        }
    }
}