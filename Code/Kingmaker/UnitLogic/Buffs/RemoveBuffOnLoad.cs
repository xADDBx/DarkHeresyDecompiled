using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Buffs;

[AllowedOn(typeof(BlueprintBuff))]
[TypeId("6957e888c702e8041b67aed4c0231fb4")]
public class RemoveBuffOnLoad : UnitBuffComponentDelegate
{
	public bool OnlyFromParty;

	protected override void OnActivateOrPostLoad()
	{
		if (!OnlyFromParty || Game.Instance.Player.Party.Contains(base.Owner))
		{
			base.Buff.MarkExpired();
		}
	}
}
