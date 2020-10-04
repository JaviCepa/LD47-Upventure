using Com.LuisPedroFonseca.ProCamera2D;
using DG.Tweening;
using PlatformerPro;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public DarkLord darkLord;

    void Awake()
    {
        Fader.instance.HideInstantly();
        Fader.instance.ShowScreen();

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
            if (DEBUG_Levels != null && DEBUG_Levels.Count > 0)
            {
                foreach (Levels level in Enum.GetValues(typeof(Levels)))
                {
                    if (!DEBUG_Levels.Contains(level))
                    {
                        var obj = GetLevelObject(level);
                        if (obj != null)
                        {
                            obj.SetActive(false);
                        }
                    }
                }
            }
        }
    }

    public void PlayEnding()
    {
        SceneManager.LoadScene(1);
    }

    public void ChangeLevel(Levels newLevel)
    {
        if (currentLevel != newLevel)
        {
            var sequence = DOTween.Sequence();

            sequence.AppendCallback(() => Fader.instance.HideScreen());
            sequence.AppendInterval(0.5f);
            sequence.AppendCallback(() => {
                var previousLevel = currentLevel;
                currentLevel = newLevel;
                var previous = GetLevelObject(previousLevel);
                var current = GetLevelObject(currentLevel);
                current.SetActive(true);
                previous.SetActive(false);
            });
            sequence.AppendInterval(1.0f);
            sequence.AppendCallback(() => Fader.instance.ShowScreen());
        }
    }

    GameObject GetLevelObject(Levels level)
    {
        if ((int)level >= levelObjects.Count)
        {
            return null;
        }

        return levelObjects[(int)level];
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

        darkLord.OnCharacterDeath();
    }
}
