namespace Owlcat.UI;

public readonly struct LocalizedStringStub
{
	public static LocalizedStringStub Empty;

	private readonly string m_Value;

	private LocalizedStringStub(string value)
	{
		m_Value = value;
	}

	public static implicit operator string(LocalizedStringStub localizedString)
	{
		return localizedString.m_Value;
	}

	public static implicit operator LocalizedStringStub(string value)
	{
		return new LocalizedStringStub(value);
	}
}
