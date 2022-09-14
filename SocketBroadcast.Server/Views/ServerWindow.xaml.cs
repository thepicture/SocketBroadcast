using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SocketBroadcast.Server
{
    /// <summary>
    /// Interaction logic for ServerWindow.xaml
    /// </summary>
    public partial class ServerWindow : Window
    {
        private Socket _socket;

        public ServerWindow()
        {
            InitializeComponent();
            IpV4Box.Text = IPAddress.Loopback.ToString();
            PortBox.Text = 12345.ToString();
        }

        private void OnPortChanged(object sender, TextCompositionEventArgs e)
        {
            if (!e.Text.All(char.IsDigit))
                e.Handled = true;
        }

        private void OnAddressChanged(object sender, TextCompositionEventArgs e)
        {
            if (!e.Text.All(c => char.IsDigit(c) || c == '.'))
                e.Handled = true;
        }

        private void OnListen(object sender, RoutedEventArgs e)
        {
            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                EndPoint endPoint = new IPEndPoint(IPAddress.Parse(IpV4Box.Text), int.Parse(PortBox.Text));
                _socket.Bind(endPoint);
                _socket.Listen(1);
                Task.Run(() =>
                {
                    try
                    {
                        Socket socket = _socket.Accept();
                        while (true)
                        {
                            try
                            {
                                byte[] buffer = new byte[1024];
                                socket.Receive(buffer);
                                socket.Send(buffer);
                            }
                            catch
                            {
                                break;
                            }
                        }
                    }
                    catch { }
                });
                ListenButton.IsEnabled = false;
                StopListeningButton.IsEnabled = true;
                MessageBox.Show("Listening");
            }
            catch (Exception ex)
            {
                ListenButton.IsEnabled = true;
                StopListeningButton.IsEnabled = false;
                MessageBox.Show("Cannot listen. " + ex);
            }
        }

        private void OnStopListening(object sender, RoutedEventArgs e)
        {
            try
            {
                _socket.Dispose();
                ListenButton.IsEnabled = true;
                StopListeningButton.IsEnabled = false;
                MessageBox.Show("Stopped listening");
            }
            catch (Exception ex)
            {
                ListenButton.IsEnabled = false;
                StopListeningButton.IsEnabled = true;

                MessageBox.Show("Cannot stop listening. " + ex);
            }
        }
    }
}
