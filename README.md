# unity-claude-launcher

Unity Editor のツールバーに Claude Code CLI 起動ボタンを追加するパッケージです。
ボタン1つで Windows Terminal に Claude が起動し、VOICEVOX も自動起動します。

## 動作環境

- Unity 6000.0 以上
- Windows Terminal (`wt`) がインストール済みであること
- [Claude Code](https://claude.ai/code) がインストール済みであること
- （任意）[VOICEVOX](https://voicevox.hiroshiba.jp/) がインストール済みであること

## インストール

Package Manager の **Add package from git URL** に以下を入力：

```
https://github.com/Nattuhan/unity-claude-launcher.git
```

## 使い方

インストール後、Unity Editor のツールバー右側に Claude アイコンが表示されます。
クリックすると、プロジェクトルートをワーキングディレクトリとして Windows Terminal で `claude --verbose` が起動します。

## ライセンス

MIT License
