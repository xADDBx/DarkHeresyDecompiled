using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.ResourceLinks;
using Kingmaker.Visual.CharacterSystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[Serializable]
[ComponentName("Command/UnclotheRT")]
[TypeId("e123a430872f4f13b53b94326e2725ba")]
public class UnclotheRT : CommandBase
{
	public bool ClotheON;

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		BaseUnitEntity playerCharacter = GameHelper.GetPlayerCharacter();
		if (playerCharacter?.View == null)
		{
			return CommandResult.Fail("Unit not found");
		}
		if (playerCharacter.View.CharacterAvatar == null)
		{
			return CommandResult.Fail("Unit avatar not found");
		}
		if (!ClotheON)
		{
			playerCharacter.View.CharacterAvatar.SavedBeforeCutsceneEquipment.Clear();
			playerCharacter.View.CharacterAvatar.SavedBeforeCutsceneRampIndices.Clear();
			foreach (EquipmentEntity equipmentEntity2 in playerCharacter.View.CharacterAvatar.EquipmentEntities)
			{
				playerCharacter.View.CharacterAvatar.SavedBeforeCutsceneEquipment.Add(equipmentEntity2);
			}
			foreach (Character.SelectedRampIndices rampIndex in playerCharacter.View.CharacterAvatar.RampIndices)
			{
				playerCharacter.View.CharacterAvatar.SavedBeforeCutsceneRampIndices.Add(rampIndex);
			}
			List<EquipmentEntityLink> dontUnequip = ConfigRoot.Instance.CharGenRoot.GetDollConfig(playerCharacter.Gender).DontUnequip;
			playerCharacter.View.CharacterAvatar.RemoveAllEquipmentEntities();
			IEnumerable<EquipmentEntity> source = dontUnequip.Select((EquipmentEntityLink x) => x.Load());
			foreach (EquipmentEntity equipmentEntity in playerCharacter.View.CharacterAvatar.SavedBeforeCutsceneEquipment)
			{
				if (source.Contains(equipmentEntity))
				{
					playerCharacter.View.CharacterAvatar.AddEquipmentEntity(equipmentEntity);
				}
				Character.SelectedRampIndices selectedRampIndices = playerCharacter.View.CharacterAvatar.SavedBeforeCutsceneRampIndices.FirstOrDefault((Character.SelectedRampIndices x) => x.EquipmentEntity == equipmentEntity);
				if (selectedRampIndices != null)
				{
					playerCharacter.View.CharacterAvatar.SetRampIndices(equipmentEntity, selectedRampIndices.PrimaryIndex, selectedRampIndices.SecondaryIndex);
				}
			}
			EquipmentEntityLink equipmentEntityLink = ConfigRoot.Instance.CharGenRoot.GetDollConfig(playerCharacter.Gender).Clothes[0];
			if (equipmentEntityLink != null)
			{
				playerCharacter.View.CharacterAvatar.AddEquipmentEntity(equipmentEntityLink);
			}
			playerCharacter.View.HandsEquipment.UpdateVisibility(isVisible: false);
		}
		else
		{
			playerCharacter.View.CharacterAvatar.RestoreEquipment();
			playerCharacter.View.HandsEquipment.UpdateVisibility(isVisible: true);
		}
		return CommandResult.Success;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return true;
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		BaseUnitEntity playerCharacter = GameHelper.GetPlayerCharacter();
		playerCharacter.View.CharacterAvatar.RestoreEquipment();
		playerCharacter.View.HandsEquipment.UpdateVisibility(isVisible: true);
		return CommandResult.Success;
	}
}
