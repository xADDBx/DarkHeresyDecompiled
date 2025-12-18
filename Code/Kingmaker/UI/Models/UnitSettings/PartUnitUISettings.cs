using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UI.Models.UnitSettings;

[OwlPackable(OwlPackableMode.Generate)]
public class PartUnitUISettings : BaseUnitPart, IHashable, IOwlPackable<PartUnitUISettings>
{
	[JsonProperty]
	[OwlPackInclude]
	private bool m_ShowHelm = true;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_ShowHelmAboveAll;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_ShowBackpack = true;

	[JsonProperty]
	[CanBeNull]
	[OwlPackInclude]
	private BlueprintPortrait m_Portrait;

	[JsonProperty]
	[CanBeNull]
	[OwlPackInclude]
	private PortraitData m_CustomPortrait;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartUnitUISettings",
		OldNames = null,
		Fields = new FieldInfo[5]
		{
			new FieldInfo("m_ShowHelm", typeof(bool)),
			new FieldInfo("m_ShowHelmAboveAll", typeof(bool)),
			new FieldInfo("m_ShowBackpack", typeof(bool)),
			new FieldInfo("m_Portrait", typeof(BlueprintPortrait)),
			new FieldInfo("m_CustomPortrait", typeof(PortraitData))
		}
	};

	public bool ShowHelm
	{
		get
		{
			return m_ShowHelm;
		}
		set
		{
			if (m_ShowHelm != value)
			{
				m_ShowHelm = value;
				this.OnChangedHelmetVisibility?.Invoke();
			}
		}
	}

	public bool ShowHelmAboveAll
	{
		get
		{
			return m_ShowHelmAboveAll;
		}
		set
		{
			if (m_ShowHelmAboveAll != value)
			{
				m_ShowHelmAboveAll = value;
				this.OnChangedHelmetVisibilityAboveAll?.Invoke();
			}
		}
	}

	public bool ShowBackpack
	{
		get
		{
			return m_ShowBackpack;
		}
		set
		{
			if (m_ShowBackpack != value)
			{
				m_ShowBackpack = value;
				this.OnChangedBackpackVisibility?.Invoke();
			}
		}
	}

	[CanBeNull]
	public BlueprintPortrait PortraitBlueprint
	{
		get
		{
			if (m_CustomPortrait == null)
			{
				if (!m_Portrait)
				{
					return base.Owner.Blueprint.PortraitSafe;
				}
				return m_Portrait;
			}
			return null;
		}
	}

	[CanBeNull]
	public PortraitData CustomPortraitRaw => m_CustomPortrait;

	[CanBeNull]
	public BlueprintPortrait PortraitBlueprintRaw => m_Portrait;

	public PortraitData Portrait
	{
		get
		{
			if (m_CustomPortrait != null)
			{
				return m_CustomPortrait;
			}
			BlueprintPortrait p = m_Portrait ?? base.Owner.Blueprint.PortraitSafe;
			if (!(Game.Instance.Player.MainCharacter == base.Owner) && Game.Instance.Player.AllCharacters.HasItem((BaseUnitEntity u) => u.UISettings.m_Portrait == p) && p.BackupPortrait != null)
			{
				return p.BackupPortrait.Data;
			}
			return p.Data;
		}
	}

	private event Action OnChangedBackpackVisibility;

	private event Action OnChangedHelmetVisibility;

	private event Action OnChangedHelmetVisibilityAboveAll;

	public void SetPortrait(BlueprintPortrait portrait)
	{
		if (portrait == ConfigRoot.Instance.CharGenRoot.CustomPortrait || portrait.Data.IsCustom)
		{
			m_CustomPortrait = portrait.Data;
			m_Portrait = null;
		}
		else
		{
			m_Portrait = portrait;
			m_CustomPortrait = null;
		}
		EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUnitPortraitChangedHandler>)delegate(IUnitPortraitChangedHandler h)
		{
			h.HandlePortraitChanged();
		}, isCheckRuntime: true);
	}

	public void SetPortrait(PortraitData portraitData, bool raiseEvent = true)
	{
		m_CustomPortrait = portraitData;
		m_Portrait = null;
		if (raiseEvent)
		{
			EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUnitPortraitChangedHandler>)delegate(IUnitPortraitChangedHandler h)
			{
				h.HandlePortraitChanged();
			}, isCheckRuntime: true);
		}
	}

	public void SetPortraitUnsafe([CanBeNull] BlueprintPortrait portrait, [CanBeNull] PortraitData portraitData)
	{
		m_CustomPortrait = portraitData;
		m_Portrait = portrait;
		EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUnitPortraitChangedHandler>)delegate(IUnitPortraitChangedHandler h)
		{
			h.HandlePortraitChanged();
		}, isCheckRuntime: true);
	}

	public void SubscribeOnBackpackVisibilityChange(Action subscriber)
	{
		OnChangedBackpackVisibility += subscriber;
	}

	public void SubscribeOnHelmetVisibilityChange(Action subscriber)
	{
		OnChangedHelmetVisibility += subscriber;
	}

	public void SubscribeOnHelmetVisibilityAboveAllChange(Action subscriber)
	{
		OnChangedHelmetVisibilityAboveAll += subscriber;
	}

	public void UnsubscribeFromBackpackVisibilityChange(Action subscriber)
	{
		OnChangedBackpackVisibility -= subscriber;
	}

	public void UnsubscribeFromHelmetVisibilityChange(Action subscriber)
	{
		OnChangedHelmetVisibility -= subscriber;
	}

	public void UnsubscribeFromHelmetVisibilityAboveAllChange(Action subscriber)
	{
		OnChangedHelmetVisibilityAboveAll -= subscriber;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_ShowHelm);
		result.Append(ref m_ShowHelmAboveAll);
		result.Append(ref m_ShowBackpack);
		Hash128 val2 = SimpleBlueprintHasher.GetHash128(m_Portrait);
		result.Append(ref val2);
		Hash128 val3 = ClassHasher<PortraitData>.GetHash128(m_CustomPortrait);
		result.Append(ref val3);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartUnitUISettings source = new PartUnitUISettings();
		result = Unsafe.As<PartUnitUISettings, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartUnitUISettings>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.UnmanagedField(0, "m_ShowHelm", ref m_ShowHelm, state);
		formatter.UnmanagedField(1, "m_ShowHelmAboveAll", ref m_ShowHelmAboveAll, state);
		formatter.UnmanagedField(2, "m_ShowBackpack", ref m_ShowBackpack, state);
		formatter.Field(3, "m_Portrait", ref m_Portrait, state);
		formatter.Field(4, "m_CustomPortrait", ref m_CustomPortrait, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartUnitUISettings>();
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
				m_ShowHelm = formatter.ReadUnmanaged<bool>(state);
				break;
			case 1:
				m_ShowHelmAboveAll = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				m_ShowBackpack = formatter.ReadUnmanaged<bool>(state);
				break;
			case 3:
				m_Portrait = formatter.ReadPackable<BlueprintPortrait>(state);
				break;
			case 4:
				m_CustomPortrait = formatter.ReadPackable<PortraitData>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
