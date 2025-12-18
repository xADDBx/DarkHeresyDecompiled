using System;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[Obsolete]
[TypeId("76fe9bda9aacb324b9c638394e232536")]
public class UnlockableFlagStateGetter : IntPropertyGetter
{
	[SerializeField]
	private BlueprintUnlockableFlagReference m_Flag;

	public BlueprintUnlockableFlag Flag => m_Flag;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"State of {Flag}";
	}

	protected override int GetBaseValue()
	{
		if (!Flag.IsUnlocked)
		{
			return 0;
		}
		return 1;
	}
}
