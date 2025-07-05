using System;
using EasyModbus;

namespace ModbusApp
{
    public class ModbusClientManager
    {
        private ModbusClient modbusClient;

        public bool Connect(string ipAddress, int port)
        {
            try
            {
                modbusClient = new ModbusClient(ipAddress, port);
                modbusClient.Connect();
                return modbusClient.Connected;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка подключения: " + ex.Message);
                return false;
            }
        }

        public void Disconnect()
        {
            if (modbusClient != null && modbusClient.Connected)
            {
                modbusClient.Disconnect();
            }
        }

        public bool IsConnected => modbusClient != null && modbusClient.Connected;

        public bool[] ReadCoils(int startAddress, int numberOfCoils)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Нет подключения к серверу Modbus.");

            return modbusClient.ReadCoils(startAddress, numberOfCoils);
        }

        public void WriteSingleCoil(int address, bool value)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Нет подключения к серверу Modbus.");

            modbusClient.WriteSingleCoil(address, value);
        }

        public int[] ReadHoldingRegisters(int startAddress, int numberOfRegisters)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Нет подключения к серверу Modbus.");

            return modbusClient.ReadHoldingRegisters(startAddress, numberOfRegisters);
        }

        public void WriteSingleRegister(int address, int value)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Нет подключения к серверу Modbus.");

            modbusClient.WriteSingleRegister(address, value);
        }
    }
}