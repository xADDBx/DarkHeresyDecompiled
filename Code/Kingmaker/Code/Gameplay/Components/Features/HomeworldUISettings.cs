using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Components.Features;

[AllowedOn(typeof(BlueprintFeature))]
[TypeId("dea4ef4f36bb4b63b6d05a3104f29f64")]
public class HomeworldUISettings : BlueprintComponent
{
	[SerializeField]
	private LocalizedString m_PlanetName;

	[SerializeField]
	private LocalizedString m_PlanetDescription;

	[SerializeField]
	private LocalizedString m_PlanetAdditionalDescription;

	[SerializeField]
	private SpriteLink m_Icon;

	[SerializeField]
	private SpriteLink m_Picture;

	[SerializeField]
	private Color m_TempDollColor = Color.white;

	public LocalizedString PlanetName => m_PlanetName;

	public LocalizedString PlanetDescription => m_PlanetDescription;

	public LocalizedString PlanetAdditionalDescription => m_PlanetAdditionalDescription;

	public SpriteLink Icon => m_Icon;

	public SpriteLink Picture => m_Picture;

	public Color TempDollColor => m_TempDollColor;
}
