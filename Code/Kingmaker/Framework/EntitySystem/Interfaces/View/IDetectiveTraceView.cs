using Kingmaker.EntitySystem.Interfaces;

namespace Kingmaker.Framework.EntitySystem.Interfaces.View;

public interface IDetectiveTraceView : IEntityView
{
	void OnStatusLoad(bool inCombat);

	void OnStatusChanged(bool inCombat);
}
