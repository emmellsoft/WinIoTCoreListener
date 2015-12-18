namespace Laserbrain.WinIoTCoreListener.Lib
{
	/// <summary>
	/// Different kinds of updates.
	/// </summary>
	public enum UpdateStatus
	{
		/// <summary>
		/// The device was just found.
		/// </summary>
		Found,

		/// <summary>
		/// Some of the device information has been changed.
		/// </summary>
		Updated,

		/// <summary>
		/// The device has been lost (i.e. has not refreshed its info within a certain time period).
		/// </summary>
		Lost
	}
}