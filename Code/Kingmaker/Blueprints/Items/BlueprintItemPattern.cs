using System;
using Kingmaker.Localization;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Items;

[Obsolete]
[TypeId("44b1b3d7e265f9b4989a14ce5315433c")]
public class BlueprintItemPattern : BlueprintScriptableObject
{
	[SerializeField]
	private LocalizedString m_DisplayNameText;

	[SerializeField]
	private LocalizedString m_DescriptionText;

	[SerializeField]
	private LocalizedString m_FlavorText;
}
