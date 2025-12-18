using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Settings;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[Obsolete]
[TypeId("a41155f9dec4b9844bf843a46f8b47ae")]
public class CheckIronmanGetter : IntPropertyGetter
{
	[SerializeField]
	private bool m_IsInIronMan;

	public bool IsInIronMan => m_IsInIronMan;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"Check ironman is [{IsInIronMan}]";
	}

	protected override int GetBaseValue()
	{
		if (SettingsRoot.Difficulty.OnlyOneSave.GetValue() != m_IsInIronMan)
		{
			return 0;
		}
		return 1;
	}
}
