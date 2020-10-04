using PlatformerPro;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { Loading, Intro, PressAnyKey, Gameplay }

public enum Levels { Level1, Level2, Level3, Level4, Level5 }

public class UpventureGameManager : MonoBehaviour
{

    public AudioSource introMusic;
    public AudioSource themeMusic;

    public Character character;

    public List<GameObject> levelObjects;

    [ReadOnly]
    public float time = 0;

    public GameState gameState;
    public Levels currentLevel = Levels.Level1;

    internal void ChangeLevel(Levels newLevel)
    {
        if (currentLevel != newLevel)
        {
            var previousLevel = currentLevel;
            currentLevel = newLevel;
            var previous = GetLevelObject(previousLevel);
            var current = GetLevelObject(currentLevel);
            current.SetActive(true);
            previous.SetActive(false);
        }
    }

    GameObject GetLevelObject(Levels level)
    {
        return levelObjects[(int)level];
    }

    public float introMinDuration = 10f;

    void Start()
    {
        gameState = GameState.Intro;
        character.InputEnabled = false;
        PlayIntro();
        var startLevel = GetLevelObject(Levels.Level1);
        foreach (var level in levelObjects)
        {
            if (level != startLevel)
            {
                level.SetActive(false);
            }
        }
    }

    void Update()
    {
        time = Time.time;
        switch (gameState)
        {
            case GameState.Intro:
                if (time > introMinDuration || Application.isEditor)
                {
                    gameState = GameState.PressAnyKey;
                    DisplayPressAnyKeyHint();
                }
                break;
            case GameState.PressAnyKey:
                if (UnityEngine.Input.anyKeyDown)
                {
                    StartGame();
                }
                break;
            case GameState.Gameplay:
                break;
            default:
                break;
        }

    }

    private void DisplayPressAnyKeyHint()
    {
        //Show "press any key" text
    }

    private void PlayIntro()
    {
        introMusic.Play();
    }

    void StartGame()
    {
        gameState = GameState.Gameplay;
        introMusic.Stop();
        themeMusic.Play();
        character.InputEnabled = true;
    }
}
