using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("1373b0e272ac47d7b99dd6ee1030ab6e")]
public class UnlockableFlagValueGetter : IntPropertyGetter
{
	[SerializeField]
	private BlueprintUnlockableFlagReference m_Flag;

	public BlueprintUnlockableFlag Flag => m_Flag;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"Value of {Flag}";
	}

	protected override int GetBaseValue()
	{
		return Flag.Value;
	}
}
