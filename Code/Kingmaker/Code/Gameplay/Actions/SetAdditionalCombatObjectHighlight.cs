using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Actions;

[TypeId("eba1646d841e431a8920206f70d314b9")]
public class SetAdditionalCombatObjectHighlight : GameAction
{
	[SerializeReference]
	public MapObjectEvaluator MapObject;

	public AdditionalCombatType CombatType;

	public override string GetCaption()
	{
		return $"Set AdditionalCombat Object highlight State ({CombatType} to {MapObject})";
	}

	protected override void RunAction()
	{
		MapObject.GetValue().GetOptional<PartAdditionalCombatObjectiveMapObject>()?.SetShowType(CombatType);
	}
}
