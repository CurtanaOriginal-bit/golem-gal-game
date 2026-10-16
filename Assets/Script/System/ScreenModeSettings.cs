using UnityEngine;

/// <summary>
/// フルスクリーン / ウィンドウ表示の保存と適用を行うクラス。
/// タイトル画面・メイン画面の両方の Model から利用する。
/// </summary>
public static class ScreenModeSettings
{
    private const string FullScreenKey = "FullScreen";

    // ウィンドウ表示時のサイズ（Player Settings の Default Screen Width / Height と合わせる）
    private const int WindowedWidth = 1520;
    private const int WindowedHeight = 720;

    /// <summary>
    /// 保存されている表示モードを返す。未保存ならウィンドウ表示（初期サイズ）とする。
    /// </summary>
    public static bool Load()
    {
        return PlayerPrefs.GetInt(FullScreenKey, 0) == 1;
    }

    /// <summary>
    /// 表示モードを保存して適用する。
    /// </summary>
    public static void Save(bool isFullScreen)
    {
        PlayerPrefs.SetInt(FullScreenKey, isFullScreen ? 1 : 0);
        PlayerPrefs.Save();
        Apply(isFullScreen);
    }

    /// <summary>
    /// 表示モードを適用する。
    /// </summary>
    public static void Apply(bool isFullScreen)
    {
        if (isFullScreen)
        {
            Resolution display = Screen.currentResolution;
            Screen.SetResolution(display.width, display.height, FullScreenMode.FullScreenWindow);
        }
        else
        {
            Screen.SetResolution(WindowedWidth, WindowedHeight, FullScreenMode.Windowed);
        }
    }
}
