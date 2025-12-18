using System.Threading;
using System.Threading.Tasks;

namespace Kingmaker.Code.UI.MVVM;

public static class BarkHandleExtensions
{
	public static async Task ToAsync(this IBarkHandle handle, CancellationToken ct = default(CancellationToken))
	{
		while (handle.IsPlayingBark())
		{
			if (ct.IsCancellationRequested)
			{
				handle.InterruptBark();
			}
			await Task.Yield();
		}
	}
}
