using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Laserbrain.WinIoTCoreListener.Lib
{
	internal static class SocketAsyncExtensions
	{
		public static Task<int> ReceiveAsync(this Socket socket, byte[] buffer, int offset, int size, SocketFlags socketFlags = SocketFlags.None)
		{
			return Task.Factory.FromAsync(
				(cb, s) => socket.BeginReceive(buffer, offset, size, socketFlags, cb, s),
				ias => EndReceive(socket, ias),
				null);
		}

		private static int EndReceive(Socket socket, IAsyncResult ias)
		{
			try
			{
				return socket.EndReceive(ias);
			}
			catch
			{
				return 0;
			}
		}
	}
}