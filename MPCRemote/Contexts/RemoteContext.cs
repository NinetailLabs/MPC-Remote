using Microsoft.Win32;
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
            FeedbackString = string.Empty;
            ApiVersion = "0.0.0";
            FullscreenText = "Fullscreen";
            DurationInMilliseconds = 100;
        }

        /// <summary>
        /// Command used to connect to the client
        /// </summary>
        public ICommand ConnectCommand => new RelayCommand(_ => Task.Run(ConnectToClient));

        /// <summary>
        /// Command used to disconnect the client
        /// </summary>
        public ICommand DisconnectCommand => new RelayCommand(_ => Task.Run(DisconnectFromHost));

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
        /// Enable the Play button
        /// </summary>
        public bool EnablePlay { get; private set; }

        /// <summary>
        /// Enable the Stop button
        /// </summary>
        public bool EnableStop { get; private set; }

        /// <summary>
        /// Enable the pause button
        /// </summary>
        public bool EnablePause { get; private set; }

        /// <summary>
        /// Enable the progress bar
        /// </summary>
        public bool EnableProgressBar { get; private set; }

        /// <summary>
        /// The API version that is being connected to
        /// </summary>
        public string ApiVersion { get; private set; }

        /// <summary>
        /// Text for the fullscreen button
        /// </summary>
        public string FullscreenText { get; private set; }

        /// <summary>
        /// The current playback position
        /// </summary>
        public long PositionInMilliseconds { get; set; }

        /// <summary>
        /// The duration of the file
        /// </summary>
        public long DurationInMilliseconds { get; set; }

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
        /// Request the media player to seek to the specified position
        /// </summary>
        /// <param name="newPosition">The position to seek to</param>
        public void SeekToPosition(long newPosition)
        {
            SendComamndToClient("SeekTo", newPosition.ToString());
        }

        /// <summary>
        /// Sets the state of the buttons
        /// </summary>
        private void SetButtonState()
        {
            EnableConnectButton = !string.IsNullOrEmpty(IpAddress)
                && Port != null
                && !IsConnected;

            EnablePlay = IsConnected
                && PlayerState != "Loading"
                && PlayerState != "Playing"
                && PlayerState != "Closed";

            EnablePause = IsConnected
                && PlayerState == "Playing";

            EnableStop = IsConnected
                && (PlayerState == "Playing"
                    || PlayerState == "Paused");

            EnableProgressBar = IsConnected
                && (PlayerState == "Playing"
                    || PlayerState == "Paused");
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
                        Task.Run(() =>
                        {
                            var command = JsonSerializer.Deserialize<MpcReceiveCommand>(data);
                            HandleCommand(command);
                        });
                    }
                    catch(Exception exception) when (exception.InnerException is SocketException)
                    {
                        if (exception.InnerException.Message.Contains("closed", StringComparison.InvariantCultureIgnoreCase))
                        {
                            FeedbackString += "Connection closed by host";
                            CleanupConnection();
                            break;
                        }
                    }
                    catch (Exception exception) when (exception.InnerException is ObjectDisposedException)
                    {
                        FeedbackString += "Connection closed\r\n";
                        CleanupConnection();
                        return;
                    }
                    catch (Exception exception)
                    {
                        FeedbackString += $"{exception.Message}\r\n";
                    }
                }
            }
            catch(Exception exception)
            {
                FeedbackString += $"{exception.Message}\r\n";
            }
        }

        /// <summary>
        /// Clean up variables when the connection is closed
        /// </summary>
        private void CleanupConnection()
        {
            IsConnected = false;
            File = string.Empty;
            Position = string.Empty;
            PlayerState = string.Empty;
            ApiVersion = "0.0.0";
            DurationInMilliseconds = 100;
            SetButtonState();
        }

        /// <summary>
        /// Disconnect from the host
        /// </summary>
        private void DisconnectFromHost()
        {
            SendComamndToClient("OSD", "Remote disconnected");
            _server.Close();
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
                    if(IsConnected)
                    {
                        SendComamndToClient("GetAPIVersion", string.Empty);
                    }
                    break;
                case "PlaybackStateChange":
                    HandlePlaybackStateChange(command);
                    break;
                case "Position":
                    HandlePosition(command);
                    break;
                case "APIVersion":
                    ApiVersion = command.Parameters.Version;
                    SendComamndToClient("GetCurrentStatus", string.Empty);
                    break;
                case "Fullscreen":
                    FullscreenText = command.Parameters.IsFullscreen
                        ? "Window"
                        : "Fullscreen";
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Handle the status response
        /// </summary>
        /// <param name="command">Command containing the status response</param>
        private void HandlePosition(MpcReceiveCommand command)
        {
            var parameters = command.Parameters;

            if(long.TryParse(parameters.Position, out var position) && long.TryParse(parameters.Duration, out var duration))
            {
                var positionSpan = TimeSpan.FromMilliseconds(position);
                var durationSpan = TimeSpan.FromMilliseconds(duration);
                Position = $"{positionSpan.Hours:00}:{positionSpan.Minutes:00}:{positionSpan.Seconds:00}\\{durationSpan.Hours:00}:{durationSpan.Minutes:00}:{durationSpan.Seconds:00}";
                
                var newProgress = (int)(Math.Round((position / (decimal)duration) * 100, 0));
                DurationInMilliseconds = duration;
                PositionInMilliseconds = position;
            }
        }

        /// <summary>
        /// Handle the playback state change command
        /// </summary>
        /// <param name="command">Command containing the state change response</param>
        private void HandlePlaybackStateChange(MpcReceiveCommand command)
        {
            var parameters = command.Parameters;
            File = parameters.File;
            PlayerState = parameters.State;
            SetButtonState();
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
                FeedbackString += $"{exception.Message}\r\n";
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
