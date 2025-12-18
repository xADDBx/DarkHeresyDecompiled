using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Core.Cheats;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.Settings;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.StatefulRandom;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Tutorial;

[OwlPackable(OwlPackableMode.Generate)]
public class TutorialSystem : Entity, IHashable, IOwlPackable<TutorialSystem>
{
	public const string ID = "tutorial-system-id";

	public new static readonly EntityRef<TutorialSystem> Ref = new EntityRef<TutorialSystem>("tutorial-system-id");

	[JsonProperty]
	[OwlPackInclude]
	private TimeSpan m_CooldownBeforeTime;

	[JsonProperty]
	[OwlPackInclude]
	private int m_ShowIndex;

	[CanBeNull]
	private TutorialData m_ShowData;

	[JsonProperty]
	[OwlPackInclude]
	private List<TutorialTag> m_BannedTags = new List<TutorialTag>();

	[CanBeNull]
	private TutorialData m_CandidateForShow;

	private float? m_Countdown;

	private readonly HashSet<BlueprintTutorial> m_TriedToTriggerThisFrame = new HashSet<BlueprintTutorial>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "TutorialSystem",
		OldNames = null,
		Fields = new FieldInfo[13]
		{
			new FieldInfo("UniqueId", typeof(string)),
			new FieldInfo("m_IsInGame", typeof(bool)),
			new FieldInfo("m_Position", typeof(Vector3)),
			new FieldInfo("m_Orientation", typeof(float)),
			new FieldInfo("m_InitialPosition", typeof(Vector3?)),
			new FieldInfo("m_InitialOrientation", typeof(float?)),
			new FieldInfo("Facts", typeof(EntityFactsManager)),
			new FieldInfo("Parts", typeof(EntityPartsManager)),
			new FieldInfo("m_IsRevealed", typeof(bool)),
			new FieldInfo("m_ViewHandlingOnDisposePolicyOverride", typeof(ViewHandlingOnDisposePolicyType?)),
			new FieldInfo("m_CooldownBeforeTime", typeof(TimeSpan)),
			new FieldInfo("m_ShowIndex", typeof(int)),
			new FieldInfo("m_BannedTags", typeof(List<TutorialTag>))
		}
	};

	public bool HasCooldown => m_CooldownBeforeTime > Game.Instance.Controllers.TimeController.RealTime;

	public int LastCooldownTutorialPriority { get; private set; }

	public bool HasCandidateForShow => m_CandidateForShow != null;

	public bool HasShownData => m_ShowData != null;

	public TutorialData ShowingData
	{
		get
		{
			return m_ShowData;
		}
		set
		{
			m_ShowData = value;
		}
	}

	public Tutorial Ensure(BlueprintTutorial blueprint)
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			return Facts.Get<Tutorial>(blueprint) ?? Facts.Add(new Tutorial(blueprint));
		}
	}

	public bool IsTagBanned(TutorialTag tag)
	{
		return !SettingsRoot.Game.Tutorial.ShouldShowTag(tag);
	}

	protected TutorialSystem(OwlPackConstructorParameter _)
		: base(_)
	{
	}

	public TutorialSystem()
		: base("tutorial-system-id", isInGame: true)
	{
	}

	protected override IEntityViewBase CreateViewForData()
	{
		return null;
	}

	public void Ban(BlueprintTutorial blueprintTutorial)
	{
		Tutorial tutorial = Ensure(blueprintTutorial);
		tutorial.Banned = true;
		tutorial.UpdateIsEnabled();
	}

	public void BanTag(TutorialTag tag)
	{
		if (!IsTagBanned(tag))
		{
			m_BannedTags.Add(tag);
			SettingsRoot.Game.Tutorial.SetValueAndConfirmForTag(tag, value: false);
			SettingsController.Instance.SaveAll();
			UpdateEnabledTutorials();
		}
	}

	public void UpdateEnabledTutorials()
	{
		foreach (Tutorial item in Facts.GetAll<Tutorial>())
		{
			item.UpdateIsEnabled();
		}
	}

	public bool OnTryToTrigger(Tutorial tutorial, TutorialContext context)
	{
		if (!tutorial.IsEnabled)
		{
			if (tutorial.IsLimitReached)
			{
				EventBus.RaiseEvent(delegate(ITutorialTriggerFailedHandler h)
				{
					h.HandleLimitReached(tutorial, context);
				});
			}
			if (tutorial.Owner.IsTagBanned(tutorial.Blueprint.Tag))
			{
				EventBus.RaiseEvent(delegate(ITutorialTriggerFailedHandler h)
				{
					h.HandleTagBanned(tutorial, context);
				});
			}
			return false;
		}
		if (!m_TriedToTriggerThisFrame.Contains(tutorial.Blueprint))
		{
			tutorial.TriggeredTimes++;
		}
		m_TriedToTriggerThisFrame.Add(tutorial.Blueprint);
		if (!tutorial.Blueprint.IgnoreCooldown && HasCooldown)
		{
			if (LastCooldownTutorialPriority > tutorial.Blueprint.Priority)
			{
				EventBus.RaiseEvent(delegate(ITutorialTriggerFailedHandler h)
				{
					h.HandleHigherPriorityCooldown(tutorial, context);
				});
			}
			else
			{
				EventBus.RaiseEvent(delegate(ITutorialTriggerFailedHandler h)
				{
					h.HandleLowerOrEqualPriorityCooldown(tutorial, context);
				});
			}
			return false;
		}
		if (tutorial.Blueprint.Frequency > 1 && tutorial.LastShowIndex > 0 && Math.Abs(m_ShowIndex - tutorial.LastShowIndex) < tutorial.Blueprint.Frequency)
		{
			EventBus.RaiseEvent(delegate(ITutorialTriggerFailedHandler h)
			{
				h.HandleFrequencyReached(tutorial, context);
			});
			return false;
		}
		if (m_CandidateForShow != null && m_CandidateForShow.Blueprint.Priority >= tutorial.Blueprint.Priority)
		{
			EventBus.RaiseEvent(delegate(ITutorialTriggerFailedHandler h)
			{
				h.HandleLowerOrEqualPriorityCooldown(tutorial, context);
			});
			return false;
		}
		if (m_CandidateForShow?.Trigger != null)
		{
			return false;
		}
		return true;
	}

	public void Trigger(BlueprintTutorial tutorial, [CanBeNull] TutorialTrigger trigger)
	{
		TutorialContext current = ContextData<TutorialContext>.Current;
		if (current == null)
		{
			PFLog.Default.ErrorWithReport("BlueprintTutorial.Trigger: context is missing");
			return;
		}
		bool flag = false;
		if (trigger != null)
		{
			foreach (TutorialSolver component in tutorial.GetComponents<TutorialSolver>())
			{
				try
				{
					flag = component.Solve(current);
				}
				catch (Exception exception)
				{
					PFLog.Default.ExceptionWithReport(exception, null);
				}
				if (flag)
				{
					break;
				}
			}
		}
		if (trigger != null && trigger.RevealTargetUnitInfo && current.RevealUnitInfo.FromBaseUnitEntity() == null)
		{
			current.RevealUnitInfo = current.TargetUnit;
		}
		TutorialData tutorialData = new TutorialData(tutorial, trigger, current.RevealUnitInfo, flag)
		{
			SolutionItem = current.SolutionItem,
			SolutionAbility = current.SolutionAbility,
			SolutionUnit = (current.SolutionUnit ?? (current.SolutionAbility?.Caster as BaseUnitEntity) ?? (current.SolutionItem?.Owner as BaseUnitEntity)),
			SourceUnit = current.SourceUnit
		};
		tutorialData.AddPage(tutorial);
		foreach (ITutorialPage component2 in tutorial.GetComponents<ITutorialPage>())
		{
			tutorialData.AddPage(component2);
		}
		m_CandidateForShow = tutorialData;
		m_Countdown = ((trigger != null && !tutorial.IgnoreCooldown) ? new float?(ConfigRoot.Instance.SystemMechanics.TutorialDelaySeconds) : null);
		if (trigger != null && LoadingProcess.Instance.IsLoadingInProcess)
		{
			m_Countdown = m_Countdown.GetValueOrDefault() + ConfigRoot.Instance.SystemMechanics.TutorialDelaySecondsAfterLoading;
		}
	}

	public void Tick()
	{
		m_TriedToTriggerThisFrame.Clear();
		if (m_CandidateForShow == null)
		{
			return;
		}
		if (m_Countdown.HasValue)
		{
			m_Countdown -= Game.Instance.Controllers.TimeController.DeltaTime;
			if (m_Countdown > 0f)
			{
				return;
			}
		}
		if (CutsceneLock.Active)
		{
			return;
		}
		if (m_CandidateForShow.Blueprint.SetCooldown)
		{
			m_CooldownBeforeTime = Game.Instance.Controllers.TimeController.RealTime + ConfigRoot.Instance.SystemMechanics.TutorialCooldownSeconds.Seconds();
			LastCooldownTutorialPriority = m_CandidateForShow.Blueprint.Priority;
		}
		if (m_CandidateForShow.RevealUnitInfo.Entity.FromBaseUnitEntity() != null)
		{
			Game.Instance.Player.InspectUnitsManager.ForceRevealUnitInfo(m_CandidateForShow.RevealUnitInfo.Entity);
		}
		try
		{
			Show(m_CandidateForShow);
		}
		finally
		{
			m_CandidateForShow = null;
			m_Countdown = null;
		}
	}

	private void Show(TutorialData data)
	{
		EventBus.RaiseEvent(delegate(INewTutorialUIHandler h)
		{
			h.ShowTutorial(data);
		});
		Tutorial tutorial;
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			tutorial = Ensure(data.Blueprint);
		}
		tutorial.ShowedTimes++;
		tutorial.LastShowIndex = ++m_ShowIndex;
		tutorial.UpdateIsEnabled();
	}

	[Cheat(Name = "tutorial_unban")]
	public static void UnBanAll()
	{
		SettingsRoot.Game.Tutorial.SetValueAndConfirmForAll(value: true);
		SettingsController.Instance.SaveAll();
		foreach (Tutorial item in Game.Instance.TutorialSystem.Facts.GetAll<Tutorial>())
		{
			item.Banned = false;
		}
		Game.Instance.TutorialSystem.UpdateEnabledTutorials();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_CooldownBeforeTime);
		result.Append(ref m_ShowIndex);
		List<TutorialTag> bannedTags = m_BannedTags;
		if (bannedTags != null)
		{
			for (int i = 0; i < bannedTags.Count; i++)
			{
				TutorialTag obj = bannedTags[i];
				Hash128 val2 = UnmanagedHasher<TutorialTag>.GetHash128(ref obj);
				result.Append(ref val2);
			}
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		TutorialSystem source = new TutorialSystem(default(OwlPackConstructorParameter));
		result = Unsafe.As<TutorialSystem, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<TutorialSystem>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.UniqueId;
		formatter.StringField(0, "UniqueId", ref value, state);
		formatter.UnmanagedField(1, "m_IsInGame", ref m_IsInGame, state);
		formatter.Field(2, "m_Position", ref m_Position, state);
		formatter.UnmanagedField(3, "m_Orientation", ref m_Orientation, state);
		formatter.NullableField(4, "m_InitialPosition", ref m_InitialPosition, state);
		formatter.UnmanagedNullableField(5, "m_InitialOrientation", ref m_InitialOrientation, state);
		formatter.Field(6, "Facts", ref Facts, state);
		formatter.Field(7, "Parts", ref Parts, state);
		formatter.UnmanagedField(8, "m_IsRevealed", ref m_IsRevealed, state);
		formatter.EnumNullableField(9, "m_ViewHandlingOnDisposePolicyOverride", ref m_ViewHandlingOnDisposePolicyOverride, state);
		formatter.Field(10, "m_CooldownBeforeTime", ref m_CooldownBeforeTime, state);
		formatter.UnmanagedField(11, "m_ShowIndex", ref m_ShowIndex, state);
		formatter.Field(12, "m_BannedTags", ref m_BannedTags, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<TutorialSystem>();
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
				base.UniqueId = formatter.ReadString(state);
				break;
			case 1:
				m_IsInGame = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				m_Position = formatter.ReadPackable<Vector3>(state);
				break;
			case 3:
				m_Orientation = formatter.ReadUnmanaged<float>(state);
				break;
			case 4:
				m_InitialPosition = formatter.ReadNullablePackable<Vector3>(state);
				break;
			case 5:
				m_InitialOrientation = formatter.ReadNullableUnmanaged<float>(state);
				break;
			case 6:
				Facts = formatter.ReadPackable<EntityFactsManager>(state);
				break;
			case 7:
				Parts = formatter.ReadPackable<EntityPartsManager>(state);
				break;
			case 8:
				m_IsRevealed = formatter.ReadUnmanaged<bool>(state);
				break;
			case 9:
				m_ViewHandlingOnDisposePolicyOverride = formatter.ReadNullableEnum<ViewHandlingOnDisposePolicyType>(state);
				break;
			case 10:
				m_CooldownBeforeTime = formatter.ReadPackable<TimeSpan>(state);
				break;
			case 11:
				m_ShowIndex = formatter.ReadUnmanaged<int>(state);
				break;
			case 12:
				m_BannedTags = formatter.ReadPackable<List<TutorialTag>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
