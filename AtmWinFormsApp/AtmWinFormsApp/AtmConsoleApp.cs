using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AtmLibrary;

namespace AtmConsoleApp
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Console.OutputEncoding = Encoding.UTF8;
                Console.InputEncoding = Encoding.UTF8;
            }
            catch (System.IO.IOException) { }

            Console.WriteLine("Оберіть режим запуску:");
            Console.WriteLine("1 - Консольний додаток");
            Console.WriteLine("2 - Віконний додаток (WinForms)");
            Console.Write("Ваш вибір: ");
            string mode = Console.ReadLine();

            if (mode == "2")
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new AtmWinFormsApp.AtmWinFormsApp());
                return;
            }

            Bank bank = Bank.CreateSampleBank();
            AutomatedTellerMachine atm = bank.Atms.First();

            atm.OnAuthenticated += ConsoleEventHandler;
            atm.OnBalanceChecked += ConsoleEventHandler;
            atm.OnCashWithdrawn += ConsoleEventHandler;
            atm.OnCashDeposited += ConsoleEventHandler;
            atm.OnFundsTransferred += ConsoleEventHandler;

            while (true)
            {
                try
                {
                    Console.Clear();
                }
                catch (System.IO.IOException)
                {
                    Console.WriteLine(new string('-', 50));
                }

                Console.WriteLine(string.Format("=== {0} | Банкомат {1} ({2}) ===", bank.Name, atm.AtmId, atm.Address));
                Console.WriteLine("Тестові картки: 1111222233334444 (ПІН: 1234) | 5555666677778888 (ПІН: 4321)\n");

                if (!atm.IsAuthenticated)
                {
                    Console.Write("Введіть номер картки (або 'exit' для виходу): ");
                    string card = Console.ReadLine();
                    if (card != null && card.ToLower() == "exit") break;

                    Console.Write("Введіть ПІН-код: ");
                    string pin = Console.ReadLine();

                    atm.Authenticate(card ?? "", pin ?? "");
                    Console.WriteLine("Натисніть Enter для продовження...");
                    Console.ReadLine();
                    continue;
                }

                Console.WriteLine(string.Format("Вітаємо, {0}!", atm.CurrentUserName));
                Console.WriteLine("1. Переглянути баланс");
                Console.WriteLine("2. Зняти кошти");
                Console.WriteLine("3. Поповнити картку");
                Console.WriteLine("4. Переказати кошти на іншу картку");
                Console.WriteLine("5. Завершити роботу (витягнути картку)");
                Console.Write("\nОберіть дію: ");

                switch (Console.ReadLine())
                {
                    case "1":
                        atm.CheckBalance();
                        break;
                    case "2":
                        Console.Write("Введіть суму для зняття: ");
                        decimal withdrawSum;
                        if (decimal.TryParse(Console.ReadLine(), out withdrawSum))
                            atm.Withdraw(withdrawSum);
                        else
                            Console.WriteLine("Некоректна сума.");
                        break;
                    case "3":
                        Console.Write("Введіть суму для зарахування: ");
                        decimal depositSum;
                        if (decimal.TryParse(Console.ReadLine(), out depositSum))
                            atm.Deposit(depositSum);
                        else
                            Console.WriteLine("Некоректна сума.");
                        break;
                    case "4":
                        Console.Write("Введіть номер картки отримувача: ");
                        string targetCard = Console.ReadLine() ?? "";
                        Console.Write("Введіть суму переказу: ");
                        decimal transferSum;
                        if (decimal.TryParse(Console.ReadLine(), out transferSum))
                            atm.Transfer(targetCard, transferSum);
                        else
                            Console.WriteLine("Некоректна сума.");
                        break;
                    case "5":
                        atm.Logout();
                        Console.WriteLine("Картку повернуто.");
                        break;
                    default:
                        Console.WriteLine("Невідома команда.");
                        break;
                }

                Console.WriteLine("\nНатисніть Enter для продовження...");
                Console.ReadLine();
            }
        }

        private static void ConsoleEventHandler(object sender, AtmEventArgs e)
        {
            Console.ForegroundColor = e.IsSuccess ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine(string.Format("\n[ПОВІДОМЛЕННЯ БАНКОМАТУ]: {0}", e.Message));
            Console.ResetColor();
        }
    }
}