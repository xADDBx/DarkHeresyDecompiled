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
	private SharedStringAsset? Bark;

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
	private SharedStringAsset[] RandomBarks = new SharedStringAsset[0];

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

	public SharedStringAsset? GetBark()
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
			yield return Bark.String;
		}
		SharedStringAsset[] randomBarks = RandomBarks;
		foreach (SharedStringAsset sharedStringAsset in randomBarks)
		{
			yield return sharedStringAsset.String;
		}
	}
}
