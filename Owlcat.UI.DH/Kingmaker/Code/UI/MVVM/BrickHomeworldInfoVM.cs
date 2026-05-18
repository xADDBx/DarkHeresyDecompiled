using Kingmaker.Code.Gameplay.Components.Features;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickHomeworldInfoVM : TooltipBrickVM
{
	private readonly HomeworldUISettings m_HomeworldUISettings;

	public string PlanetTitle => m_HomeworldUISettings?.PlanetName;

	public string PlanetDescription => m_HomeworldUISettings?.PlanetAdditionalDescription;

	public Sprite Picture => m_HomeworldUISettings?.Picture?.Load();

	public BrickHomeworldInfoVM(HomeworldUISettings homeworldUISettings)
	{
		m_HomeworldUISettings = homeworldUISettings;
	}
}
