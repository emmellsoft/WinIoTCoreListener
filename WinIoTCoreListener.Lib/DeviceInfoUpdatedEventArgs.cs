using System;

namespace Laserbrain.WinIoTCoreListener.Lib
{
	/// <summary>
	/// Event arguments of an updated DeviceInfo.
	/// </summary>
	public class DeviceInfoUpdatedEventArgs : EventArgs
	{
		internal DeviceInfoUpdatedEventArgs(
			DeviceInfo deviceInfo,
			UpdateStatus updateStatus)
		{
			DeviceInfo = deviceInfo;
			UpdateStatus = updateStatus;
		}

		/// <summary>
		/// The updated DeviceInfo.
		/// </summary>
		public DeviceInfo DeviceInfo { get; }

		/// <summary>
		/// The kind of update.
		/// </summary>
		public UpdateStatus UpdateStatus { get; }
	}
}