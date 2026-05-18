using System;
using System.Collections.Generic;
using System.Linq;
using Code.Editor;
using Kingmaker.Blueprints;
using Kingmaker.Code.Framework.VO;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[Serializable]
public class InteractionBarkSettings : InteractionSettings, IBarkSource
{
	[Header("Bark settings")]
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

	public bool BarkDurationByText = true;

	public bool OverrideBarkDuration;

	[SerializeField]
	[ShowIf("OverrideBarkDuration")]
	public float BarkDuration = 5f;

	[NonSerialized]
	private int _lastRandomBarkIdx = -1;

	[ShowCreator]
	public ActionsReference? BarkActions;

	public bool RunActionsOnce;

	[Tooltip("Show bark on MapObject user. By default bark is shown on MapObject.")]
	public bool ShowOnUser;

	[ShowCreator]
	public ConditionsReference? Condition;

	public bool UseGlobalCooldown;

	public override bool ShouldShowUseAnimationState => false;

	public override bool ShouldShowDialog => false;

	public override bool ShouldShowUnlimitedInteractionsPerRound => false;

	public override bool ShouldShowOverrideActionPointsCost => false;

	public override bool ShouldShowInteractWithMeltaChargeFXData => false;

	public bool ActionsRan { get; set; }

	public IEnumerable<LocalizedString> Barks => GetAllBarks();

	public bool IsVoIdForced => ForceVoId;

	public List<string> ForcedVoGuids => ForcedVoIds.Select((VoIdField v) => v.Guid).ToList();

	public bool Spammable => IsSpammable;

	public LocalizedString? GetBark()
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

	private IEnumerable<LocalizedString> GetAllBarks()
	{
		if (Bark != null)
		{
			yield return Bark;
		}
		LocalizedString[] randomBarks = RandomBarks;
		for (int i = 0; i < randomBarks.Length; i++)
		{
			yield return randomBarks[i];
		}
	}
}
