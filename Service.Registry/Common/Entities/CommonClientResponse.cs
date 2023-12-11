namespace Service.Registry.Common.Entities
{

	public class CommonClientResponse
	{
		public string Id { get; set; }

		public string Host { get; set; }
		public string Data { get; set; }
        public string Address { get; set; }

        public int? Port { get; set; }
		public int CallCount { get; set; }

	}
}
