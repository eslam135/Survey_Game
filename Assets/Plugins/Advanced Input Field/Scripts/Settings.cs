using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	public class Settings
	{
		private const string SETTINGS_PATH = "AdvancedInputField/Settings";

		private static SettingsData data;

		private static SettingsData Data
		{
			get
			{
				if(data == null)
				{
					data = Resources.Load(SETTINGS_PATH) as SettingsData;
				}

				return data;
			}
		}

		public static LocalizationData[] Localizations { get { return Data.Localizations; } }
		public static MobileKeyboardBehaviour MobileKeyboardBehaviour { get { return Data.MobileKeyboardBehaviour; } }
		public static ActionBar AndroidActionBarPrefab { get { return Data.AndroidActionBarPrefab; } }
		public static ActionBar IOSActionBarPrefab { get { return Data.IOSActionBarPrefab; } }
		public static ActionBar UWPActionBarPrefab { get { return Data.UWPActionBarPrefab; } }
		public static MobileCursorsControl MobileSelectionCursorsPrefab { get { return Data.MobileSelectionCursorsPrefab; } }
		public static float MobileSelectionCursorsScale { get { return Data.MobileSelectionCursorsScale; } }
		public static bool SimulateMobileBehaviourInEditor { get { return Data.SimulateMobileBehaviourInEditor; } }
		public static Canvas PortraitKeyboardCanvasPrefab { get { return Data.PortraitKeyboardCanvasPrefab; } }
		public static Canvas LandscapeKeyboardCanvasPrefab { get { return Data.LandscapeKeyboardCanvasPrefab; } }

		/// <summary>The ActionBar prefab for current platform</summary>
		public static ActionBar ActionBarPrefab
		{
			get
			{
#if UNITY_ANDROID
				return AndroidActionBarPrefab;
#elif UNITY_IOS
				return IOSActionBarPrefab;
#else
				return UWPActionBarPrefab;
#endif
			}
		}
	}
}
