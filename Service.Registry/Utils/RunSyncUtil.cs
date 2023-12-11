using System;
using System.Threading.Tasks;

namespace Service.Registry.Utils
{
	public static class RunSyncUtil
	{
		private static readonly TaskFactory factory = new
			TaskFactory(default,
				TaskCreationOptions.None,
				TaskContinuationOptions.None,
				TaskScheduler.Default);

		public static void RunSync(Func<Task> task)
			=> factory.StartNew(task).Unwrap().GetAwaiter().GetResult();

		public static T RunSync<T>(Func<Task<T>> task)
			=> factory.StartNew(task).Unwrap().GetAwaiter().GetResult();

		
	}
}
