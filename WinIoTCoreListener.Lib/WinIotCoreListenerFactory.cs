namespace Laserbrain.WinIoTCoreListener.Lib
{
	/// <summary>
	/// Factory creating IWinIotCoreListeners.
	/// </summary>
	public static class WinIotCoreListenerFactory
	{
		/// <summary>
		/// Create a new IWinIotCoreListener.
		/// </summary>
		public static IWinIotCoreListener Create()
		{
			return new WinIotCoreListener();
		}
	}
}