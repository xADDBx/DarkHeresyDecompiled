using Kingmaker.View.Spawners;

namespace Kingmaker.View.Mechanics;

public abstract class MechanicGroupView<T> : AbstractEntityGroupView where T : MechanicEntityView
{
	public override bool CreatesDataOnLoad => true;

	public new MechanicGroupEntity Data => (MechanicGroupEntity)base.Data;

	public virtual void Activate(bool flag)
	{
		Data.IsInGame = flag;
	}
}
