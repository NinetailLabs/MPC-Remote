﻿using Microsoft.Win32;
using MPCRemote.Enumerations;
using MPCRemote.Models;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using VaraniumSharp.Shenfield;

namespace MPCRemote
{
    /// <summary>
    /// Class that serves as the context for the MainWindow
    /// </summary>
    public sealed class RemoteContext : INotifyPropertyChanged
    {
        public RemoteContext()
        {
            // TODO - For quick access
            IpAddress = "127.0.0.1";
            Port = 13580;
            SetButtonState();
        }

        /// <summary>
        /// Command used to connect to the client
        /// </summary>
        public ICommand ConnectCommand => new RelayCommand(_ => Task.Run(ConnectToClient));

        /// <summary>
        /// Command to open a file in the client
        /// </summary>
        public ICommand OpenFileCommand => new RelayCommand(_ => Task.Run(OpenFileInClient));

        public ICommand NoParameterCommand => new RelayCommand(o => Task.Run(() => SendComamndToClient(o.ToString(), string.Empty)));

        /// <summary>
        /// String to provide feedback to the UI
        /// </summary>
        public string FeedbackString { get; private set; }

        /// <summary>
        /// Indicate if the client is connected or not
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Current file open in MPC
        /// </summary>
        public string File { get; private set; }

        /// <summary>
        /// The current MPC status
        /// </summary>
        public string PlayerState { get; private set; }

        /// <summary>
        /// The current playback position
        /// </summary>
        public string Position { get; private set; }

        /// <summary>
        /// The IP address of the server to connect to
        /// </summary>
        public string IpAddress
        {
            get => _ipAddress;
            set
            {
                _ipAddress = value;
                SetButtonState();
            }
        }

        /// <summary>
        /// The port to connect to
        /// </summary>
        public int? Port 
        {
            get => _port;
            set
            {
                _port = value;
                SetButtonState();
            }
        }

        /// <summary>
        /// Indicate if the connect button should be enabled
        /// </summary>
        public bool EnableConnectButton { get; private set; }

        /// <summary>
        /// Sets the state of the buttons
        /// </summary>
        private void SetButtonState()
        {
            EnableConnectButton = !string.IsNullOrEmpty(IpAddress)
                && Port != null
                && !IsConnected;
        }

        /// <summary>
        /// Connect to the client
        /// </summary>
        private void ConnectToClient()
        {
            try
            {
                var ipAddrress = IPAddress.Parse(IpAddress);
                var serverEndpoint = new IPEndPoint(ipAddrress, Port.Value);
                _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _server.Connect(serverEndpoint);
                NetworkStream nStream = new NetworkStream(_server);
                StreamReader reader = new StreamReader(nStream);
                _streamWriter = new StreamWriter(nStream);

                while (true)
                {
                    try
                    {
                        var data = reader.ReadLine();
                        var command = JsonSerializer.Deserialize<MpcReceiveCommand>(data);
                        HandleCommand(command);
                    }
                    catch (Exception exception)
                    {
                        // TODO - Log and feed back
                    }
                }
            }
            catch(Exception exception)
            {
                FeedbackString = exception.Message;
            }
        }

        /// <summary>
        /// Handle commands being returned from MPC
        /// </summary>
        /// <param name="command"></param>
        private void HandleCommand(MpcReceiveCommand command)
        {
            switch(command.Command)
            {
                case "Connection":
                    IsConnected = command.Parameters.Connected;
                    SetButtonState();
                    break;
                case "Status":
                    HandleStatus(command);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Handle the status response
        /// </summary>
        /// <param name="command">Command containing the status response</param>
        private void HandleStatus(MpcReceiveCommand command)
        {
            var parameters = command.Parameters;
            File = parameters.File;
            PlayerState = parameters.State;

            if(long.TryParse(parameters.Position, out var position) && long.TryParse(parameters.Duration, out var duration))
            {
                var positionSpan = TimeSpan.FromMilliseconds(position);
                var durationSpan = TimeSpan.FromMilliseconds(duration);
                Position = $"{positionSpan.Hours:00}:{positionSpan.Minutes:00}:{positionSpan.Seconds:00}\\{durationSpan.Hours:00}:{durationSpan.Minutes:00}:{durationSpan.Seconds:00}";
            }
        }

        /// <summary>
        /// Send a command to the client
        /// </summary>
        /// <param name="command">Command to send</param>
        /// <param name="parameters">Parameters for the command</param>
        private void SendComamndToClient(string command, string parameters)
        {
            try
            {
                var commandEntry = new MpcCommand
                {
                    Command = command,
                    Parameters = parameters
                };

                var jsonString = JsonSerializer.Serialize(commandEntry);

                _streamWriter.WriteLine(jsonString);
                _streamWriter.Flush();
            }
            catch(Exception exception)
            {
                FeedbackString = exception.Message;
            }
        }

        /// <summary>
        /// Shows dialog requesting the file to open.
        /// If a file is selected the file will be passed to the client
        /// </summary>
        private void OpenFileInClient()
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                // TODO - Handle sending of the file
                SendComamndToClient(MpcCommands.OpenFile, dialog.FileName);
            }
        }

        /// <summary>
        /// Socket used for connecting to the server
        /// </summary>
        private Socket _server;

        /// <summary>
        /// Writer used to write data to the remote endpoint
        /// </summary>
        private StreamWriter _streamWriter;

        /// <summary>
        /// Backing variable for <see cref="IPAddress"/>
        /// </summary>
        private string _ipAddress;

                /// <summary>
        /// Backing variable for <see cref="Port"/>
        /// </summary>
        private int? _port;

        /// <summary>
        /// Occurs when a property is changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
