namespace EugeneC.Utilities
{
	public enum ELanguage : byte
	{
		English = 0,
		SimplifiedChinese = 1,
		TraditionalChinese = 2,
		Malay = 3,
		NotDefined = byte.MaxValue,
	}

	public enum EAxis : byte
	{
		X = 0,
		Y = 1 << 0,
		Z = 1 << 1
	}

	public enum ControlSchemeEnum : byte
	{
		Keyboard,
		Gamepad,
		Touchscreen,
		XR,
		NotDefined = byte.MaxValue
	}
	
	public enum FitMode : byte
	{
		Expand = 0,
		Shrink = 1,
		FitWidth = 1 << 1,
		FitHeight = 1 << 2,
	}
}