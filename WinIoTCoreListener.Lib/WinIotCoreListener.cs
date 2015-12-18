using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Threading;

namespace Laserbrain.WinIoTCoreListener.Lib
{
	internal class WinIotCoreListener : IWinIotCoreListener
	{
		private class TimestampedDeviceInfo
		{
			public TimestampedDeviceInfo(DeviceInfo deviceInfo)
			{
				DeviceInfo = deviceInfo;
				LastUpdate = DateTime.UtcNow;
			}

			public DeviceInfo DeviceInfo { get; set; }

			public DateTime LastUpdate { get; set; }
		}

		private static readonly TimeSpan CleanupCacheInterval = TimeSpan.FromSeconds(5);
		private static readonly TimeSpan TimeoutDuration = TimeSpan.FromSeconds(16);

		private readonly UdpMulticastListener[] _udpMulticastListeners;
		private readonly object _syncObj = new object();
		private readonly Dictionary<string, TimestampedDeviceInfo> _cache;
		private readonly Timer _cleanupCacheTimer;
		private bool _isDisposed;

		public WinIotCoreListener()
		{
			_cache = new Dictionary<string, TimestampedDeviceInfo>();

			_cleanupCacheTimer = new Timer(CleanupOldDevices, null, CleanupCacheInterval, CleanupCacheInterval);

			_udpMulticastListeners = GetUdpMulticastListeners().ToArray();
		}

		~WinIotCoreListener()
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

				_cleanupCacheTimer.Dispose();

				foreach (var udpMulticastListener in _udpMulticastListeners)
				{
					udpMulticastListener.Dispose();
				}

				_cache.Clear();
			}
		}

		public event EventHandler<DeviceInfoUpdatedEventArgs> OnDeviceInfoUpdated;

		public IEnumerable<DeviceInfo> DeviceInfos
		{
			get
			{
				lock (_syncObj)
				{
					return _cache.Values.Select(x => x.DeviceInfo);
				}
			}
		}

		private IEnumerable<UdpMulticastListener> GetUdpMulticastListeners()
		{
			foreach (IPAddress ipAddress in NetworkSupport.GetIpAddresses())
			{
				var udpMulticastListener = new UdpMulticastListener(ipAddress);
				udpMulticastListener.OnReceivedBytes += UdpMulticastListener_OnReceivedBytes;
				yield return udpMulticastListener;
			}
		}

		private void UdpMulticastListener_OnReceivedBytes(object sender, ReceivedBytesEventArgs e)
		{
			DeviceInfo deviceInfo;
			if (DeviceInfoParser.TryParse(e.Bytes, out deviceInfo))
			{
				UpdateStatus updateStatus;
				if (TryUpdateDictionary(deviceInfo, out updateStatus))
				{
					FireDeviceInfoUpdated(deviceInfo, updateStatus);
				}
			}
		}

		private bool TryUpdateDictionary(DeviceInfo deviceInfo, out UpdateStatus updateStatus)
		{
			lock (_syncObj)
			{
				if (_isDisposed)
				{
					updateStatus = UpdateStatus.Lost;
					return false;
				}

				TimestampedDeviceInfo existing;
				if (_cache.TryGetValue(deviceInfo.MacAddressString, out existing))
				{
					updateStatus = UpdateStatus.Updated;
					existing.LastUpdate = DateTime.UtcNow;

					if (existing.DeviceInfo.Equals(deviceInfo))
					{
						// Nothing has changed.
						return false;
					}

					existing.DeviceInfo = deviceInfo;
				}
				else
				{
					updateStatus = UpdateStatus.Found;
					_cache[deviceInfo.MacAddressString] = new TimestampedDeviceInfo(deviceInfo);
				}
			}

			return true;
		}

		private void CleanupOldDevices(object state)
		{
			DeviceInfo[] disappearedDeviceInfoArray;

			lock (_syncObj)
			{
				if (_isDisposed)
				{
					return;
				}

				DateTime timeoutAt = DateTime.UtcNow.Subtract(TimeoutDuration);

				disappearedDeviceInfoArray = _cache
					.Where(kvp => kvp.Value.LastUpdate < timeoutAt)
					.Select(kvp => kvp.Value.DeviceInfo)
					.ToArray();

				foreach (DeviceInfo deviceInfo in disappearedDeviceInfoArray)
				{
					_cache.Remove(deviceInfo.MacAddressString);
				}
			}

			foreach (DeviceInfo deviceInfo in disappearedDeviceInfoArray)
			{
				FireDeviceInfoUpdated(deviceInfo, UpdateStatus.Lost);
			}
		}

		private void FireDeviceInfoUpdated(DeviceInfo deviceInfo, UpdateStatus updateStatus)
		{
			OnDeviceInfoUpdated?.BeginInvoke(
				this,
				new DeviceInfoUpdatedEventArgs(deviceInfo, updateStatus),
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
	}
}