using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Code.Framework.VO;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Dialog;
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
			if (!NoSpeaker)
			{
				if (SpeakerPortrait == null)
				{
					return Blueprint != null;
				}
				return true;
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
	public BaseUnitEntity GetSpeaker(BlueprintCueBase cue)
	{
		if (NoSpeaker)
		{
			return null;
		}
		if (NeedsEntity && TryGetSpeakerEntity(cue, out var speaker))
		{
			return speaker;
		}
		return null;
	}

	[CanBeNull]
	public bool TryGetSpeakerEntity([CanBeNull] BlueprintCueBase cue, out BaseUnitEntity speaker)
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
			speaker = SpeakerEvaluator.GetValue() as BaseUnitEntity;
		}
		else if (Blueprint != null)
		{
			Vector3 dialogPosition = Game.Instance.Controllers.DialogController.DialogPosition;
			IEnumerable<BaseUnitEntity> second = Game.Instance.Controllers.EntitySpawner.CreationQueue.Select((EntitySpawnController.SpawnEntry ce) => ce.Entity).OfType<BaseUnitEntity>();
			MakeEssentialCharactersConscious();
			ReplacedSpeakerWithErrorSpeaker = false;
			speaker = (from u in Game.Instance.EntityPools.AllBaseUnits.Concat(Game.Instance.Player.Party)
				where u.IsInGame && !u.Suppressed
				select u).Concat(second).Select(SelectMatchingUnit).NotNull()
				.Distinct()
				.Nearest(dialogPosition);
		}
		if (speaker != null)
		{
			return true;
		}
		string message = "speaker[" + Blueprint.name + "] doesnt exist. Skipping Cue";
		if (SpeakerPortrait != null || Blueprint.IsCompanion || DoNotReplaceSpeakerWithErrorSpeaker)
		{
			DialogDebug.Add(cue, message);
			return false;
		}
		speaker = ErrorSpeaker;
		ReplacedSpeakerWithErrorSpeaker = true;
		message = "speaker[" + Blueprint.name + "] doesnt exist, replaced with defaultUnit";
		DialogDebug.Add(cue, message, Color.red);
		return false;
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
		if (SpeakerEvaluator != null && SpeakerEvaluator.GetValue().TryGetVoGuid(out var voGuid))
		{
			return voGuid;
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
		if (Application.isPlaying && SpeakerEvaluator?.GetValue() is UnitEntity unitEntity)
		{
			return unitEntity.Blueprint;
		}
		if (Blueprint != null)
		{
			return Blueprint;
		}
		return null;
	}
}
