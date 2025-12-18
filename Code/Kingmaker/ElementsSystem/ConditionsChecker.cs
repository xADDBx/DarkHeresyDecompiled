using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Code.GameCore.ElementsSystem;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.ElementsSystem.Interfaces;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.ElementsSystem;

[Serializable]
[HashRoot]
[OwlPackable(OwlPackableMode.Generate)]
public class ConditionsChecker : ElementsList, IHashable, IOwlPackable, IOwlPackable<ConditionsChecker>
{
	public Operation Operation;

	[SerializeReference]
	public Condition[] Conditions;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "ConditionsChecker",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public override IEnumerable<Element> Elements => Conditions;

	public bool HasConditions
	{
		get
		{
			Condition[] conditions = Conditions;
			if (conditions != null)
			{
				return conditions.Length > 0;
			}
			return false;
		}
	}

	public bool Check([CanBeNull] IConditionDebugContext debugContext = null, bool @unsafe = false)
	{
		using (ProfileScope.New("ConditionChecker"))
		{
			using ElementsDebugger elementsDebugger = ElementsDebugger.Scope(this);
			if (!HasConditions)
			{
				elementsDebugger?.SetResult(1);
				return true;
			}
			Exception ex = null;
			bool flag = Operation == Operation.And;
			Condition[] conditions = Conditions;
			foreach (Condition condition in conditions)
			{
				if (condition == null)
				{
					continue;
				}
				bool flag2 = false;
				try
				{
					flag2 = condition.Check(this, debugContext);
				}
				catch (Exception ex2)
				{
					if (ex == null)
					{
						ex = ex2;
						elementsDebugger?.SetException(ex);
					}
					if (@unsafe || CutscenePlayerDataScope.Current != null)
					{
						throw;
					}
				}
				if (Operation == Operation.And && !flag2)
				{
					flag = false;
					break;
				}
				if (Operation == Operation.Or && flag2)
				{
					flag = true;
					break;
				}
			}
			if (ex == null)
			{
				elementsDebugger?.SetResult(flag ? 1 : 0);
			}
			return flag;
		}
	}

	public override string ToString()
	{
		if (!HasConditions)
		{
			return string.Empty;
		}
		using PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request();
		StringBuilder builder = pooledStringBuilder.Builder;
		bool flag = true;
		Condition[] conditions = Conditions;
		foreach (Condition condition in conditions)
		{
			if (!flag)
			{
				builder.Append((Operation == Operation.And) ? "AND" : "OR");
			}
			if (flag)
			{
				flag = false;
			}
			builder.Append('(');
			builder.Append(condition.GetCaption(useLineBreaks: false));
			builder.Append(')');
		}
		return builder.ToString();
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
		ConditionsChecker source = new ConditionsChecker();
		result = Unsafe.As<ConditionsChecker, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<ConditionsChecker>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ConditionsChecker>();
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
