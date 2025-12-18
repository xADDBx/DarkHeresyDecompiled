using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.UnitLogic;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitGroupMemory : IOwlPackable, IOwlPackable<UnitGroupMemory>
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class UnitInfo : IOwlPackable, IOwlPackable<UnitInfo>
	{
		[JsonProperty]
		[OwlPackInclude]
		public TimeSpan LastDetectTime;

		public bool Visible;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "UnitInfo",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("m_UnitRef", typeof(UnitReference)),
				new FieldInfo("LastDetectTime", typeof(TimeSpan))
			}
		};

		[JsonProperty(IsReference = true)]
		[OwlPackInclude]
		private UnitReference m_UnitRef { get; set; }

		public BaseUnitEntity Unit => m_UnitRef.Entity.ToBaseUnitEntity();

		public UnitReference UnitReference => m_UnitRef;

		[JsonConstructor]
		public UnitInfo(UnitReference m_unitRef)
		{
			m_UnitRef = m_unitRef;
		}

		public UnitInfo()
		{
		}

		public override int GetHashCode()
		{
			return m_UnitRef.GetHashCode();
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			UnitInfo source = new UnitInfo();
			result = Unsafe.As<UnitInfo, TPossiblyBase>(ref source);
		}

		public virtual void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
		{
			(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
			var (objectId, _) = orRegister;
			if (orRegister.isRef)
			{
				formatter.ObjectRef(objectId);
				return;
			}
			ushort type = state.TypeLibrary.RegisterType<UnitInfo>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			UnitReference value = m_UnitRef;
			formatter.Field(0, "m_UnitRef", ref value, state);
			formatter.Field(1, "LastDetectTime", ref LastDetectTime, state);
			formatter.EndObject();
		}

		public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitInfo>();
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
					m_UnitRef = formatter.ReadPackable<UnitReference>(state);
					break;
				case 1:
					LastDetectTime = formatter.ReadPackable<TimeSpan>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	private readonly SortedDictionary<string, UnitInfo> m_Units = new SortedDictionary<string, UnitInfo>();

	private readonly List<UnitInfo> m_UnitsList = new List<UnitInfo>();

	private bool m_UnitsListValid;

	private readonly List<UnitInfo> m_EnemiesList = new List<UnitInfo>();

	private bool m_EnemiesListValid;

	private UnitGroup m_Group;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitGroupMemory",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("PersistentUnits", typeof(List<UnitInfo>)),
			new FieldInfo("GroupId", typeof(string))
		}
	};

	[JsonProperty]
	[UsedImplicitly]
	[OwlPackInclude]
	private List<UnitInfo> PersistentUnits
	{
		get
		{
			return m_Units.Select((KeyValuePair<string, UnitInfo> pair) => pair.Value).ToList();
		}
		set
		{
			m_Units.Clear();
			foreach (UnitInfo item in value)
			{
				m_Units.Add(item.UnitReference.Id, item);
			}
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	public string GroupId { get; private set; }

	public UnitGroup Group => m_Group ?? (m_Group = Game.Instance.UnitGroups.GetGroup(GroupId));

	public List<UnitInfo> UnitsList
	{
		get
		{
			if (!m_UnitsListValid)
			{
				m_UnitsList.Clear();
				foreach (KeyValuePair<string, UnitInfo> unit2 in m_Units)
				{
					BaseUnitEntity unit = unit2.Value.Unit;
					if (unit != null && unit.IsInGame)
					{
						m_UnitsList.Add(unit2.Value);
					}
				}
				m_UnitsListValid = true;
			}
			return m_UnitsList;
		}
	}

	public List<UnitInfo> Enemies
	{
		get
		{
			if (!m_EnemiesListValid)
			{
				m_EnemiesList.Clear();
				foreach (UnitInfo value in m_Units.Values)
				{
					BaseUnitEntity unit = value.Unit;
					if (unit != null && unit.IsInGame && Group.IsEnemy(unit))
					{
						m_EnemiesList.Add(value);
					}
				}
				m_EnemiesListValid = true;
			}
			return m_EnemiesList;
		}
	}

	[JsonConstructor]
	public UnitGroupMemory(string groupId)
	{
		GroupId = groupId;
	}

	public UnitGroupMemory()
	{
	}

	public UnitInfo Add(BaseUnitEntity unit)
	{
		m_EnemiesListValid = false;
		m_UnitsListValid = false;
		if (!m_Units.TryGetValue(unit.UniqueId, out var value))
		{
			value = new UnitInfo(unit.FromBaseUnitEntity());
			m_Units.Add(unit.UniqueId, value);
		}
		value.LastDetectTime = Game.Instance.Controllers.TimeController.GameTime;
		return value;
	}

	public void Remove(BaseUnitEntity unit)
	{
		m_EnemiesListValid = false;
		m_UnitsListValid = false;
		m_Units.Remove(unit.UniqueId);
	}

	[CanBeNull]
	public UnitInfo Find(BaseUnitEntity unit)
	{
		m_Units.TryGetValue(unit.UniqueId, out var value);
		return value;
	}

	public void Clear()
	{
		m_UnitsListValid = false;
		ClearEnemies();
		m_UnitsList.Clear();
		m_Units.Clear();
	}

	public void ClearEnemies()
	{
		m_EnemiesListValid = false;
		m_EnemiesList.Clear();
	}

	private bool Contains(MechanicEntity unit, bool visibleOnly)
	{
		try
		{
			UnitInfo unitInfo = m_Units.Get(unit.UniqueId);
			return unitInfo != null && (!visibleOnly || unitInfo.Visible);
		}
		finally
		{
		}
	}

	public bool Contains(MechanicEntity unit)
	{
		return Contains(unit, visibleOnly: false);
	}

	public bool ContainsVisible(MechanicEntity unit)
	{
		return Contains(unit, visibleOnly: true);
	}

	public void Cleanup()
	{
		if (m_Units.Count > 0)
		{
			List<UnitInfo> list = TempList.Get<UnitInfo>();
			foreach (KeyValuePair<string, UnitInfo> unit2 in m_Units)
			{
				UnitInfo value = unit2.Value;
				BaseUnitEntity unit = value.Unit;
				if (unit == null || !unit.IsInGame)
				{
					list.Add(value);
				}
			}
			if (list.Count > 0)
			{
				for (int i = 0; i < list.Count; i++)
				{
					m_Units.Remove(list[i].UnitReference.Id);
				}
				list.Clear();
			}
		}
		m_EnemiesListValid = false;
		m_UnitsListValid = false;
	}

	public bool HasPlayerCharacterInMemory()
	{
		foreach (UnitInfo units in UnitsList)
		{
			if (units.Unit.Faction.IsPlayer)
			{
				return true;
			}
		}
		return false;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitGroupMemory source = new UnitGroupMemory();
		result = Unsafe.As<UnitGroupMemory, TPossiblyBase>(ref source);
	}

	public virtual void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<UnitGroupMemory>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		List<UnitInfo> value = PersistentUnits;
		formatter.Field(0, "PersistentUnits", ref value, state);
		string value2 = GroupId;
		formatter.StringField(1, "GroupId", ref value2, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitGroupMemory>();
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
				PersistentUnits = formatter.ReadPackable<List<UnitInfo>>(state);
				break;
			case 1:
				GroupId = formatter.ReadString(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
