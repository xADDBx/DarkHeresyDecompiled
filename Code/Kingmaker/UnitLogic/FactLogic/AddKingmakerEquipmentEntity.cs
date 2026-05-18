using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.ResourceLinks;
using Kingmaker.Visual.CharacterSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[TypeId("1cc0a2001d424bb8b1e30329f0f8693d")]
public class AddKingmakerEquipmentEntity : UnitFactComponentDelegate
{
	[SerializeField]
	private KingmakerEquipmentEntityReference m_EquipmentEntity;

	public KingmakerEquipmentEntity EquipmentEntity => m_EquipmentEntity;

	protected override void OnActivateOrPostLoad()
	{
		if (TryGetCharacter(out var resultCharacter))
		{
			resultCharacter.AddEquipmentEntities(GetEquipmentLinks());
		}
	}

	protected override void OnDeactivate()
	{
		if (TryGetCharacter(out var resultCharacter))
		{
			resultCharacter.RemoveEquipmentEntities(GetEquipmentLinks());
		}
	}

	private bool TryGetCharacter(out Character resultCharacter)
	{
		resultCharacter = null;
		IUnitEntityView view = base.Owner.View;
		if (view == null)
		{
			return false;
		}
		resultCharacter = view.CharacterAvatar;
		return resultCharacter != null;
	}

	private IEnumerable<EquipmentEntityLink> GetEquipmentLinks()
	{
		Race race = base.Owner.Progression.Race?.RaceId ?? Race.Human;
		return EquipmentEntity.GetLinks(base.Owner.Gender, race);
	}

	protected override void OnViewDidAttach()
	{
		OnActivateOrPostLoad();
	}
}
