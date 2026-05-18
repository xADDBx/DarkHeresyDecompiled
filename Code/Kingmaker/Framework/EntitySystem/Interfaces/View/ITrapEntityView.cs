using Kingmaker.EntitySystem.Interfaces;

namespace Kingmaker.Framework.EntitySystem.Interfaces.View;

public interface ITrapEntityView : IEntityView
{
	void OnDeactivated();

	void OnTriggered();

	void OnDisarmFailed();

	void OnDisarmed();
}
