using UnityEngine.SceneManagement;

/// <summary>
/// 指定したシーンへ遷移するためのクラスです。
/// </summary>
public sealed class SceneLoader
{
    /// <summary>
    /// 遷移先のシーン名
    /// </summary>
    private readonly string _sceneName;

    /// <summary>
    /// 遷移先のシーン名を指定してローダーを作成します。
    /// </summary>
    public SceneLoader(string sceneName)
    {
        _sceneName = sceneName;
    }

    /// <summary>
    /// シーンを読み込みます。
    /// </summary>
    public void Load()
    {
        SceneManager.LoadScene(_sceneName);
    }
}
