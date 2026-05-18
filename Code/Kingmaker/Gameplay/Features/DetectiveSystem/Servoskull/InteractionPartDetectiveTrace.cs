using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Code.Framework.CutsceneSystem;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Interaction;
using Kingmaker.Interaction;
using Kingmaker.Localization;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Sound.Base;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Kingmaker.Visual.Animation.Kingmaker;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;

[OwlPackable(OwlPackableMode.Generate)]
public class InteractionPartDetectiveTrace : NewInteractionPart<InteractionDetectiveTraceSettings>, IInteractionVariantActor, IInteractionRestriction, IHashable, IOwlPackable<InteractionPartDetectiveTrace>
{
	public sealed class Process : InteractionProcess
	{
		private readonly InteractionDetectiveTraceSettings m_Settings;

		private readonly PartDetectiveServoSkull? m_Servoskull;

		private readonly BaseUnitEntity m_Initiator;

		public Process(BaseUnitEntity initiator, PartDetectiveServoSkull? servoskull, MapObjectEntity target, InteractionDetectiveTraceSettings settings)
			: base(servoskull?.Owner ?? initiator, target)
		{
			m_Settings = settings;
			m_Servoskull = servoskull;
			m_Initiator = initiator;
		}

		protected override Task RunProcess()
		{
			return RunProcessInternal();
		}

		private async Task RunProcessInternal()
		{
			if (m_Servoskull != null)
			{
				await m_Servoskull.Scan(base.Target);
			}
			m_Settings.Actions.Get()?.Run();
			if (!string.IsNullOrEmpty(m_Settings.SoundFX))
			{
				SoundEventsManager.PostEvent(m_Settings.SoundFX, base.Target.View.gameObject);
			}
			BlueprintCutscene maybeBlueprint = m_Settings.Cutscene.MaybeBlueprint;
			if (maybeBlueprint != null)
			{
				CutscenePlayerView.Play(maybeBlueprint);
			}
			BlueprintDialog maybeBlueprint2 = m_Settings.Dialog.MaybeBlueprint;
			if (maybeBlueprint2 != null)
			{
				DialogData data = DialogController.SetupDialogWithMapObject(maybeBlueprint2, base.Target, null, m_Initiator);
				Game.Instance.Controllers.DialogController.StartDialog(data);
			}
			MapObjectEntity target = base.Target;
			if (!(target is DetectiveTraceEntity detectiveTraceEntity))
			{
				if (target is DetectiveTraceRootEntity detectiveTraceRootEntity)
				{
					detectiveTraceRootEntity.FoundRootTraces();
					InteractionPartDetectiveTrace optional = base.Target.GetOptional<InteractionPartDetectiveTrace>();
					if (optional != null)
					{
						optional.Enabled = false;
					}
				}
			}
			else
			{
				detectiveTraceEntity.Followed();
			}
			base.EventBus.RaiseEvent((IMapObjectEntity)base.Target, (Action<IInteractionObjectUIHandler>)delegate(IInteractionObjectUIHandler h)
			{
				h.HandleObjectInteractChanged();
			}, isCheckRuntime: true);
			UnitReference r = Game.Instance.Player.PartyCharacters.Random(PFStatefulRandom.UnitRandom);
			LocalizedString bark = m_Settings.Bark;
			if (bark != null && !bark.Empty)
			{
				string voGuidBySourceAndTarget = VoiceOverController.GetVoGuidBySourceAndTarget(m_Settings, base.Target);
				IBarkHandle handle = BarkPlayer.Bark(base.Target, bark, VoiceOverType.Bark, voGuidBySourceAndTarget, -1f, m_Initiator);
				if (base.Target is DetectiveTraceRootEntity)
				{
					Game.Instance.Controllers.VoiceOverController.ScheduleAskAfterBark(r.ToAbstractUnitEntity(), handle);
				}
			}
			else if (base.Target is DetectiveTraceRootEntity)
			{
				Game.Instance.Controllers.VoiceOverController.ScheduleAskTracesFound(r.ToAbstractUnitEntity());
			}
		}
	}

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "InteractionPartDetectiveTrace",
		OldNames = null,
		Fields = new FieldInfo[6]
		{
			new FieldInfo("SourceType", typeof(string)),
			new FieldInfo("AlreadyUnlocked", typeof(bool)),
			new FieldInfo("AlreadyVisited", typeof(bool)),
			new FieldInfo("m_LastCombatRoundInteractionAttempt", typeof(int)),
			new FieldInfo("m_Enabled", typeof(bool)),
			new FieldInfo("AlreadyUsed", typeof(bool))
		}
	};

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public bool AlreadyUsed { get; private set; }

	public override UIInteractionType UIInteractionType => UIInteractionType.DetectiveTrace;

	public override InteractionType Type
	{
		get
		{
			if (!base.Settings.IsVariative)
			{
				return InteractionType.Approach;
			}
			return InteractionType.Variant;
		}
	}

	public override int ApproachRadius
	{
		get
		{
			if (base.Settings.ProximityRadius <= 0)
			{
				return 2;
			}
			return base.Settings.ProximityRadius;
		}
	}

	public override UnitAnimationInteractionType UseAnimationState => UnitAnimationInteractionType.None;

	public override bool CanBeForceShown => base.Settings.CanBeShownByTab;

	public bool ShowNotFollowedOnMap => base.Settings.ShowNotFollowedOnMap;

	public bool IsVariative => base.Settings.IsVariative;

	public int? InteractionDC => null;

	InteractionActorType IInteractionVariantActor.Type => InteractionActorType.Default;

	public UIInteractionType UIType => UIInteractionType.DetectiveTrace;

	public AbstractInteractionPart InteractionPart => this;

	public BlueprintAdditionalCombatObjective CombatObjective => null;

	public bool ShowInteractFx => false;

	public int? RequiredItemsCount => null;

	public BlueprintItem RequiredItem => null;

	public StatType Skill => StatType.Unknown;

	public bool CheckOnlyOnce => false;

	public bool CanUse => base.Owner.IsInGame;

	bool IInteractionVariantActor.AlreadyUsed => AlreadyUsed;

	public override bool CanInteract()
	{
		if (base.CanInteract())
		{
			return !(PartDetectiveServoSkull.Find()?.IsBusy ?? false);
		}
		return false;
	}

	protected override InteractionProcess OnInteract(BaseUnitEntity user)
	{
		AlreadyUsed = true;
		return new Process(user, PartDetectiveServoSkull.GetFromOwner(user), base.Owner, base.Settings);
	}

	protected override bool CanBeSelected(BaseUnitEntity unit)
	{
		if (base.CanBeSelected(unit))
		{
			PartDetectiveServoSkull fromOwner = PartDetectiveServoSkull.GetFromOwner(unit);
			if (fromOwner != null)
			{
				return !fromOwner.IsBusy;
			}
			return false;
		}
		return false;
	}

	public string? GetInteractionName()
	{
		return base.Settings.DisplayName.Text;
	}

	public bool CheckRestriction(BaseUnitEntity user)
	{
		return CanInteract();
	}

	public void ShowSuccessBark(BaseUnitEntity user)
	{
	}

	public void ShowRestrictionBark(BaseUnitEntity user)
	{
	}

	public void OnDidInteract(BaseUnitEntity user)
	{
	}

	public void OnFailedInteract(BaseUnitEntity user)
	{
	}

	bool IInteractionVariantActor.TryInteract(BaseUnitEntity user)
	{
		return true;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = AlreadyUsed;
		result.Append(ref val2);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		InteractionPartDetectiveTrace source = new InteractionPartDetectiveTrace();
		result = Unsafe.As<InteractionPartDetectiveTrace, TPossiblyBase>(ref source);
	}

	public override void Serialize<TFormatter>(TFormatter formatter, SerializerState state)
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<InteractionPartDetectiveTrace>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.SourceType;
		formatter.StringField(0, "SourceType", ref value, state);
		bool value2 = base.AlreadyUnlocked;
		formatter.UnmanagedField(1, "AlreadyUnlocked", ref value2, state);
		bool value3 = AlreadyVisited;
		formatter.UnmanagedField(2, "AlreadyVisited", ref value3, state);
		formatter.UnmanagedField(3, "m_LastCombatRoundInteractionAttempt", ref m_LastCombatRoundInteractionAttempt, state);
		formatter.UnmanagedField(4, "m_Enabled", ref m_Enabled, state);
		bool value4 = AlreadyUsed;
		formatter.UnmanagedField(5, "AlreadyUsed", ref value4, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<InteractionPartDetectiveTrace>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			switch (mappingForType[fieldID])
			{
			case byte.MaxValue:
				formatter.SkipField(size);
				break;
			case 0:
				base.SourceType = formatter.ReadString(state);
				break;
			case 1:
				base.AlreadyUnlocked = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				AlreadyVisited = formatter.ReadUnmanaged<bool>(state);
				break;
			case 3:
				m_LastCombatRoundInteractionAttempt = formatter.ReadUnmanaged<int>(state);
				break;
			case 4:
				m_Enabled = formatter.ReadUnmanaged<bool>(state);
				break;
			case 5:
				AlreadyUsed = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
