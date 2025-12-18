using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Kingmaker.UnitLogic.Commands.Base;

public static class UnitCommandHandleExtensions
{
	public static TaskAwaiter GetAwaiter([CanBeNull] this UnitCommandHandle handle)
	{
		return handle?.ToTask().GetAwaiter() ?? Task.CompletedTask.GetAwaiter();
	}
}
