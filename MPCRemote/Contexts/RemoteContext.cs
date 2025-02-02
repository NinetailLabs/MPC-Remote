﻿using GongSolutions.Wpf.DragDrop;
using Microsoft.Win32;
using MPCRemote.Enumerations;
using MPCRemote.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using VaraniumSharp.Shenfield;

namespace MPCRemote
{
    /// <summary>
    /// Class that serves as the context for the MainWindow
    /// </summary>
    public sealed class RemoteContext : INotifyPropertyChanged, IDropTarget
    {
        public RemoteContext()
        {
            // TODO - For quick access
            _ipAddress = "127.0.0.1";
            Port = 13580;
            SetButtonState();
            FeedbackString = string.Empty;
            ApiVersion = "0.0.0";
            FullscreenText = "Fullscreen";
            DurationInMilliseconds = 100;
            File = string.Empty;
            PlayerState = string.Empty;
            Position = string.Empty;

            Playlist = new ObservableCollection<PlaylistEntry>();
            BindingOperations.EnableCollectionSynchronization(Playlist, _playlistLock);
        }

        /// <summary>
        /// Playlist
        /// </summary>
        public ObservableCollection<PlaylistEntry> Playlist { get; }

        /// <summary>
        /// The entry the user has selected in the playlist
        /// </summary>
        public PlaylistEntry? SelectedEntry 
        {
            get => _selectedEntry;
            set
            {
                _selectedEntry = value;
                SetButtonState();
            }
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
        /// Command to request removal of an item from the playlist
        /// </summary>
        public ICommand RemoveEntryFromPlaylistCommand => new RelayCommand(_ => Task.Run(RemoveEntryFromPlaylist));

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
        /// Indicate if the remove button should be enabled
        /// </summary>
        public bool EnableRemoveButton { get; private set; }

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
            SendComamndToClient(MpcCommands.SeekTo, newPosition.ToString());
        }

        /// <summary>
        /// Play the file at the selected index
        /// </summary>
        public void PlayFileInPlaylist(int indexToPlay)
        {
            var file = Playlist[indexToPlay];
            SendComamndToClient(MpcCommands.PlayPlaylistFile, file.Filename);
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
                && PlayerState != PlayerStates.Loading
                && PlayerState != PlayerStates.Playing
                && PlayerState != PlayerStates.Closed;

            EnablePause = IsConnected
                && PlayerState == PlayerStates.Playing;

            EnableStop = IsConnected
                && (PlayerState == PlayerStates.Playing
                    || PlayerState == PlayerStates.Paused);

            EnableProgressBar = IsConnected
                && (PlayerState == PlayerStates.Playing
                    || PlayerState == PlayerStates.Paused);

            EnableRemoveButton = IsConnected
                && SelectedEntry != null;
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
                            FeedbackString += "Connection closed by host\r\n";
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
        /// Remove the <see cref="SelectedEntry"/> from the playlist
        /// </summary>
        private void RemoveEntryFromPlaylist()
        {
            var index = Playlist.IndexOf(SelectedEntry);
            SendComamndToClient(MpcCommands.RemovePlaylistEntry, string.Empty, index);
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
            Playlist.Clear();
            SetButtonState();
        }

        /// <summary>
        /// Disconnect from the host
        /// </summary>
        private void DisconnectFromHost()
        {
            _server?.Close();
        }

        /// <summary>
        /// Handle commands being returned from MPC
        /// </summary>
        /// <param name="command"></param>
        private void HandleCommand(MpcReceiveCommand command)
        {
            switch(command.Command)
            {
                case "API.Connection":
                    IsConnected = command.Parameters.Connected;
                    SetButtonState();
                    if(IsConnected)
                    {
                        SendComamndToClient(MpcCommands.GetApiVersion, string.Empty);
                    }
                    break;
                case "Player.StateChanged":
                    HandlePlaybackStateChange(command);
                    break;
                case "Player.Position":
                    HandlePosition(command);
                    break;
                case "API.Version":
                    ApiVersion = command.Parameters.Version;
                    SendComamndToClient(MpcCommands.GetPlayerStatus, string.Empty);
                    break;
                case "Player.Fullscreen":
                    FullscreenText = command.Parameters.IsFullscreen
                        ? "Window"
                        : "Fullscreen";
                    break;
                case "Playlist":
                    HandlePlaylistChange(command);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Handle the update of the playlist when it changes
        /// </summary>
        /// <param name="command">Command containing the playlist details</param>
        private void HandlePlaylistChange(MpcReceiveCommand command)
        {
            lock(_playlistLock)
            {
                Playlist.Clear();

                foreach(var item in command.Parameters.Playlist)
                {
                    var entry = new PlaylistEntry
                    {
                        Filename = item
                    };
                    Playlist.Add(entry);
                }
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
        /// <param name="index">Index if it should be sent</param>
        /// <param name="targetIndex">Index to target when moving an item</param>
        private void SendComamndToClient(string command, string parameters, int index = 0, int targetIndex = 0)
        {
            try
            {
                var commandEntry = new MpcCommand
                {
                    Command = command,
                    Parameters = parameters,
                    Index = index,
                    TargetIndex = targetIndex
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
        /// Occurs when the user drags something over the listview
        /// </summary>
        /// <param name="dropInfo">DropInfo instance</param>
        public void DragOver(IDropInfo dropInfo)
        {
            if(!IsConnected)
            {
                return;
            }

            // Check if the dropped entry is a file from Windows explorer
            var dataObject = dropInfo.Data as IDataObject;
            if (dataObject != null)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                dropInfo.Effects = DragDropEffects.Copy;
                return;
            }

            if(dropInfo.Data is PlaylistEntry sourceData && dropInfo.TargetItem is PlaylistEntry targetData)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        /// <summary>
        /// Occurs when the user drops something over the listview
        /// </summary>
        /// <param name="dropInfo"></param>
        public void Drop(IDropInfo dropInfo)
        {
            if (!IsConnected)
            {
                return;
            }

            // Handle file drop from Windows explorer
            var dataObject = dropInfo.Data as DataObject;
            if (dataObject != null)
            {
                if (dataObject != null && dataObject.ContainsFileDropList())
                {
                    var files = dataObject.GetFileDropList();
                    var insertIndex = dropInfo.InsertIndex;
                    foreach (var file in files)
                    {
                        SendComamndToClient(MpcCommands.InsertPlaylistEntry, file, insertIndex);
                        Thread.Sleep(100);
                        insertIndex++;
                    }
                }

                return;
            }

            if (dropInfo.Data is PlaylistEntry sourceData && dropInfo.TargetItem is PlaylistEntry targetData)
            {
                var sourceIndex = Playlist.IndexOf(sourceData);
                var targetIndex = Playlist.IndexOf(targetData);

                if(sourceIndex < 0 || targetIndex < 0)
                {
                    return;
                }

                if (sourceData.Filename != targetData.Filename)
                {
                    SendComamndToClient(MpcCommands.MovePlaylistEntry, string.Empty, sourceIndex, targetIndex);
                }
            }
        }

        /// <summary>
        /// Socket used for connecting to the server
        /// </summary>
        private Socket? _server;

        /// <summary>
        /// Writer used to write data to the remote endpoint
        /// </summary>
        private StreamWriter? _streamWriter;

        /// <summary>
        /// Backing variable for <see cref="IPAddress"/>
        /// </summary>
        private string _ipAddress;

        /// <summary>
        /// Backing variable for <see cref="Port"/>
        /// </summary>
        private int? _port;

        /// <summary>
        /// Object used to lock access to the Playlist
        /// </summary>
        private readonly object _playlistLock = new object();

        /// <summary>
        /// Backing variable for <see cref="SelectedEntry"/>
        /// </summary>
        private PlaylistEntry? _selectedEntry;

        /// <summary>
        /// Occurs when a property is changed
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
