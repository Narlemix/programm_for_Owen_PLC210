# programm_for_Owen_PLC210
This project is a Windows-based application for controlling and monitoring devices via Modbus TCP/RTU, designed to work with the OVEN PLC 210. It provides a user-friendly WPF interface to send control signals, manage strobe parameters (frequency, duty cycle, type), and communicate with coils and registers of the PLC.
🚀 Features
🌐 Modbus TCP/RTU communication using both NModbus and EasyModbus libraries

📟 Strobe generator with configurable frequency, duty cycle, and signal inversion

⚡ Pin control with pulse sending and global reset

🖥️ WPF interface for real-time monitoring and manual control

📋 Live event log with status feedback

🧪 Designed for integration with OVEN PLC 210 or compatible Modbus devices

🛠 Technologies Used
Language: C# (.NET 6 / .NET Framework)

UI: WPF

Communication: Modbus TCP/RTU

Libraries: NModbus, EasyModbusTCP.NET

📦 Project Structure
MainWindow.xaml/.cs: WPF interface and UI logic

ModbusClientManager.cs: Handles Modbus communication

StrobeWorker.cs: Background task for generating strobe signals

StrobeSettings.cs: User-defined strobe parameters

BoolToColorConverter.cs: UI value converter for status colors

📷 Screenshots
(You can add screenshots of the app interface here)

🧭 How It Works
User enters the IP address and port of the Modbus device (e.g., OVEN PLC 210).

After connecting, the app retrieves available pins and strobes.

User can:

Send pulse signals to individual pins

Start/stop a strobe signal with specific frequency and duty cycle

The app logs all actions and provides visual feedback on connection and signal status.

⚙️ Requirements
OVEN PLC 210 or other Modbus-compatible device

Windows 10/11

.NET 6 SDK (or modify to work with .NET Framework 4.x if required)
