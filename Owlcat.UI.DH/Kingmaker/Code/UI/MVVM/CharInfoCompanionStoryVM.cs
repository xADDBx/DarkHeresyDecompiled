using Kingmaker.Blueprints;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoCompanionStoryVM : ViewModel
{
	public readonly string Title;

	public readonly string StoryText;

	public readonly Sprite Picture;

	public CharInfoCompanionStoryVM(BlueprintCompanionStory story)
	{
		Title = story?.Title;
		StoryText = story?.Description;
		Picture = story?.Image;
	}
}
