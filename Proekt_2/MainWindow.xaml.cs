using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NModbus;

namespace ModbusDeviceControl
{
    public partial class MainWindow : Window
    {
        private ModbusClientManager _modbusManager;
        private List<PinInfo> _pins = new List<PinInfo>();
        private List<StrobeInfo> _strobes = new List<StrobeInfo>();
        private bool _isStrobeRunning = false;

        private StrobeWorker _strobeWorker;

        public MainWindow()
        {
            InitializeComponent();
            comboStrobeType.ItemsSource = new[] { "Прямой", "Инверсный" };
            comboStrobeType.SelectedIndex = 0;
        }

        private async void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string ip = txtIP.Text.Trim();
                int port = int.Parse(txtPort.Text.Trim());

                _modbusManager = new ModbusClientManager(ip, port);
                await _modbusManager.ConnectAsync();

                txtStatus.Text = "Подключено";
                txtStatus.Foreground = Brushes.Green;

                LoadPins();
                LoadStrobes();

                Log("Подключение успешно.");
            }
            catch (Exception ex)
            {
                Log("Ошибка подключения: " + ex.Message);
                txtStatus.Text = "Не подключено";
                txtStatus.Foreground = Brushes.Red;
            }
        }

        private void BtnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            if (_modbusManager != null)
            {
                _modbusManager.Disconnect();
            }

            txtStatus.Text = "Не подключено";
            txtStatus.Foreground = Brushes.Red;
            Log("Отключено от устройства.");
        }

        private void LoadPins()
        {
            _pins.Clear();
            for (int i = 0; i < 16; i++)
            {
                _pins.Add(new PinInfo { Id = i, Name = "Пин " + i });
            }

            comboPins.ItemsSource = _pins;
            comboPins.SelectedIndex = 0;
            txtPinCount.Text = "Доступные пины: " + _pins.Count;
        }

        private void LoadStrobes()
        {
            _strobes.Clear();
            for (int i = 0; i < 8; i++)
            {
                _strobes.Add(new StrobeInfo { Id = i, Name = "Строб " + i });
            }

            comboStrobes.ItemsSource = _strobes;
            comboStrobes.SelectedIndex = 0;
            txtStrobeCount.Text = "Доступные стробы: " + _strobes.Count;
        }

        private async void BtnSendSignal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var pin = comboPins.SelectedItem as PinInfo;
                if (pin == null)
                    return;

                int duration = int.Parse(txtDuration.Text.Trim());

                await _modbusManager.WriteCoilAsync((ushort)pin.Id, true);
                await Task.Delay(duration);
                await _modbusManager.WriteCoilAsync((ushort)pin.Id, false);

                Log("Сигнал на пин " + pin.Id + " отправлен на " + duration + " мс.");
            }
            catch (Exception ex)
            {
                Log("Ошибка отправки сигнала: " + ex.Message);
            }
        }

        private async void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var pin in _pins)
                {
                    await _modbusManager.WriteCoilAsync((ushort)pin.Id, false);
                }
                Log("Сброс всех пинов.");
            }
            catch (Exception ex)
            {
                Log("Ошибка сброса: " + ex.Message);
            }
        }

        private void BtnStartStrobe_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isStrobeRunning)
                {
                    Log("Строб уже запущен.");
                    return;
                }

                var strobe = comboStrobes.SelectedItem as StrobeInfo;
                if (strobe == null) return;

                double freq = double.Parse(txtFreq.Text);
                double duty = double.Parse(((ComboBoxItem)comboDuty.SelectedItem).Content.ToString());
                string type = comboStrobeType.SelectedItem != null ? comboStrobeType.SelectedItem.ToString() : "Прямой";
                bool inverted = type == "Инверсный";

                _strobeWorker = new StrobeWorker(_modbusManager, (ushort)(100 + strobe.Id), freq, duty, inverted);
                _strobeWorker.Start();

                _isStrobeRunning = true;

                Log(string.Format("Старт строба {0} с частотой {1} Гц, скважностью {2}%, тип: {3}",
                    strobe.Id, freq, duty, type));
            }
            catch (Exception ex)
            {
                Log("Ошибка запуска строба: " + ex.Message);
            }
        }

        private async void BtnStopStrobe_Click(object sender, RoutedEventArgs e)
        {
            _isStrobeRunning = false;

            if (_strobeWorker != null)
            {
                _strobeWorker.Stop();
                _strobeWorker = null;
            }

            var strobe = comboStrobes.SelectedItem as StrobeInfo;
            if (strobe == null)
                return;

            try
            {
                await _modbusManager.WriteCoilAsync((ushort)(100 + strobe.Id), false);
                Log("Строб " + strobe.Id + " остановлен.");
            }
            catch (Exception ex)
            {
                Log("Ошибка остановки строба: " + ex.Message);
            }
        }

        private void Log(string message)
        {
            Dispatcher.Invoke(() =>
            {
                txtLog.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + message + "\n");
                txtLog.ScrollToEnd();
            });
        }

        private void comboStrobes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }

    public class PinInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public override string ToString() { return Name; }
    }

    public class StrobeInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public override string ToString() { return Name; }
    }

    public class ModbusClientManager
    {
        private string _ip;
        private int _port;
        private TcpClient _tcpClient;
        private IModbusMaster _master;

        public bool IsConnected
        {
            get { return _tcpClient != null && _tcpClient.Connected; }
        }

        public ModbusClientManager(string ip, int port)
        {
            _ip = ip;
            _port = port;
        }

        public async Task ConnectAsync()
        {
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(_ip, _port);
            var factory = new ModbusFactory();
            _master = factory.CreateMaster(_tcpClient);
        }

        public void Disconnect()
        {
            if (_tcpClient != null)
            {
                _tcpClient.Close();
            }
        }

        public async Task WriteCoilAsync(ushort address, bool value)
        {
            if (!IsConnected) return;
            await Task.Run(() => _master.WriteSingleCoil(1, address, value));
        }
    }
}
