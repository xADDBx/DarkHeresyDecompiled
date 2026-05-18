using JetBrains.Annotations;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.Code.UI.MVVM;

public class BrickTextVM : TooltipBrickVM
{
	public readonly string Text;

	public readonly TooltipTextType Type;

	public readonly TooltipTextAlignment Alignment;

	[CanBeNull]
	public readonly MechanicEntity Owner;

	public BrickTextVM(string text, TooltipTextType type = TooltipTextType.Simple, TooltipTextAlignment alignment = TooltipTextAlignment.Midl, [CanBeNull] MechanicEntity owner = null)
	{
		Text = text;
		Type = type;
		Alignment = alignment;
		Owner = owner;
	}
}
