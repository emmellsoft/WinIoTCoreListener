using System;

namespace Laserbrain.WinIoTCoreListener.Lib
{
	internal class ReceivedBytesEventArgs : EventArgs
	{
		public ReceivedBytesEventArgs(byte[] bytes)
		{
			Bytes = bytes;
		}

		public byte[] Bytes { get; }
	}
}