using System;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusDeviceControl
{
    public class StrobeWorker
    {
        private readonly ModbusClientManager _modbus;
        private readonly ushort _coilAddress;
        private readonly int _highTime;
        private readonly int _lowTime;
        private readonly bool _inverted;

        private bool _running;
        private Task _workerTask;

        public StrobeWorker(ModbusClientManager modbus, ushort coilAddress, double frequency, double dutyCycle, bool inverted)
        {
            _modbus = modbus;
            _coilAddress = coilAddress;
            _inverted = inverted;

            double period = 1000.0 / frequency;
            _highTime = (int)(period * (dutyCycle / 100.0));
            _lowTime = (int)(period - _highTime);
        }

        public void Start()
        {
            _running = true;
            _workerTask = Task.Run(async () =>
            {
                while (_running && _modbus != null && _modbus.IsConnected)
                {
                    await _modbus.WriteCoilAsync(_coilAddress, !_inverted);
                    await Task.Delay(_highTime);
                    await _modbus.WriteCoilAsync(_coilAddress, _inverted);
                    await Task.Delay(_lowTime);
                }
            });
        }

        public void Stop()
        {
            _running = false;
        }
    }
}