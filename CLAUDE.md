# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Alias** is a Unity-based English vocabulary learning game (Android) where players guess/explain words at CEFR proficiency levels (A1–C2). Built with Unity 2022.3.51f1, C# 9.0, .NET Framework 4.7.1.

## Architecture

**Scene flow (build index order):**
0. **MenuScene** — Start screen with Google Sheets toggle (`MenuUiController`)
1. **CategoryScene** — Level selection + mode dropdown: Practice vs Challenge (`CategoryUiController`, `LevelButton`)
2. **WordScene** — Practice mode: random words at selected level, can repeat (`WordsManager`)
3. **ChallengeScene** — Challenge mode: sequential non-repeating words from shuffled full list (`ChallengeController`)

**Data pipeline:**
- `CSVReader` (singleton prefab, persists across scenes via `DontDestroyOnLoad`) loads words from either local `Assets/StreamingAssets/words.csv` or a published Google Sheets CSV URL via `UnityWebRequest`
- Words are parsed into `Word` objects (word, level, hint) and organized into 6 level-based lists plus one shuffled full list
- `WordUiController` handles display of word/hint and audio feedback (shared by both game modes)

**Key data class:** `Word` — serializable POCO with `word`, `level`, `hint` string fields.

## Build

- **Platform:** Android (APK output in `Builds/`)
- **Unity version:** 2022.3.51f1
- Build via Unity Editor: File → Build Settings → Build
- No CI/CD pipeline configured
- No test files exist (test-framework package is installed but unused)

## External Data

Google Sheets integration requires a published sheet URL in the format:
```
https://docs.google.com/spreadsheets/d/e/{SHEET_ID}/pub?output=csv
```
Set on the `CSVReader` component's public `url` field. The CSV must have 3 columns: Word, Level, Hint.

## Dependencies

All Unity built-in packages — no external NuGet or third-party dependencies. Key packages: TextMeshPro (text rendering), ugui (UI system).
