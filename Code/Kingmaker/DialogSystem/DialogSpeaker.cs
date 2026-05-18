using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Code.Framework.VO;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Units;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.DialogSystem;

[Serializable]
public class DialogSpeaker
{
	[CanBeNull]
	[SerializeField]
	[FormerlySerializedAs("Blueprint")]
	[HideIf("NoSpeaker")]
	private BlueprintUnitReference m_Blueprint;

	[CanBeNull]
	[SerializeField]
	[FormerlySerializedAs("SpeakerPortrait")]
	[HideIf("NoSpeaker")]
	private BlueprintUnitReference m_SpeakerPortrait;

	[SerializeReference]
	[HideIf("NoSpeaker")]
	public MechanicEntityEvaluator SpeakerEvaluator;

	[ShowIf("m_BlueprintSelected")]
	public VoIdIndex VoIdIndex = new VoIdIndex();

	public bool MoveCamera = true;

	public bool NotRevealInFoW;

	public bool NoSpeaker;

	public bool DoNotReplaceSpeakerWithErrorSpeaker;

	private bool m_BlueprintSelected
	{
		get
		{
			if (!NoSpeaker && (SpeakerPortrait != null || Blueprint != null))
			{
				return SpeakerEvaluator == null;
			}
			return false;
		}
	}

	public BlueprintUnit Blueprint => m_Blueprint?.Get();

	public bool ReplacedSpeakerWithErrorSpeaker { get; set; }

	public BaseUnitEntity ErrorSpeaker => Game.Instance.DefaultUnit;

	public BlueprintUnit SpeakerPortrait => m_SpeakerPortrait?.Get();

	public bool NeedsEntity
	{
		get
		{
			if (!NoSpeaker)
			{
				if (Blueprint == null)
				{
					return SpeakerEvaluator != null;
				}
				return true;
			}
			return false;
		}
	}

	[CanBeNull]
	public BaseUnitEntity GetSpeaker(BlueprintCueBase cue = null)
	{
		if (NoSpeaker)
		{
			return null;
		}
		if (Application.isPlaying && NeedsEntity && TryGetSpeakerEntity(cue, out var speaker))
		{
			return speaker;
		}
		return null;
	}

	[CanBeNull]
	public bool TryGetSpeakerEntity([CanBeNull] BlueprintCueBase cue, out BaseUnitEntity speaker, bool isFromSequence = false)
	{
		speaker = null;
		if (NoSpeaker)
		{
			return false;
		}
		if (SpeakerEvaluator == null && Blueprint == null)
		{
			return false;
		}
		if (SpeakerEvaluator != null)
		{
			try
			{
				speaker = SpeakerEvaluator.GetValue() as BaseUnitEntity;
			}
			catch (Exception)
			{
				DialogDebug.Add(cue, "Failed to evaluate speaker.", Color.red);
			}
		}
		else if (Blueprint != null)
		{
			Vector3 dialogPosition = Game.Instance.Controllers.DialogController.DialogPosition;
			IEnumerable<BaseUnitEntity> second = Game.Instance.Controllers.EntitySpawner.CreationQueue.Select((EntitySpawnController.SpawnEntry ce) => ce.Entity).OfType<BaseUnitEntity>();
			MakeEssentialCharactersConscious();
			speaker = (from u in Game.Instance.EntityPools.AllBaseUnits.Concat(Game.Instance.Player.Party)
				where u.IsInGame && !u.Suppressed
				select u).Concat(second).Select(SelectMatchingUnit).NotNull()
				.Distinct()
				.Nearest(dialogPosition);
		}
		ReplacedSpeakerWithErrorSpeaker = false;
		if (speaker != null)
		{
			return true;
		}
		if (SpeakerPortrait != null || DoNotReplaceSpeakerWithErrorSpeaker || isFromSequence)
		{
			DialogDebug.Add(cue, "Speaker doesnt exist. Skipping Cue");
			return false;
		}
		speaker = ErrorSpeaker;
		ReplacedSpeakerWithErrorSpeaker = true;
		DialogDebug.Add(cue, "Speaker doesnt exist, replaced with defaultUnit", Color.red);
		return true;
	}

	[CanBeNull]
	private BaseUnitEntity SelectMatchingUnit(BaseUnitEntity unit)
	{
		BaseUnitEntity baseUnitEntity = null;
		if (unit.Blueprint == Blueprint)
		{
			baseUnitEntity = unit;
		}
		if (baseUnitEntity != null && !baseUnitEntity.LifeState.IsConscious && !baseUnitEntity.LifeState.IsFinallyDead)
		{
			UnitReturnToConsciousController.MakeUnitConscious(baseUnitEntity);
		}
		if (baseUnitEntity != null && !baseUnitEntity.LifeState.IsDead)
		{
			return baseUnitEntity;
		}
		return null;
	}

	private void MakeEssentialCharactersConscious()
	{
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			if (item.Blueprint == Blueprint && item.IsEssentialForGame && !item.LifeState.IsConscious)
			{
				UnitReturnToConsciousController.MakeUnitConscious(item);
			}
		}
	}

	public string GetVoGuidRuntime()
	{
		if (SpeakerEvaluator != null)
		{
			MechanicEntity mechanicEntity = null;
			try
			{
				mechanicEntity = SpeakerEvaluator.GetValue();
			}
			catch (Exception)
			{
				DialogDebug.Add(null, "Failed to evaluate speaker for VO ID.", Color.red);
				return null;
			}
			if (mechanicEntity.TryGetVoGuid(out var voGuid))
			{
				return voGuid;
			}
		}
		return GetVoGuidFromBlueprints();
	}

	public string GetVoGuidFromBlueprints()
	{
		if (SpeakerPortrait != null)
		{
			return VoIdIndex.GetVoGuid(SpeakerPortrait);
		}
		if (Blueprint != null)
		{
			return VoIdIndex.GetVoGuid(Blueprint);
		}
		return string.Empty;
	}

	public BlueprintUnit GetSpeakerBlueprint()
	{
		if (NoSpeaker)
		{
			return null;
		}
		if (SpeakerPortrait != null)
		{
			return SpeakerPortrait;
		}
		if (Application.isPlaying && SpeakerEvaluator != null)
		{
			try
			{
				if (SpeakerEvaluator.GetValue() is UnitEntity { Blueprint: var blueprint })
				{
					return blueprint;
				}
			}
			catch (Exception)
			{
				DialogDebug.Add(null, "Failed to evaluate speaker for unit blueprint.", Color.red);
				return null;
			}
		}
		return Blueprint;
	}
}
