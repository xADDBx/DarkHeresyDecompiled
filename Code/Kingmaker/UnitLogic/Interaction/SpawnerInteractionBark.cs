using System;
using System.Collections.Generic;
using System.Linq;
using Code.Editor;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Framework.VO;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.MapObjects;
using UnityEngine;

namespace Kingmaker.UnitLogic.Interaction;

[KnowledgeDatabaseID("50ac711133aea7343ae4091a29784c2d")]
public class SpawnerInteractionBark : SpawnerInteraction, IBarkSource
{
	[SerializeField]
	private bool UseRandomBark;

	[SerializeField]
	[HideIf("UseRandomBark")]
	[StringCreateWindow(StringCreateWindowAttribute.StringType.Bark)]
	private LocalizedString? Bark;

	public bool ForceVoId;

	[ShowIf("ForceVoId")]
	public List<VoIdField> ForcedVoIds = new List<VoIdField>();

	public bool IsSpammable;

	public VoiceOverActAs ActAs;

	[SerializeField]
	[ShowIf("UseRandomBark")]
	private bool DoNotRepeatLastBark;

	[SerializeField]
	[ShowIf("UseRandomBark")]
	[StringCreateWindow(StringCreateWindowAttribute.StringType.Bark)]
	private LocalizedString[] RandomBarks = new LocalizedString[0];

	[NonSerialized]
	private int _lastRandomBarkIdx = -1;

	[Tooltip("Show bark on user. By default bark is shown on target unit.")]
	public bool ShowOnUser;

	public IEnumerable<LocalizedString> Barks => GetAllBarks();

	public bool IsVoIdForced => ForceVoId;

	public List<string> ForcedVoGuids => ForcedVoIds.Select((VoIdField v) => v.Guid).ToList();

	public bool Spammable => IsSpammable;

	public override bool IsDialog => false;

	public IEnumerable<LocalizedString> GetAllBarks()
	{
		if (!UseRandomBark && Bark != null)
		{
			yield return Bark;
			yield break;
		}
		LocalizedString[] randomBarks = RandomBarks;
		for (int i = 0; i < randomBarks.Length; i++)
		{
			yield return randomBarks[i];
		}
	}

	private LocalizedString? GetBark()
	{
		if (UseRandomBark)
		{
			int nextRandomIdx = InteractionHelper.GetNextRandomIdx(RandomBarks.Length, DoNotRepeatLastBark, ref _lastRandomBarkIdx);
			if (nextRandomIdx >= 0)
			{
				return RandomBarks[nextRandomIdx];
			}
			return null;
		}
		return Bark;
	}

	public override AbstractUnitCommand.ResultType Interact(BaseUnitEntity user, AbstractUnitEntity target)
	{
		LocalizedString bark = GetBark();
		if (bark == null)
		{
			return AbstractUnitCommand.ResultType.Success;
		}
		using (ContextData<ActionExecutionContextData>.Request().Setup(ActionExecutionContextData.Type.Interaction))
		{
			AbstractUnitEntity abstractUnitEntity = (ShowOnUser ? user : target);
			string voGuidBySourceAndTarget = VoiceOverController.GetVoGuidBySourceAndTarget(this, abstractUnitEntity);
			BarkPlayer.Bark(abstractUnitEntity, bark, (VoiceOverType)ActAs, voGuidBySourceAndTarget, -1f, user);
		}
		return AbstractUnitCommand.ResultType.Success;
	}
}
