namespace Owlcat.UI;

public interface IViewComposerRule
{
	bool IsForbidden(object candidate, object alreadyOnScreen);
}
