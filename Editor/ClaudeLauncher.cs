using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// Claude AI コマンドラインインターフェースを起動するためのユーティリティクラス
/// </summary>
public static class ClaudeLauncher
{
    private static Texture2D claudeIcon;

    /// <summary>
    /// Claude起動ツールバーボタン
    /// </summary>
    [MainToolbarElement("ClaudeLauncher",
        defaultDockPosition = MainToolbarDockPosition.Right,
        defaultDockIndex = 0)]
    public static MainToolbarElement ClaudeButton()
    {
        LoadIcon();
        var content = claudeIcon != null
            ? new MainToolbarContent("", claudeIcon, "Claude起動")
            : new MainToolbarContent("Claude", "Claude起動");
        return new MainToolbarButton(content, Launch);
    }

    private static void LoadIcon()
    {
        if (claudeIcon != null) return;

        claudeIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.nattuhan.claude-launcher/Editor/Icon/claude-ai-icon.png");
        if (claudeIcon == null) Debug.LogWarning("[ClaudeLauncher] アイコンが見つかりません");
    }

    /// <summary>
    /// Windows TerminalでClaude CLIを起動する
    /// ワーキングディレクトリはUnityプロジェクトルートに設定
    /// VOICEVOXも同時起動する
    /// </summary>
    private static void Launch()
    {
        try
        {
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            if (projectRoot == null)
            {
                Debug.LogError("[ClaudeLauncher] プロジェクトルートディレクトリを取得できませんでした");
                return;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = "wt",
                Arguments = $"--title \"Claude\" -d \"{projectRoot}\" cmd /k \"claude --verbose\"",
                UseShellExecute = false,
                CreateNoWindow = false,
            };
            // Claude Code内からUnityを起動した場合、CLAUDECODE環境変数が引き継がれてネストエラーになるため除去
            startInfo.EnvironmentVariables.Remove("CLAUDECODE");

            Process.Start(startInfo);
            Debug.Log($"[ClaudeLauncher] Claude起動 (ワーキングディレクトリ: {projectRoot})");

            LaunchVoiceVox();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ClaudeLauncher] 起動エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// VOICEVOXを起動する。既に起動中の場合はスキップ
    /// </summary>
    private static void LaunchVoiceVox()
    {
        try
        {
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    if (process.ProcessName.Contains("VOICEVOX", StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.Log("[ClaudeLauncher] VOICEVOXは既に起動しています（起動スキップ）");
                        return;
                    }
                }
                catch (InvalidOperationException) { }
                catch (System.ComponentModel.Win32Exception) { }
            }

            var voiceVoxPath = $@"C:\Users\{Environment.UserName}\AppData\Local\Programs\VOICEVOX\VOICEVOX.exe";
            if (!File.Exists(voiceVoxPath))
            {
                Debug.LogWarning($"[ClaudeLauncher] VOICEVOXが見つかりませんでした: {voiceVoxPath}");
                return;
            }

            Process.Start(new ProcessStartInfo { FileName = voiceVoxPath, UseShellExecute = true });
            Debug.Log($"[ClaudeLauncher] VOICEVOX起動: {voiceVoxPath}");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[ClaudeLauncher] VOICEVOX起動エラー: {ex.Message}");
        }
    }
}
