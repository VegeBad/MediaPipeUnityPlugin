namespace EugeneC.Utilities
{
	public enum ELanguage : byte
	{
		NotDefined = 0,
		English = 1,
		SimplifiedChinese = 2,
		TraditionalChinese = 3,
		Malay = 4,
	}

	public enum EAxis : byte
	{
		X = 0,
		Y = 1 << 1,
		Z = 1 << 2
	}

	public enum ControlSchemeEnum : byte
	{
		Keyboard,
		Gamepad
	}
}