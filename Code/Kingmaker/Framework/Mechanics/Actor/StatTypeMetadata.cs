using System;
using System.Reflection;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using UnityEngine;

namespace Kingmaker.Framework.Mechanics.Actor;

public static class StatTypeMetadata
{
	private static Func<MechanicEntity, int>?[] _ReadonlyProviders = Array.Empty<Func<MechanicEntity, int>>();

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Initialize()
	{
		_ReadonlyProviders = new Func<MechanicEntity, int>[StatTypeHelper.AllStatsArraySize];
	}

	public static void RegisterReadonly(StatType stat, Func<MechanicEntity, int> provider)
	{
		if (stat < StatType.Unknown || (int)stat >= _ReadonlyProviders.Length)
		{
			throw new ArgumentOutOfRangeException("stat", $"StatType {stat} ({(int)stat}) out of range");
		}
		_ReadonlyProviders[(int)stat] = provider;
	}

	public static bool TryGetReadonlyProvider(StatType stat, out Func<MechanicEntity, int> provider)
	{
		if (stat >= StatType.Unknown && (int)stat < _ReadonlyProviders.Length)
		{
			Func<MechanicEntity, int> func = _ReadonlyProviders[(int)stat];
			if (func != null)
			{
				provider = func;
				return true;
			}
		}
		provider = null;
		return false;
	}

	public static bool IsReadonly(StatType stat)
	{
		if (stat >= StatType.Unknown && (int)stat < _ReadonlyProviders.Length)
		{
			return _ReadonlyProviders[(int)stat] != null;
		}
		return false;
	}

	public static void ValidateRegistrations()
	{
		FieldInfo[] fields = typeof(StatType).GetFields(BindingFlags.Static | BindingFlags.Public);
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo.GetCustomAttribute<NonModifiableStatAttribute>() != null)
			{
				StatType statType = (StatType)fieldInfo.GetValue(null);
				if (!IsReadonly(statType))
				{
					PFLog.Default.Error(string.Format("StatTypeMetadata: {0}.{1} has ", "StatType", statType) + "[NonModifiableStatAttribute] but no provider registered");
				}
			}
		}
	}
}
