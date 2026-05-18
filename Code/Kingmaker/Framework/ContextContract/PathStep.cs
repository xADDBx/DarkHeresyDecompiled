namespace Kingmaker.Framework.ContextContract;

public readonly struct PathStep
{
	public object Element { get; }

	public string FieldName { get; }

	public string DisplayName => FieldName ?? Element?.GetType().Name ?? "?";

	public PathStep(object element, string fieldName = null)
	{
		Element = element;
		FieldName = fieldName;
	}
}
