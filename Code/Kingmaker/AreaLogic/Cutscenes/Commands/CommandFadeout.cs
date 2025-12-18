using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.View.Bridge.Canvas;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[ComponentName("Command/CommandFadeout")]
[TypeId("c70d3959f8ce9d145a883fab41d59e62")]
public class CommandFadeout : CommandFadeoutBase
{
	[SerializeField]
	[HideIf("m_Continuous")]
	private float m_Lifetime = 1f;

	protected override float Lifetime => m_Lifetime;

	protected override void Fadeout(bool fade)
	{
		FadeCanvas.Instance.Fadeout(fade);
	}

	public override string GetCaption()
	{
		return "Fade screen";
	}
}
