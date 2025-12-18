using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.View;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Gameplay.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartBuffAnimation : MechanicEntityPart, IHashable, IOwlPackable<PartBuffAnimation>
{
	private ICustomLoopActionTypeProvider m_CustomLoopActionTypeProvider;

	[CanBeNull]
	private UnitAnimationActionHandle m_Handle;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartBuffAnimation",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public void PlayAnimation(ICustomLoopActionTypeProvider customLoopActionTypeProvider)
	{
		if (customLoopActionTypeProvider.Type != null)
		{
			UnitAnimationManager unitAnimationManager = (base.Owner.View as UnitEntityView).Or(null)?.AnimationManager;
			if ((object)unitAnimationManager != null)
			{
				UnitAnimationActionHandle unitAnimationActionHandle = unitAnimationManager.CreateHandle(UnitAnimationType.BuffLoopAction);
				unitAnimationActionHandle.CustomLoopActionType = customLoopActionTypeProvider.Type;
				unitAnimationManager.Execute(unitAnimationActionHandle);
				Set(customLoopActionTypeProvider, unitAnimationActionHandle);
			}
		}
	}

	public void StopAnimation(ICustomLoopActionTypeProvider customLoopActionTypeProvider)
	{
		if (m_CustomLoopActionTypeProvider == customLoopActionTypeProvider)
		{
			WarhammerBuffLoopAction warhammerBuffLoopAction = m_Handle?.Action as WarhammerBuffLoopAction;
			warhammerBuffLoopAction.Or(null)?.SwitchToExit(m_Handle);
			Clear(warhammerBuffLoopAction == null);
		}
	}

	private void Set(ICustomLoopActionTypeProvider source, UnitAnimationActionHandle handle)
	{
		m_Handle?.Release();
		m_CustomLoopActionTypeProvider = source;
		m_Handle = handle;
	}

	private void Clear(bool releaseHandle)
	{
		if (releaseHandle)
		{
			m_Handle?.Release();
		}
		m_CustomLoopActionTypeProvider = null;
		m_Handle = null;
		RemoveSelf();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartBuffAnimation source = new PartBuffAnimation();
		result = Unsafe.As<PartBuffAnimation, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartBuffAnimation>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartBuffAnimation>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			if (mappingForType[fieldID] == byte.MaxValue)
			{
				formatter.SkipField(size);
			}
		}
		formatter.LeaveObject();
	}
}
