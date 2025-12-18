using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("f42258a78a4b4c8490804ac5e91d095c")]
public class VeilStressGetter : IntPropertyGetter
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Veil fracture";
	}

	protected override int GetBaseValue()
	{
		return Game.Instance.LoadedArea.Veil.Damage;
	}
}
