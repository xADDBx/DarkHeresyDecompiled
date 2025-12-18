using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Mechanics.Facts;

public class MechanicEntityFactComponentDelegate : EntityFactComponentDelegate<MechanicEntity>
{
	public new MechanicEntityFact Fact => (MechanicEntityFact)base.Fact;
}
