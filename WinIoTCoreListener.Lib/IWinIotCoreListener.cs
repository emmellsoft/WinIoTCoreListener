using System;
using System.Collections.Generic;

namespace Laserbrain.WinIoTCoreListener.Lib
{
	/// <summary>
	/// Listens for WinIoTCore devices.
	/// Maintains a list of found devices and fires an event when the list is updated.
	/// </summary>
	public interface IWinIotCoreListener : IDisposable
	{
		/// <summary>
		/// An event fired when a device is either found, updated or lost.
		/// </summary>
		event EventHandler<DeviceInfoUpdatedEventArgs> OnDeviceInfoUpdated;

		/// <summary>
		/// Enumerable containing the found devices.
		/// </summary>
		IEnumerable<DeviceInfo> DeviceInfos { get; }
	}
}