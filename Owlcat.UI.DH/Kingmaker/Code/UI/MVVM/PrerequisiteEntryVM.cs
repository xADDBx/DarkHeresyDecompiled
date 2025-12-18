using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Settings;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class PrerequisiteEntryVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly bool IsGroup;

	public readonly bool IsOrComposition;

	public readonly List<PrerequisiteEntryVM> Prerequisites;

	public readonly string Text;

	public readonly string Value;

	public readonly bool Done;

	public readonly bool Inverted;

	public readonly float FontMultiplier = FontSizeMultiplier;

	private static float FontSizeMultiplier => SettingsRoot.Accessiability.FontSizeMultiplier;

	public PrerequisiteEntryVM(string text, bool done, bool inverted, string value = null)
	{
		IsGroup = false;
		Text = text;
		Value = value ?? string.Empty;
		Done = done;
		Inverted = inverted;
	}

	public PrerequisiteEntryVM(List<PrerequisiteEntryVM> prerequisites, bool isOrComposition)
	{
		IsGroup = true;
		IsOrComposition = isOrComposition;
		Prerequisites = prerequisites;
		Text = string.Join("\n", prerequisites.Select((PrerequisiteEntryVM p) => p.Text));
	}

	protected override void DisposeImplementation()
	{
	}
}
