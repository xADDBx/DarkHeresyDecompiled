using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Visual.Critters;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[ComponentName("Root/FamiliarsRoot")]
[TypeId("2a5430fdd0f4404a9b9092149d3ca753")]
public class FamiliarsRoot : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<FamiliarsRoot>
	{
	}

	[SerializeField]
	[ValidateNotNull]
	private FamiliarSettings m_DefaultFamiliarSettings = new FamiliarSettings();

	public FamiliarSettings DefaultFamiliarSettings => m_DefaultFamiliarSettings;
}
