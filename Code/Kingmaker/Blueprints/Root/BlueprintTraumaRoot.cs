using System;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Obsolete("New critical effects design")]
[TypeId("abe4dc1e69db4056a515a3e39315d44a")]
public class BlueprintTraumaRoot : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintTraumaRoot>
	{
	}

	[SerializeField]
	[ValidateNotNull]
	private BlueprintBuffReference m_FreshWound;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintBuffReference m_OldWound;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintBuffReference m_Trauma;

	public BlueprintBuff FreshWound => m_FreshWound;

	public BlueprintBuff OldWound => m_OldWound;

	public BlueprintBuff Trauma => m_Trauma;
}
