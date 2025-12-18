using System;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Buffs.Components;

[Obsolete]
[TypeId("2f27bef52f4606843a1bea657367eda0")]
public class WarhammerRemoveBuffAfterSpaceCombat : UnitBuffComponentDelegate, IAreaHandler, ISubscriber
{
	public void OnAreaBeginUnloading()
	{
		if (Game.Instance.CurrentlyLoadedArea.AreaStatGameMode == GameModeType.SpaceCombat)
		{
			base.Buff.Remove();
		}
	}

	public void OnAreaDidLoad()
	{
	}
}
