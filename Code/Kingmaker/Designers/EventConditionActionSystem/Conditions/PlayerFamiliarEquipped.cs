using System;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Obsolete]
[TypeId("c273aead1882a1649894a3d9b9accfd1")]
public class PlayerFamiliarEquipped : AbstractFamiliarEquipped
{
	[SerializeField]
	private BlueprintUnit.Reference m_Blueprint;

	public new BlueprintUnit Unit => m_Blueprint?.Get();

	protected override BaseUnitEntity Leader => Game.Instance.Player.MainCharacterEntity;

	protected override string GetConditionCaption()
	{
		if (Unit != null)
		{
			return $"Player has equipped {Unit} familiar";
		}
		return "Player has no equipped familiar";
	}
}
