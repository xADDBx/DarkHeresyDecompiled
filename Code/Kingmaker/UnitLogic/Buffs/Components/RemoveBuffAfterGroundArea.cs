using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Buffs.Components;

[TypeId("b9357f92dbb44df4882680382c77fb71")]
public class RemoveBuffAfterGroundArea : UnitBuffComponentDelegate, IAreaHandler, ISubscriber
{
	public List<BlueprintAreaReference> AreaExceptions = new List<BlueprintAreaReference>();

	public void OnAreaBeginUnloading()
	{
		if (Game.Instance.CurrentlyLoadedArea.AreaStatGameMode == GameModeType.Default && !AreaExceptions.Contains(Game.Instance.CurrentlyLoadedArea.ToReference<BlueprintAreaReference>()))
		{
			base.Buff.Remove();
		}
	}

	public void OnAreaDidLoad()
	{
	}
}
