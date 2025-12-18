using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using OwlPack.Runtime;

namespace Owlcat.EntityBlackboard;

[OwlPackable(OwlPackableMode.Generate)]
public class RuntimeEntityBlackboard : IRuntimeEntityBlackboard, IOwlPackable, IOwlPackable<RuntimeEntityBlackboard>
{
	[OwlPackInclude]
	private RuntimeVariable[] m_Variables;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "RuntimeEntityBlackboard",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_Variables", typeof(RuntimeVariable[]))
		}
	};

	public IEnumerable<RuntimeVariable> Variables => m_Variables;

	public RuntimeEntityBlackboard()
	{
	}

	public RuntimeEntityBlackboard(EntityBlackboardComponent blackboard)
	{
		int count = blackboard.Variables.Count;
		m_Variables = new RuntimeVariable[count];
		for (int i = 0; i < count; i++)
		{
			VariableElement variableElement = blackboard.Variables[i];
			if (!(variableElement is IntegerVariableElement integerVariableElement))
			{
				if (variableElement is BooleanVariableElement booleanVariableElement)
				{
					m_Variables[i] = new IntegerVariable
					{
						Key = booleanVariableElement.Key,
						Value = (booleanVariableElement.Value ? 1 : 0)
					};
				}
			}
			else
			{
				m_Variables[i] = new IntegerVariable
				{
					Key = integerVariableElement.Key,
					Value = integerVariableElement.Value
				};
			}
		}
	}

	public RuntimeEntityBlackboard(IRuntimeEntityBlackboard runtimeEntityBlackboard)
	{
		m_Variables = runtimeEntityBlackboard.Variables.ToArray();
	}

	public int GetIntValue(string key)
	{
		if (TryGet<IntegerVariable>(key, out var entry))
		{
			return entry.Value;
		}
		return 0;
	}

	public bool TrySetIntValue(string key, int value)
	{
		IntegerVariable entry;
		bool num = TryGet<IntegerVariable>(key, out entry);
		if (num)
		{
			entry.Value = value;
		}
		return num;
	}

	public bool GetBoolValue(string key)
	{
		if (TryGet<IntegerVariable>(key, out var entry))
		{
			return entry.Value != 0;
		}
		return false;
	}

	public bool TrySetBoolValue(string key, bool value)
	{
		IntegerVariable entry;
		bool num = TryGet<IntegerVariable>(key, out entry);
		if (num)
		{
			entry.Value = (value ? 1 : 0);
		}
		return num;
	}

	private bool TryGet<T>(string key, out T entry) where T : RuntimeVariable
	{
		if (m_Variables == null)
		{
			entry = null;
		}
		else
		{
			entry = (T)m_Variables.FirstOrDefault((RuntimeVariable e) => e.Key == key);
		}
		return entry != null;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		RuntimeEntityBlackboard source = new RuntimeEntityBlackboard();
		result = Unsafe.As<RuntimeEntityBlackboard, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<RuntimeEntityBlackboard>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_Variables", ref m_Variables, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<RuntimeEntityBlackboard>();
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
				m_Variables = formatter.ReadPackable<RuntimeVariable[]>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
