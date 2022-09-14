using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SocketBroadcast.Client
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        private Socket _socket;

        public ChatWindow()
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

        private void OnConnect(object sender, RoutedEventArgs e)
        {
            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress address = IPAddress.Parse(IpV4Box.Text);
                _socket.Connect(address, int.Parse(PortBox.Text));
                Task.Run(() =>
                {
                    while (true)
                    {
                        try
                        {
                            byte[] buffer = new byte[1024];
                            _socket.Receive(buffer);
                            Dispatcher.Invoke(() => MessagesBox.Text += Encoding.UTF8.GetString(buffer) + '\n');
                        }
                        catch
                        {
                            break;
                        }
                    }
                });
                ConnectButton.IsEnabled = false;
                DisconnectButton.IsEnabled = true;
                ButtonSend.IsEnabled = true;
                MessageBox.Show("Connected");
            }
            catch (Exception ex)
            {
                ConnectButton.IsEnabled = true;
                DisconnectButton.IsEnabled = false;
                ButtonSend.IsEnabled = false;
                MessageBox.Show("Cannot connect. " + ex);
            }
        }

        private void OnDisconnect(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_socket.Connected)
                    _socket.Close();
                _socket.Dispose();
                ConnectButton.IsEnabled = true;
                DisconnectButton.IsEnabled = false;
                ButtonSend.IsEnabled = false;
                MessageBox.Show("Disconnected");
            }
            catch (Exception ex)
            {
                ConnectButton.IsEnabled = false;
                DisconnectButton.IsEnabled = true;
                ButtonSend.IsEnabled = true;
                MessageBox.Show("Cannot discconnect. " + ex);
            }
        }

        private void OnMessageSend(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextBoxMessage.Text))
                return;
            byte[] buffer = Encoding.UTF8.GetBytes($"{Environment.MachineName}: {TextBoxMessage.Text}");
            try
            {
                _socket.Send(buffer);
                TextBoxMessage.Text = string.Empty;
            }
            catch { }
        }
    }
}
