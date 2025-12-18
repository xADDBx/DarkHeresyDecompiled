using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Visual.Animation;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartVisualChange : AbstractUnitPart, IHashable, IOwlPackable<UnitPartVisualChange>
{
	[JsonProperty]
	[OwlPackInclude]
	private string m_AnimationSetAssetId;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartVisualChange",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_AnimationSetAssetId", typeof(string))
		}
	};

	public void SetAnimationSet(AnimationSetLink animationSetLink)
	{
		m_AnimationSetAssetId = animationSetLink.AssetId;
		TrySetupAnimationSet(animationSetLink);
	}

	protected override void OnViewDidAttach()
	{
		AnimationSetLink animationSetLink = new AnimationSetLink
		{
			AssetId = m_AnimationSetAssetId
		};
		TrySetupAnimationSet(animationSetLink);
	}

	private void TrySetupAnimationSet(AnimationSetLink animationSetLink)
	{
		AnimationSet animationSet = animationSetLink?.Load();
		if ((object)animationSet != null)
		{
			base.Owner.AnimationManager.AnimationSet = animationSet;
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(m_AnimationSetAssetId);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartVisualChange source = new UnitPartVisualChange();
		result = Unsafe.As<UnitPartVisualChange, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartVisualChange>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.StringField(0, "m_AnimationSetAssetId", ref m_AnimationSetAssetId, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartVisualChange>();
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
				m_AnimationSetAssetId = formatter.ReadString(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
