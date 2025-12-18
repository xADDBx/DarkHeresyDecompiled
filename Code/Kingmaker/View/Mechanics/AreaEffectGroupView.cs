using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.View.MapObjects;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.View.Mechanics;

[KnowledgeDatabaseID("2fe2aff34d6e42d58b7e4374890b22a0")]
public class AreaEffectGroupView : MechanicGroupView<AreaEffectView>
{
	[OwlPackable(OwlPackableMode.Generate)]
	public new class MechanicGroupData : MechanicGroupView<AreaEffectView>.MechanicGroupData, IOwlPackable<MechanicGroupData>
	{
		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "MechanicGroupData",
			OldNames = null,
			Fields = new FieldInfo[10]
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
				new FieldInfo("m_ViewHandlingOnDisposePolicyOverride", typeof(ViewHandlingOnDisposePolicyType?))
			}
		};

		public MechanicGroupData(EntityViewBase view)
			: base(view)
		{
		}

		protected MechanicGroupData(JsonConstructorMark _)
			: base(_)
		{
		}

		protected MechanicGroupData()
		{
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			MechanicGroupData source = new MechanicGroupData();
			result = Unsafe.As<MechanicGroupData, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<MechanicGroupData>(OwlPackTypeInfo);
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
			formatter.EndObject();
		}

		public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<MechanicGroupData>();
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
				}
			}
			formatter.LeaveObject();
		}
	}

	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new MechanicGroupData(this));
	}
}
