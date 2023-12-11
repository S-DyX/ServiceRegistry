namespace Service.Registry.Common.Entities
{
	public abstract class TcpClientResponseBase
	{
		protected readonly TcpClientResponseSettings Settings;

		protected TcpClientResponseBase(TcpClientResponseSettings settings)
		{
			Settings = settings;
		}

	}
}
