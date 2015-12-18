using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Laserbrain.WinIoTCoreListener.Lib
{
	internal static class NetworkSupport
	{
		public static IEnumerable<IPAddress> GetIpAddresses()
		{
			return NetworkInterface
				.GetAllNetworkInterfaces()
				.Where(networkInterface =>
					networkInterface.Supports(NetworkInterfaceComponent.IPv4) &&
					networkInterface.OperationalStatus == OperationalStatus.Up)
				.SelectMany(networkInterface => networkInterface.GetIPProperties().UnicastAddresses)
				.Where(unicastIpAddressInformation => unicastIpAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
				.Select(unicastIpAddressInformation => unicastIpAddressInformation.Address);
		}
	}
}
