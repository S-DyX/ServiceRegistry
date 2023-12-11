namespace Service.Registry.Common.Entities
{
	/// <summary>
	/// Common object contains client configuration 
	/// </summary>
	public sealed class ServiceClient
	{
		/// <summary>
		/// Name
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Host
		/// </summary>
		public string Host { get; set; }

		/// <summary>
		/// Address
		/// </summary>
		public string Address { get; set; }

		/// <summary>
		/// Port
		/// </summary>
		public int? Port { get; set; }

		/// <summary>
		/// How many calls
		/// </summary>
		public int CallCount { get; set; }
	}
}
