using System.Linq;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Parts;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("e3ebc45afa2e07746a0de30f4b75ac84")]
public class TutorialTriggerAbilityModifiersInterface : TutorialTrigger, ICharInfoAbilitiesOpenHandler, ISubscriber
{
	private bool m_IsTriggered;

	public void HandleCharInfoAbilitiesOpen(BaseUnitEntity unit)
	{
		if (m_IsTriggered || unit == null)
		{
			return;
		}
		PartAbilityModifiers optional = unit.GetOptional<PartAbilityModifiers>();
		if (optional != null && HasUninsertedModifier(optional))
		{
			TryToTrigger(null, delegate(TutorialContext ctx)
			{
				ctx.SourceUnit = unit;
			});
			m_IsTriggered = true;
		}
	}

	private static bool HasUninsertedModifier(PartAbilityModifiers partModifiers)
	{
		foreach (BlueprintAbilityModifier modifier in partModifiers.KnownModifiers)
		{
			if (partModifiers.AddedModifiers.All((PartAbilityModifiers.AddedEntry a) => a.Modifier != modifier))
			{
				return true;
			}
		}
		return false;
	}
}
