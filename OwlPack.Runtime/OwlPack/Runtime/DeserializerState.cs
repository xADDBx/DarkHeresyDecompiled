using System.Collections.Generic;

namespace OwlPack.Runtime;

public class DeserializerState
{
	public readonly byte Version;

	public TypeLibrary TypeLibrary;

	public ReferenceResolver References = new ReferenceResolver();

	private Dictionary<TypeInfo, List<byte>> m_Mappings = new Dictionary<TypeInfo, List<byte>>();

	public List<byte> GetMappingForType(TypeInfo currentType, TypeInfo serializedType)
	{
		if (m_Mappings.TryGetValue(serializedType, out var value))
		{
			return value;
		}
		FieldInfo[] fields = serializedType.Fields;
		value = new List<byte>((fields != null) ? fields.Length : 0);
		ushort num = 0;
		while (true)
		{
			ushort num2 = num;
			FieldInfo[] fields2 = serializedType.Fields;
			if (num2 >= ((fields2 != null) ? fields2.Length : 0))
			{
				break;
			}
			string name = serializedType.Fields[num].Name;
			bool flag = false;
			for (byte b = 0; b < currentType.Fields.Length; b++)
			{
				FieldInfo fieldInfo = currentType.Fields[b];
				bool flag2 = fieldInfo.Name == name;
				if (!flag2 && fieldInfo.OldNames != null)
				{
					string[] oldNames = fieldInfo.OldNames;
					for (int i = 0; i < oldNames.Length; i++)
					{
						if (oldNames[i] == name)
						{
							flag2 = true;
							break;
						}
					}
				}
				if (flag2)
				{
					value.Add(b);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				value.Add(byte.MaxValue);
			}
			num++;
		}
		m_Mappings.Add(serializedType, value);
		return value;
	}

	public DeserializerState(byte version, TypeLibrary typeLibrary)
	{
		Version = version;
		TypeLibrary = typeLibrary;
	}
}
