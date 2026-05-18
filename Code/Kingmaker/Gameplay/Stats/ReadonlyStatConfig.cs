using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.Gameplay.Stats;

public static class ReadonlyStatConfig
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	private static void RegisterAll()
	{
		StatTypeMetadata.RegisterReadonly(StatType.CurrentHitPoints, (MechanicEntity entity) => entity.GetRequired<PartHealth>().HitPointsLeft);
		StatTypeMetadata.RegisterReadonly(StatType.CurrentArmorDurability, (MechanicEntity entity) => entity.GetRequired<PartArmor>().DurabilityLeft);
		StatTypeMetadata.ValidateRegistrations();
	}
}
