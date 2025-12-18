using System.Text.RegularExpressions;
using Kingmaker.TextTools.Base;

namespace Kingmaker.Code.View.Bridge.Interfaces;

public interface ITextTemplateEngine
{
	string Process(string text);

	TextTemplate GetTemplate(Match match, out string tag, out string[] parameters, out bool capitalized);

	string[] GetTemplateTags();
}
