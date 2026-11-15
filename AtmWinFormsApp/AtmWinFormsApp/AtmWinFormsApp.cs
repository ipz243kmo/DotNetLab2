using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AtmLibrary;

namespace AtmWinFormsApp
{
    public class AtmWinFormsApp : Form
    {
        private readonly Bank _bank;
        private readonly AutomatedTellerMachine _atm;

        private enum AtmState { EnterCard, EnterPin, MainMenu, Withdraw, Deposit, TransferCard, TransferAmount }
        private AtmState _state = AtmState.EnterCard;

        private string _tempCard = "";
        private string _tempTargetCard = "";
        private string _inputBuffer = "";

        private Label _lblHeader;
        private Label _lblScreenMain;
        private Label _lblInputDisplay;
        private Button[] _sideButtons = new Button[4];

        public AtmWinFormsApp()
        {
            _bank = Bank.CreateSampleBank();
            _atm = _bank.Atms.First();

            _atm.OnAuthenticated += ShowAtmMessage;
            _atm.OnBalanceChecked += ShowAtmMessage;
            _atm.OnCashWithdrawn += ShowAtmMessage;
            _atm.OnCashDeposited += ShowAtmMessage;
            _atm.OnFundsTransferred += ShowAtmMessage;

            BuildAtmUi();
            UpdateScreen();
        }

        private void ShowAtmMessage(object sender, AtmEventArgs e)
        {
            MessageBox.Show(
                e.Message,
                e.IsSuccess ? "Банкомат — Успіх" : "Банкомат — Помилка",
                MessageBoxButtons.OK,
                e.IsSuccess ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }

        private void BuildAtmUi()
        {
            Text = string.Format("{0} — Банкомат ({1})", _bank.Name, _atm.AtmId);
            Size = new Size(680, 560);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(45, 52, 54);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            _lblHeader = new Label
            {
                Text = string.Format("{0} | {1}", _bank.Name.ToUpper(), _atm.Address),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 35,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(9, 132, 227)
            };
            Controls.Add(_lblHeader);

            Panel screenPanel = new Panel
            {
                Location = new Point(110, 55),
                Size = new Size(440, 220),
                BackColor = Color.FromArgb(10, 61, 98),
                BorderStyle = BorderStyle.Fixed3D
            };
            Controls.Add(screenPanel);

            _lblScreenMain = new Label
            {
                Location = new Point(15, 15),
                Size = new Size(410, 145),
                ForeColor = Color.FromArgb(130, 204, 221),
                Font = new Font("Consolas", 11, FontStyle.Bold)
            };
            screenPanel.Controls.Add(_lblScreenMain);

            _lblInputDisplay = new Label
            {
                Location = new Point(15, 165),
                Size = new Size(410, 35),
                BackColor = Color.FromArgb(20, 80, 120),
                ForeColor = Color.White,
                Font = new Font("Consolas", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            screenPanel.Controls.Add(_lblInputDisplay);

            for (int i = 0; i < 4; i++)
            {
                int index = i;
                Button btn = new Button
                {
                    Location = new Point(560, 65 + i * 52),
                    Size = new Size(85, 42),
                    Text = "◄ F" + (i + 1),
                    BackColor = Color.LightGray,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                };
                btn.Click += (s, e) => OnSideButtonClick(index);
                _sideButtons[i] = btn;
                Controls.Add(btn);
            }

            Button btnExit = new Button
            {
                Location = new Point(15, 221),
                Size = new Size(85, 42),
                Text = "Вихід ►",
                BackColor = Color.IndianRed,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnExit.Click += (s, e) =>
            {
                _atm.Logout();
                _state = AtmState.EnterCard;
                _inputBuffer = "";
                UpdateScreen();
            };
            Controls.Add(btnExit);

            string[] keys = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "00", "0", "." };
            int startX = 160, startY = 295;
            for (int i = 0; i < keys.Length; i++)
            {
                string k = keys[i];
                Button btn = new Button
                {
                    Text = k,
                    Size = new Size(65, 45),
                    Location = new Point(startX + (i % 3) * 72, startY + (i / 3) * 50),
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    BackColor = Color.WhiteSmoke
                };
                btn.Click += (s, e) => AppendInput(k);
                Controls.Add(btn);
            }

            Button btnCancel = CreateActionButton("CANCEL", Color.Crimson, new Point(390, 295), (s, e) =>
            {
                _inputBuffer = "";
                _state = _atm.IsAuthenticated ? AtmState.MainMenu : AtmState.EnterCard;
                UpdateScreen();
            });

            Button btnClear = CreateActionButton("CLEAR", Color.Goldenrod, new Point(390, 345), (s, e) =>
            {
                _inputBuffer = "";
                UpdateInputDisplay();
            });

            Button btnEnter = CreateActionButton("ENTER", Color.SeaGreen, new Point(390, 395), (s, e) => ProcessEnter());

            Controls.AddRange(new Control[] { btnCancel, btnClear, btnEnter });
        }

        private Button CreateActionButton(string text, Color color, Point loc, EventHandler onClick)
        {
            Button btn = new Button
            {
                Text = text,
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(110, 45),
                Location = loc
            };
            btn.Click += onClick;
            return btn;
        }

        private void AppendInput(string val)
        {
            if (_state == AtmState.MainMenu) return;
            if (_inputBuffer.Length < 16)
            {
                _inputBuffer += val;
                UpdateInputDisplay();
            }
        }

        private void UpdateInputDisplay()
        {
            _lblInputDisplay.Text = _state == AtmState.EnterPin
                ? new string('*', _inputBuffer.Length)
                : _inputBuffer;
        }

        private void UpdateScreen()
        {
            UpdateInputDisplay();
            switch (_state)
            {
                case AtmState.EnterCard:
                    _lblScreenMain.Text = "ВВЕДІТЬ НОМЕР КАРТКИ (16 цифр)\nта натисніть ENTER:\n\nТест: 1111222233334444 (ПІН 1234)";
                    break;
                case AtmState.EnterPin:
                    _lblScreenMain.Text = string.Format("Картка: {0}\n\nВВЕДІТЬ ПІН-КОД\nта натисніть ENTER:", _tempCard);
                    break;
                case AtmState.MainMenu:
                    _lblScreenMain.Text =
                        string.Format("Клієнт: {0}\nОберіть операцію (кнопки F1-F4):\n\n", _atm.CurrentUserName) +
                        "                         [F1] Баланс ►\n" +
                        "                    [F2] Зняти кошти ►\n" +
                        "                 [F3] Поповнити картку ►\n" +
                        "               [F4] Переказ на картку ►";
                    break;
                case AtmState.Withdraw:
                    _lblScreenMain.Text = "ЗНЯТТЯ ГОТІВКИ\n\nВведіть суму (грн) та натисніть ENTER:";
                    break;
                case AtmState.Deposit:
                    _lblScreenMain.Text = "ПОПОВНЕННЯ РАХУНКУ\n\nВведіть суму (грн) та натисніть ENTER:";
                    break;
                case AtmState.TransferCard:
                    _lblScreenMain.Text = "ПЕРЕКАЗ КОШТІВ\n\nВведіть номер картки отримувача\nта натисніть ENTER:";
                    break;
                case AtmState.TransferAmount:
                    _lblScreenMain.Text = string.Format("Отримувач: {0}\n\nВведіть суму переказу (грн)\nта натисніть ENTER:", _tempTargetCard);
                    break;
            }
        }

        private void OnSideButtonClick(int index)
        {
            if (!_atm.IsAuthenticated) return;
            _inputBuffer = "";

            switch (index)
            {
                case 0:
                    _atm.CheckBalance();
                    break;
                case 1:
                    _state = AtmState.Withdraw;
                    break;
                case 2:
                    _state = AtmState.Deposit;
                    break;
                case 3:
                    _state = AtmState.TransferCard;
                    break;
            }
            UpdateScreen();
        }

        private void ProcessEnter()
        {
            switch (_state)
            {
                case AtmState.EnterCard:
                    _tempCard = _inputBuffer;
                    _inputBuffer = "";
                    _state = AtmState.EnterPin;
                    break;

                case AtmState.EnterPin:
                    bool ok = _atm.Authenticate(_tempCard, _inputBuffer);
                    _inputBuffer = "";
                    _state = ok ? AtmState.MainMenu : AtmState.EnterCard;
                    break;

                case AtmState.Withdraw:
                    decimal wAmount;
                    if (decimal.TryParse(_inputBuffer, out wAmount))
                        _atm.Withdraw(wAmount);
                    _inputBuffer = "";
                    _state = AtmState.MainMenu;
                    break;

                case AtmState.Deposit:
                    decimal dAmount;
                    if (decimal.TryParse(_inputBuffer, out dAmount))
                        _atm.Deposit(dAmount);
                    _inputBuffer = "";
                    _state = AtmState.MainMenu;
                    break;

                case AtmState.TransferCard:
                    _tempTargetCard = _inputBuffer;
                    _inputBuffer = "";
                    _state = AtmState.TransferAmount;
                    break;

                case AtmState.TransferAmount:
                    decimal tAmount;
                    if (decimal.TryParse(_inputBuffer, out tAmount))
                        _atm.Transfer(_tempTargetCard, tAmount);
                    _inputBuffer = "";
                    _state = AtmState.MainMenu;
                    break;
            }
            UpdateScreen();
        }
    }
}