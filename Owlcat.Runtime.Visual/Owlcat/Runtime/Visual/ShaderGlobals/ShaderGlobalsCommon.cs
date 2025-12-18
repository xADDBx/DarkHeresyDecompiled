using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Burst;
using UnityEngine;

namespace Owlcat.Runtime.Visual.ShaderGlobals;

public static class ShaderGlobalsCommon
{
	public enum GlobalKind
	{
		Static,
		Dynamic
	}

	public readonly struct ReferenceNameKey : IEquatable<ReferenceNameKey>
	{
		public readonly string GlobalName;

		public readonly Type Type;

		public readonly GlobalKind GlobalKind;

		public ReferenceNameKey(string globalName, Type type, GlobalKind globalKind)
		{
			GlobalName = globalName;
			Type = type;
			GlobalKind = globalKind;
		}

		[BurstDiscard]
		public bool Equals(ReferenceNameKey other)
		{
			if (GlobalName == other.GlobalName && Type == other.Type)
			{
				return GlobalKind == other.GlobalKind;
			}
			return false;
		}

		[BurstDiscard]
		public override bool Equals(object obj)
		{
			if (obj is ReferenceNameKey other)
			{
				return Equals(other);
			}
			return false;
		}

		[BurstDiscard]
		public override int GetHashCode()
		{
			return (((((GlobalName != null) ? GlobalName.GetHashCode() : 0) * 397) ^ ((Type != null) ? Type.GetHashCode() : 0)) * 397) ^ (int)GlobalKind;
		}
	}

	public const string kResourcesConfigPath = "ShaderGlobalsConfig";

	public const string kConfigPath = "Assets/Resources/ShaderGlobalsConfig";

	public const string kConfigPathWithExtension = "Assets/Resources/ShaderGlobalsConfig.asset";

	private const string kShaderReferencePrefix = "_Owlcat_ShaderGlobal";

	private static readonly Dictionary<Type, string> s_PrimitiveTypeAliases = new Dictionary<Type, string>
	{
		[typeof(int)] = "int",
		[typeof(float)] = "float",
		[typeof(Vector4)] = "float4",
		[typeof(Color)] = "color"
	};

	private static readonly Dictionary<ReferenceNameKey, string> s_ReferenceNameCache = new Dictionary<ReferenceNameKey, string>();

	public static bool TryLoadConfig(out WaaaghShaderGlobalsConfig config)
	{
		config = Resources.Load<WaaaghShaderGlobalsConfig>("ShaderGlobalsConfig");
		return config != null;
	}

	public static void EnsureConfig()
	{
	}

	private static string GetTypeName(Type type)
	{
		if (!s_PrimitiveTypeAliases.TryGetValue(type, out var value))
		{
			return type.Name;
		}
		return value;
	}

	public static string BuildReferenceName(in ReferenceNameKey nameKey)
	{
		if (s_ReferenceNameCache.TryGetValue(nameKey, out var value))
		{
			return value;
		}
		string typeName = GetTypeName(nameKey.Type);
		string text = string.Format("{0}_{1}_{2}_{3}", "_Owlcat_ShaderGlobal", nameKey.GlobalKind, typeName, nameKey.GlobalName);
		s_ReferenceNameCache.Add(nameKey, text);
		return text;
	}

	public static string BuildVectorComponentName(string baseName, int componentIndex)
	{
		return $"{baseName}_{componentIndex:00}";
	}

	[MustUseReturnValue]
	public static string CreateIncludePath(GlobalKind globalKind)
	{
		return string.Format("{0}.Generated.{1}.hlsl", "Assets/Resources/ShaderGlobalsConfig", globalKind);
	}

	public static string BufferCountName(string referenceName)
	{
		return "count" + referenceName;
	}
}
