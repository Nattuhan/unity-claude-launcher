using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// Unity Editor ツールバーから AI CLI を起動するユーティリティ
/// </summary>
public static class AiLauncherToolbar
{
    private static Texture2D claudeIcon;
    private static Texture2D codexIcon;

    /// <summary>
    /// Claude起動ツールバーボタン
    /// </summary>
    [MainToolbarElement("AiLauncherClaudeButton",
        defaultDockPosition = MainToolbarDockPosition.Right,
        defaultDockIndex = 200)]
    public static MainToolbarElement ClaudeButton()
    {
        LoadClaudeIcon();
        var content = claudeIcon != null
            ? new MainToolbarContent("", claudeIcon, "Claude起動")
            : new MainToolbarContent("Claude", "Claude起動");
        return new MainToolbarButton(content, LaunchClaude);
    }

    /// <summary>
    /// Codex起動ツールバーボタン
    /// </summary>
    [MainToolbarElement("AiLauncherCodexButton",
        defaultDockPosition = MainToolbarDockPosition.Right,
        defaultDockIndex = 201)]
    public static MainToolbarElement CodexButton()
    {
        LoadCodexIcon();
        var content = codexIcon != null
            ? new MainToolbarContent("", codexIcon, "Codex起動")
            : new MainToolbarContent("Codex", "Codex起動");
        return new MainToolbarButton(content, LaunchCodex);
    }

    /// <summary>
    /// Windows TerminalでClaude CLIを起動する
    /// ワーキングディレクトリはUnityプロジェクトルートに設定
    /// VOICEVOXも同時起動する
    /// </summary>
    private static void LaunchClaude()
    {
        LaunchCli("Claude", "claude --verbose", "[AI Launcher] Claude起動");
    }

    /// <summary>
    /// Windows TerminalでCodex CLIを起動する
    /// ワーキングディレクトリはUnityプロジェクトルートに設定
    /// VOICEVOXも同時起動する
    /// </summary>
    private static void LaunchCodex()
    {
        LaunchCli("Codex", "codex --full-auto", "[AI Launcher] Codex起動");
    }

    private static void LoadCodexIcon()
    {
        if (codexIcon != null) return;

        codexIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.nattuhan.ai-launcher/Editor/Icon/codex-ai-icon.png");
        if (codexIcon == null) Debug.LogWarning("[AI Launcher] Codexアイコンが見つかりません");
    }

    private static void LoadClaudeIcon()
    {
        if (claudeIcon != null) return;

        claudeIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.nattuhan.ai-launcher/Editor/Icon/claude-ai-icon.png");
        if (claudeIcon == null) Debug.LogWarning("[AI Launcher] Claudeアイコンが見つかりません");
    }

    private static void LaunchCli(string title, string command, string successLogPrefix)
    {
        try
        {
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            if (projectRoot == null)
            {
                Debug.LogError("[AI Launcher] プロジェクトルートディレクトリを取得できませんでした");
                return;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = "wt",
                // --window 0: 既存のWindows Terminalウィンドウがあればそこにタブとして開く（なければ新規ウィンドウ）
                Arguments = $"--window 0 new-tab --title \"{title}\" -d \"{projectRoot}\" cmd /k \"{command}\"",
                UseShellExecute = false,
                CreateNoWindow = false,
            };

            // AI CLI内からUnityを起動した場合のネスト問題を避ける
            startInfo.EnvironmentVariables.Remove("CLAUDECODE");

            Process.Start(startInfo);
            Debug.Log($"{successLogPrefix} (ワーキングディレクトリ: {projectRoot})");

            LaunchVoiceVox();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AI Launcher] 起動エラー: {ex.Message}");
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
                        Debug.Log("[AI Launcher] VOICEVOXは既に起動しています（起動スキップ）");
                        return;
                    }
                }
                catch (InvalidOperationException) { }
                catch (System.ComponentModel.Win32Exception) { }
            }

            var voiceVoxPath = $@"C:\Users\{Environment.UserName}\AppData\Local\Programs\VOICEVOX\VOICEVOX.exe";
            if (!File.Exists(voiceVoxPath))
            {
                Debug.LogWarning($"[AI Launcher] VOICEVOXが見つかりませんでした: {voiceVoxPath}");
                return;
            }

            Process.Start(new ProcessStartInfo { FileName = voiceVoxPath, UseShellExecute = true });
            Debug.Log($"[AI Launcher] VOICEVOX起動: {voiceVoxPath}");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[AI Launcher] VOICEVOX起動エラー: {ex.Message}");
        }
    }
}
