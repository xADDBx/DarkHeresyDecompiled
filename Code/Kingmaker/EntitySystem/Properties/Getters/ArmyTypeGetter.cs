using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("632fcd2b80d228149945aceb30f308b7")]
public class ArmyTypeGetter : BoolPropertyGetter
{
	[HideIf("SpecificArmyType")]
	public bool Human;

	[HideIf("SpecificArmyType")]
	public bool Xenos;

	[HideIf("SpecificArmyType")]
	public bool Daemon;

	public bool SpecificArmyType;

	[SerializeField]
	[ShowIf("SpecificArmyType")]
	private BlueprintArmyTypeReference[] m_Armies = new BlueprintArmyTypeReference[0];

	public ReferenceArrayProxy<BlueprintArmyType> Armies
	{
		get
		{
			BlueprintReference<BlueprintArmyType>[] armies = m_Armies;
			return armies;
		}
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		List<string> list = new List<string>();
		if (SpecificArmyType)
		{
			BlueprintArmyTypeReference[] armies = m_Armies;
			foreach (BlueprintArmyTypeReference blueprintArmyTypeReference in armies)
			{
				list.Add(blueprintArmyTypeReference.NameSafe());
			}
		}
		else
		{
			if (Human)
			{
				list.Add("Human");
			}
			if (Xenos)
			{
				list.Add("Xenos");
			}
			if (Daemon)
			{
				list.Add("Daemon");
			}
		}
		return FormulaTargetScope.Current + " is from any of army types (" + string.Join("|", list) + ")";
	}

	protected override bool GetBaseValue()
	{
		if (!(base.CurrentEntity.Blueprint is BlueprintUnit { Army: { } army }))
		{
			return false;
		}
		if (SpecificArmyType)
		{
			return HasSpecificArmyType(army);
		}
		bool isHuman = army.IsHuman;
		bool isXenos = army.IsXenos;
		bool isDaemon = army.IsDaemon;
		if (!(Human && isHuman) && !(Xenos && isXenos))
		{
			return Daemon && isDaemon;
		}
		return true;
	}

	private bool HasSpecificArmyType(BlueprintArmyType army)
	{
		BlueprintArmyTypeReference[] armies = m_Armies;
		foreach (BlueprintArmyTypeReference blueprintArmyTypeReference in armies)
		{
			if (army == blueprintArmyTypeReference.Get())
			{
				return true;
			}
		}
		return false;
	}
}
