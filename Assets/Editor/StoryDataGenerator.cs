using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class StoryDataGenerator : EditorWindow
{
    [MenuItem("Tools/Story/テキストからStoryDataを自動生成")]
    public static void GenerateStoryData()
    {
        // 1. テキストファイルの選択
        string txtPath = EditorUtility.OpenFilePanel("テキストファイルを選択", "", "txt");
        if (string.IsNullOrEmpty(txtPath)) return;

        // 2. 保存先アセットの選択
        string savePath = EditorUtility.SaveFilePanelInProject("StoryDataの保存", "NewStoryData", "asset", "保存するファイル名を入力してください");
        if (string.IsNullOrEmpty(savePath)) return;

        string[] lines = File.ReadAllLines(txtPath);

        // 既存アセットのロードを試みる
        StoryData storyData = AssetDatabase.LoadAssetAtPath<StoryData>(savePath);
        bool isAppendMode = false;

        if (storyData != null)
        {
            isAppendMode = true;
            if (storyData.stories == null)
            {
                storyData.stories = new List<Story>();
            }
        }
        else
        {
            storyData = ScriptableObject.CreateInstance<StoryData>();
            storyData.stories = new List<Story>();
        }

        bool currentIsFadeIn = false;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            // タブかスペースでIDとコンテンツを分割
            int firstSpaceOrTab = line.IndexOfAny(new char[] { '\t', ' ' });
            if (firstSpaceOrTab == -1) continue;

            string idStr = line.Substring(0, firstSpaceOrTab).Trim();
            string content = line.Substring(firstSpaceOrTab + 1).Trim();

            int id;
            if (!int.TryParse(idStr, out id)) continue;

            if (id == 3) // 演出指示
            {
                if (content.Contains("【画面暗転】"))
                {
                    currentIsFadeIn = true;
                }
            }
            else if (id == 0 || id == 1 || id == 2) // 0:主人公, 1:ギャル, 2:ナレーション
            {
                Story newStory = new Story();
                
                // 【画面暗転】フラグを適用してリセット
                newStory.isFadeIn = currentIsFadeIn;
                currentIsFadeIn = false;

                if (id == 2) // ナレーション
                {
                    newStory.CharacterName = "";
                    newStory.StoryText = content;
                }
                else // セリフ
                {
                    // 「 」が含まれているかチェックし、キャラ名とセリフに分割する
                    int bracketStart = content.IndexOf('「');
                    int bracketEnd = content.LastIndexOf('」');

                    if (bracketStart != -1 && bracketEnd != -1 && bracketEnd > bracketStart)
                    {
                        newStory.CharacterName = content.Substring(0, bracketStart).Trim();
                        // 括弧を取り除かず、括弧を含めた部分をそのままセリフとする
                        newStory.StoryText = content.Substring(bracketStart).Trim();
                    }
                    else
                    {
                        // 括弧が見つからない場合はそのままセット（名前は仮として設定）
                        newStory.CharacterName = (id == 0) ? "主人公" : "ギャル";
                        newStory.StoryText = content;
                    }
                }

                storyData.stories.Add(newStory);
            }
        }

        // 3. アセットの保存
        if (!isAppendMode)
        {
            AssetDatabase.CreateAsset(storyData, savePath);
        }
        else
        {
            EditorUtility.SetDirty(storyData);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string modeText = isAppendMode ? "追記" : "新規作成";
        EditorUtility.DisplayDialog("完了", $"StoryDataを{modeText}しました。\n保存先: {savePath}\n\n※立ち絵や背景画像などの設定は、生成されたアセットのインスペクタから手動で行ってください。", "OK");
    }
}
