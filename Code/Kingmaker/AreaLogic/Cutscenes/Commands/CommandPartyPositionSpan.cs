using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[Obsolete]
[TypeId("d2ebc7b3cf7c1c547a18810e8bba4fbe")]
public class CommandPartyPositionSpan : CommandBase
{
	private class PartyMemberEntry
	{
		public BlueprintUnit Blueprint;

		public Vector3 Position;

		public Vector3[] PetsPositions;

		public float Orientation;

		public float[] PetsOrientations;
	}

	private class Data
	{
		public readonly List<PartyMemberEntry> Entries = new List<PartyMemberEntry>();
	}

	public bool OnlyIfCanMove;

	[InfoBox(Text = "В условии юнит, которого хотим вернуть, доступен через PartyUnit")]
	public ConditionsChecker ReturnCondition;

	public override bool IsContinuous => true;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Entries.Clear();
		foreach (UnitReference partyCharacter in Game.Instance.Player.PartyCharacters)
		{
			if (partyCharacter.Entity != null)
			{
				PartyMemberEntry item = new PartyMemberEntry
				{
					Blueprint = partyCharacter.Entity.ToBaseUnitEntity().Blueprint,
					Position = partyCharacter.Entity.Position,
					Orientation = partyCharacter.Entity.ToBaseUnitEntity().Orientation
				};
				commandData.Entries.Add(item);
			}
		}
	}

	protected override void OnSkip(CutscenePlayerData player)
	{
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return false;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		base.OnStop(player);
		foreach (PartyMemberEntry entry in player.GetCommandData<Data>(this).Entries)
		{
			BaseUnitEntity baseUnitEntity = Game.Instance.Player.PartyCharacters.FirstOrDefault((UnitReference c) => c.Entity?.ToBaseUnitEntity().Blueprint == entry.Blueprint).Entity.ToBaseUnitEntity();
			if (baseUnitEntity != null && ShouldReturn(baseUnitEntity))
			{
				baseUnitEntity.Position = entry.Position;
				baseUnitEntity.SetOrientation(entry.Orientation);
			}
		}
	}

	private bool ShouldReturn(BaseUnitEntity character)
	{
		if (OnlyIfCanMove && !character.CanMove)
		{
			return false;
		}
		if (ReturnCondition.HasConditions)
		{
			using (ContextData<PartyUnitData>.Request().Setup(character))
			{
				if (!ReturnCondition.Check())
				{
					return false;
				}
			}
		}
		return true;
	}

	public override string GetCaption()
	{
		return "<b>Store</b> party positions";
	}
}
