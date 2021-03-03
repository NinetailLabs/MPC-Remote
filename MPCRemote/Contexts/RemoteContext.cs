using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
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
        /// <summary>
        /// Command used to connect to the client
        /// </summary>
        public ICommand ConnectCommand => new RelayCommand(_ => Task.Run(ConnectToClient));

        /// <summary>
        /// String to provide feedback to the UI
        /// </summary>
        public string FeedbackString { get; private set; }

        /// <summary>
        /// Connect to the client
        /// </summary>
        private void ConnectToClient()
        {
            try
            {
                var serverEndpoint = new IPEndPoint(_ipAddress, _port);
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
                        FeedbackString = data;
                        
                        }
                    catch (Exception)
                    {
                        break;
                    }
                }
            }
            catch(Exception exception)
            {
                FeedbackString = exception.Message;
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
        /// Temporary placeholder for storing the IPAddress to use for connecting to the server
        /// </summary>
        private IPAddress _ipAddress = IPAddress.Parse("127.0.0.1");

        /// <summary>
        /// Temporary placeholder for storing the port used to connect to the server
        /// </summary>
        private int _port = 13580;

        /// <summary>
        /// Occurs when a property is changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
