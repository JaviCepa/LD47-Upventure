using Com.LuisPedroFonseca.ProCamera2D;
using PlatformerPro;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { Loading, Intro, PressAnyKey, Gameplay }

public enum Levels { Level1, Level2, Level3, Level4, Level5, Final }

public class UpventureGameManager : MonoBehaviour
{

    public AudioSource introMusic;
    public AudioSource themeMusic;

    public Character character;
    public GameObject spawnPoint;
    public List<GameObject> levelObjects;

    [ReadOnly]
    public float time = 0;

    public GameState gameState;
    public Levels currentLevel = Levels.Level1;

    public float timeSinceStart => Time.time - startTime;
    float startTime = 0;

    public float introMinDuration = 10f;
    public static UpventureGameManager instance;

    public List<Levels> DEBUG_Levels;

    [HideInInspector]
    public List<RestoreOnPlayerDeath> restorables = new List<RestoreOnPlayerDeath>();
    public ProCamera2D proCamera;

    public void ChangeLevel(Levels newLevel)
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

    void Awake()
    {
        instance = this;
        gameState = GameState.Intro;
        character.InputEnabled = false;
        PlayIntro();
        var startLevel = GetLevelObject(Levels.Level1);
        if (!Application.isEditor)
        {
            foreach (var level in levelObjects)
            {
                if (level != startLevel)
                {
                    level.SetActive(false);
                }
            }
        }
        else
        {
            foreach (Levels level in Enum.GetValues(typeof(Levels)))
            {
                if (!DEBUG_Levels.Contains(level))
                {
                    GetLevelObject(level).SetActive(false);
                }
            }
        }
    }

    internal void ShakeScreen(int preset)
    {
        proCamera.GetComponent<ProCamera2DShake>().Shake(preset);
    }

    void Update()
    {
        time = Time.time;
        switch (gameState)
        {
            case GameState.Intro:
                startTime = Time.time;
                if (time > introMinDuration || Application.isEditor)
                {
                    gameState = GameState.PressAnyKey;
                    DisplayPressAnyKeyHint();
                }
                break;
            case GameState.PressAnyKey:
                startTime = Time.time;
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

    void OnHeroDeath()
    {
        foreach (var restorable in restorables)
        {
            if (restorable != null && restorable.gameObject.activeSelf)
            {
                restorable.OnPlayerDeath();
            }
        }
    }
}
