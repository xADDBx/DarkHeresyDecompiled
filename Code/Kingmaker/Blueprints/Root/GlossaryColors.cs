using System;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class GlossaryColors
{
	public Color32 GlossaryGlossary;

	public Color32 GlossaryDecisions;

	public Color32 GlossaryMechanics;

	public Color32 GlossaryDefault;

	public Color32 GlossaryEmpty;

	public string GlossaryGlossaryHTML => "#" + ColorUtility.ToHtmlStringRGB((Color)GlossaryGlossary);

	public string GlossaryDecisionsHTML => "#" + ColorUtility.ToHtmlStringRGB((Color)GlossaryDecisions);

	public string GlossaryMechanicsHTML => "#" + ColorUtility.ToHtmlStringRGB((Color)GlossaryMechanics);

	public string GlossaryDefaultHTML => "#" + ColorUtility.ToHtmlStringRGB((Color)GlossaryDefault);

	public string GlossaryEmptyHTML => "#" + ColorUtility.ToHtmlStringRGB((Color)GlossaryEmpty);
}
