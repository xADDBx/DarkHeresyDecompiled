using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;

namespace Kingmaker.UI.Models.UnitSettings;

[OwlPackable(OwlPackableMode.Generate)]
[HashNoGenerate]
public class PartUnitUISettings : BaseUnitPart, IOwlPackable<PartUnitUISettings>
{
	[JsonProperty]
	[OwlPackInclude]
	private bool m_ShowHelmet = true;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_ShowArmor = true;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_ShowBackpack = true;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_ShowCloak = true;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_ShowGloves = true;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_ShowBoots = true;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_ShowHelmAboveAll;

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
		Fields = new FieldInfo[9]
		{
			new FieldInfo("m_ShowHelmet", typeof(bool)),
			new FieldInfo("m_ShowArmor", typeof(bool)),
			new FieldInfo("m_ShowBackpack", typeof(bool)),
			new FieldInfo("m_ShowCloak", typeof(bool)),
			new FieldInfo("m_ShowGloves", typeof(bool)),
			new FieldInfo("m_ShowBoots", typeof(bool)),
			new FieldInfo("m_ShowHelmAboveAll", typeof(bool)),
			new FieldInfo("m_Portrait", typeof(BlueprintPortrait)),
			new FieldInfo("m_CustomPortrait", typeof(PortraitData))
		}
	};

	public bool ShowHelm
	{
		get
		{
			return m_ShowHelmet;
		}
		set
		{
			if (m_ShowHelmet != value)
			{
				m_ShowHelmet = value;
				this.OnChangedHelmetVisibility?.Invoke();
			}
		}
	}

	public bool ShowArmor
	{
		get
		{
			return m_ShowArmor;
		}
		set
		{
			if (m_ShowArmor != value)
			{
				m_ShowArmor = value;
				this.OnChangedArmorVisibility?.Invoke();
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

	public bool ShowCloak
	{
		get
		{
			return m_ShowCloak;
		}
		set
		{
			if (m_ShowCloak != value)
			{
				m_ShowCloak = value;
				this.OnChangedCloakVisibility?.Invoke();
			}
		}
	}

	public bool ShowGloves
	{
		get
		{
			return m_ShowGloves;
		}
		set
		{
			if (m_ShowGloves != value)
			{
				m_ShowGloves = value;
				this.OnChangedGlovesVisibility?.Invoke();
			}
		}
	}

	public bool ShowBoots
	{
		get
		{
			return m_ShowBoots;
		}
		set
		{
			if (m_ShowBoots != value)
			{
				m_ShowBoots = value;
				this.OnChangedBootsVisibility?.Invoke();
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

	public bool ShowHood
	{
		get
		{
			return false;
		}
		set
		{
			this.OnChangedHoodVisibility?.Invoke();
		}
	}

	public bool ShowBaseHeadwear
	{
		get
		{
			return false;
		}
		set
		{
			this.OnChangedBaseHeadwearVisibility?.Invoke();
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

	private event Action OnChangedHelmetVisibility;

	private event Action OnChangedArmorVisibility;

	private event Action OnChangedBackpackVisibility;

	private event Action OnChangedCloakVisibility;

	private event Action OnChangedGlovesVisibility;

	private event Action OnChangedBootsVisibility;

	private event Action OnChangedHelmetVisibilityAboveAll;

	private event Action OnChangedHoodVisibility;

	private event Action OnChangedBaseHeadwearVisibility;

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
		base.EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUnitPortraitChangedHandler>)delegate(IUnitPortraitChangedHandler h)
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
			base.EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUnitPortraitChangedHandler>)delegate(IUnitPortraitChangedHandler h)
			{
				h.HandlePortraitChanged();
			}, isCheckRuntime: true);
		}
	}

	public void SetPortraitUnsafe([CanBeNull] BlueprintPortrait portrait, [CanBeNull] PortraitData portraitData)
	{
		m_CustomPortrait = portraitData;
		m_Portrait = portrait;
		base.EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUnitPortraitChangedHandler>)delegate(IUnitPortraitChangedHandler h)
		{
			h.HandlePortraitChanged();
		}, isCheckRuntime: true);
	}

	public void SubscribeOnHelmetVisibilityChange(Action subscriber)
	{
		OnChangedHelmetVisibility += subscriber;
	}

	public void UnsubscribeFromHelmetVisibilityChange(Action subscriber)
	{
		OnChangedHelmetVisibility -= subscriber;
	}

	public void SubscribeOnArmorVisibilityChange(Action subscriber)
	{
		OnChangedArmorVisibility += subscriber;
	}

	public void UnsubscribeFromArmorVisibilityChange(Action subscriber)
	{
		OnChangedArmorVisibility -= subscriber;
	}

	public void SubscribeOnBackpackVisibilityChange(Action subscriber)
	{
		OnChangedBackpackVisibility += subscriber;
	}

	public void UnsubscribeFromBackpackVisibilityChange(Action subscriber)
	{
		OnChangedBackpackVisibility -= subscriber;
	}

	public void SubscribeOnCloakVisibilityChange(Action subscriber)
	{
		OnChangedCloakVisibility += subscriber;
	}

	public void UnsubscribeFromCapeVisibilityChange(Action subscriber)
	{
		OnChangedCloakVisibility -= subscriber;
	}

	public void SubscribeOnGlovesVisibilityChange(Action subscriber)
	{
		OnChangedGlovesVisibility += subscriber;
	}

	public void UnsubscribeFromGlovesVisibilityChange(Action subscriber)
	{
		OnChangedGlovesVisibility -= subscriber;
	}

	public void SubscribeOnBootsVisibilityChange(Action subscriber)
	{
		OnChangedBootsVisibility += subscriber;
	}

	public void UnsubscribeFromBootsVisibilityChange(Action subscriber)
	{
		OnChangedBootsVisibility -= subscriber;
	}

	public void SubscribeOnHelmetVisibilityAboveAllChange(Action subscriber)
	{
		OnChangedHelmetVisibilityAboveAll += subscriber;
	}

	public void UnsubscribeFromHelmetVisibilityAboveAllChange(Action subscriber)
	{
		OnChangedHelmetVisibilityAboveAll -= subscriber;
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
		formatter.UnmanagedField(0, "m_ShowHelmet", ref m_ShowHelmet, state);
		formatter.UnmanagedField(1, "m_ShowArmor", ref m_ShowArmor, state);
		formatter.UnmanagedField(2, "m_ShowBackpack", ref m_ShowBackpack, state);
		formatter.UnmanagedField(3, "m_ShowCloak", ref m_ShowCloak, state);
		formatter.UnmanagedField(4, "m_ShowGloves", ref m_ShowGloves, state);
		formatter.UnmanagedField(5, "m_ShowBoots", ref m_ShowBoots, state);
		formatter.UnmanagedField(6, "m_ShowHelmAboveAll", ref m_ShowHelmAboveAll, state);
		formatter.Field(7, "m_Portrait", ref m_Portrait, state);
		formatter.Field(8, "m_CustomPortrait", ref m_CustomPortrait, state);
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
				m_ShowHelmet = formatter.ReadUnmanaged<bool>(state);
				break;
			case 1:
				m_ShowArmor = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				m_ShowBackpack = formatter.ReadUnmanaged<bool>(state);
				break;
			case 3:
				m_ShowCloak = formatter.ReadUnmanaged<bool>(state);
				break;
			case 4:
				m_ShowGloves = formatter.ReadUnmanaged<bool>(state);
				break;
			case 5:
				m_ShowBoots = formatter.ReadUnmanaged<bool>(state);
				break;
			case 6:
				m_ShowHelmAboveAll = formatter.ReadUnmanaged<bool>(state);
				break;
			case 7:
				m_Portrait = formatter.ReadPackable<BlueprintPortrait>(state);
				break;
			case 8:
				m_CustomPortrait = formatter.ReadPackable<PortraitData>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
