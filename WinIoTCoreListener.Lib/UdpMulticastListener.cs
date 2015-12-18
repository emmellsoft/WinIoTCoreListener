using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;

namespace Laserbrain.WinIoTCoreListener.Lib
{
	internal class UdpMulticastListener : IDisposable
	{
		private readonly IPAddress _ipAddress;
		private const int MaxUdpSize = 0x10000;
		private const int Port = 6;
		private static readonly IPAddress GroupAddress = IPAddress.Parse("239.0.0.222");

		private readonly object _syncObj = new object();
		private bool _isDisposed;
		private readonly Task _mainTask;
		private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
		private Action _disposeSocketAction = () => { };

		public UdpMulticastListener(IPAddress ipAddress)
		{
			_ipAddress = ipAddress;

			_mainTask = Task.Factory.StartNew(MainTaskBody, _cancellationTokenSource.Token);
		}

		~UdpMulticastListener()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!disposing)
			{
				return;
			}

			lock (_syncObj)
			{
				if (_isDisposed)
				{
					return;
				}

				_isDisposed = true;

				_cancellationTokenSource.Cancel();
				_disposeSocketAction();

				_mainTask.Wait();
			}
		}

		public event EventHandler<ReceivedBytesEventArgs> OnReceivedBytes;

		private async Task MainTaskBody()
		{
			Socket socket;
			var syncLock = new object();

			Action disposeSocket = () => { };

			try
			{
				socket = CreateUdpSocket();

				disposeSocket = () =>
				{
					lock (syncLock)
					{
						if (socket != null)
						{
							socket.Dispose();
							socket = null;
						}
					}
				};

				if (socket != null)
				{
					_disposeSocketAction = disposeSocket;

					await ReceiveLoop(socket);
				}
			}
			finally
			{
				disposeSocket();
			}
		}

		private Socket CreateUdpSocket()
		{
			Socket socket = null;

			try
			{
				socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

				var endPoint = new IPEndPoint(_ipAddress, Port);

				socket.Bind(endPoint);

				socket.SetSocketOption(
					SocketOptionLevel.IP,
					SocketOptionName.AddMembership,
					new MulticastOption(GroupAddress, _ipAddress));
			}
			catch
			{
				if (socket != null)
				{
					socket.Dispose();
					socket = null;
				}
			}

			return socket;
		}

		private async Task ReceiveLoop(Socket socket)
		{
			var buffer = new byte[MaxUdpSize];

			while (!_cancellationTokenSource.IsCancellationRequested)
			{
				try
				{
					int receivedLength = await socket.ReceiveAsync(buffer, 0, buffer.Length);

					if (receivedLength == 0)
					{
						return;
					}

					if (receivedLength < 0)
					{
						continue;
					}

					var receivedBytes = new byte[receivedLength];

					Array.Copy(buffer, receivedBytes, receivedLength);

					OnReceivedBytes?.BeginInvoke(
						this,
						new ReceivedBytesEventArgs(receivedBytes),
						ar =>
						{
							try
							{
								var asyncResult = (AsyncResult)ar;
								var eventHandler = (EventHandler<ReceivedBytesEventArgs>)asyncResult.AsyncDelegate;
								eventHandler.EndInvoke(ar);
							}
							catch
							{
							}
						},
						null);
				}
				catch
				{
				}
			}
		}
	}
}