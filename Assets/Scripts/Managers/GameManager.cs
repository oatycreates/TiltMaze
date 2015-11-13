/**
 * File: GameManager.cs
 * Author: Patrick Ferguson
 * Maintainer: Patrick Ferguson
 * Created: 12/11/2015
 * Copyright: (c) 2015 Patrick Ferguson, All Rights Reserved.
 * Description: Manages state transitions.
 **/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace ProjectLunar
{
    /// <summary>
    /// Manages state transitions.
    /// </summary>
    class GameManager : MonoBehaviour
    {
        /// <summary>
        /// Represents a particular set of enabled gameobjects to describe a gameplay state.
        /// </summary>
        public enum EGameState
        {
            NONE, 
            PREGAME, 
            PLAY, 
            GAMEOVER
        };

        /// <summary>
        /// Amount of score to get per second.
        /// </summary>
        public float scorePerSec = 10.0f;

        /// <summary>
        /// How many times to divide up the screen for the various score multipliers.
        /// </summary>
        public int screenScoreMultDivs = 3;

        /// <summary>
        /// Maximum score multiplier for the top screen section.
        /// </summary>
        public int maxScreenScoreMult = 3;

        /// <summary>
        /// How long to wait before hiding the score indicator.
        /// </summary>
        public float gameHighScoreHideWait = 3.5f;

        /// <summary>
        /// Handle to the obstacle spawner.
        /// </summary>
        public ObstacleSpawner obstacleSpawner = null;

        /// <summary>
        /// 'Press anywhere to play' text object.
        /// </summary>
        public GameObject beginText = null;

        /// <summary>
        /// 'Game over!' text object.
        /// </summary>
        public GameObject gameOverText = null;

        /// <summary>
        /// 'Score: #' text object.
        /// </summary>
        public GameObject scoreText = null;

        /// <summary>
        /// 'High score: #' text object.
        /// </summary>
        public GameObject highScoreText = null;

        /// <summary>
        /// Handle to the player.
        /// </summary>
        public GameObject player = null;

        /// <summary>
        /// Current game state.
        /// </summary>
        private EGameState m_currState = EGameState.NONE;
        
        /// <summary>
        /// Previous game state.
        /// </summary>
        private EGameState m_prevState = EGameState.NONE;

        /// <summary>
        /// Short wait to prevent instantly replaying the game.
        /// </summary>
        private float m_replayWait = -1.0f;

        /// <summary>
        /// Current player's score.
        /// </summary>
        private float m_score = 0.0f;

        /// <summary>
        /// Current game high score.
        /// </summary>
        private float m_highScore = 0.0f;

        // Cached references
        private Transform m_playerTrans = null;
        private PlayerInputManager m_playerInput = null;

        /// <summary>
        /// Called while objects are being initialised.
        /// </summary>
        void Awake()
        {
            // Cache variables
            m_playerTrans = player.transform;
            m_playerInput = player.GetComponent<PlayerInputManager>();

            // Reset variables
            m_score = 0.0f;
        }

        /// <summary>
        /// Called once all objects have been created and initialised.
        /// </summary>
        void Start()
        {
            // Begin in pre-game state
            ChangeToState(EGameState.PREGAME);
        }

        /// <summary>
        /// Called once per frame.
        /// </summary>
        void Update()
        {
            InputTick();

            UpdateTick();
        }

        /// <summary>
        /// Handles input sampling.
        /// </summary>
        private void InputTick()
        {

        }

        /// <summary>
        /// Handles state updating.
        /// </summary>
        private void UpdateTick()
        {
            if (m_replayWait > 0)
            {
                m_replayWait -= Time.unscaledDeltaTime;
            }

            // Update the current state
            switch (m_currState)
            {
                case EGameState.PREGAME:
                {
                    // Press anywhere to begin
                    if (AnyPressDown())
                    {
                        ChangeToState(EGameState.PLAY);
                    }

                    break;
                }
                case EGameState.PLAY:
                {
                    // Enable high score text if getting a high score
                    if (m_score + scorePerSec * Time.deltaTime >= m_highScore)
                    {
                        highScoreText.SetActive(true);
                    }

                    // Calculate screen score multiplier
                    int scoreMult = CalcScoreMultiplier();

                    // Increase score
                    m_score += scorePerSec * scoreMult * Time.deltaTime;

                    // Update score display
                    scoreText.GetComponent<Text>().text = "Score: " + Mathf.RoundToInt(m_score) + " x" + scoreMult;

                    if (m_score >= m_highScore)
                    {
                        // Update high score display
                        highScoreText.GetComponent<Text>().text = "High score!";
                    }

                    break;
                }
                case EGameState.GAMEOVER:
                {
                    // Press anywhere to replay
                    if (AnyPressDown() && m_replayWait <= 0)
                    {
                        Application.LoadLevel(Application.loadedLevel);
                        //ChangeToState(GameState.PLAY);
                    }

                    break;
                }
                case EGameState.NONE:
                {
                    Debug.LogWarning("Updating NONE game-play state!");
                    break;
                }
                default:
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Changes to the input state.
        /// </summary>
        /// <param name="a_state">State to change to.</param>
        public void ChangeToState(EGameState a_state)
        {
            // Switch on and off game components to match state
            switch (a_state)
            {
                case EGameState.PREGAME:
                    {
                        obstacleSpawner.isSpawning = false;
                        beginText.SetActive(true);
                        gameOverText.SetActive(false);
                        scoreText.SetActive(false);

                        // Update high score
                        m_highScore = PlayerPrefs.GetFloat("HighScore", 0);
                        // Update high score display
                        highScoreText.GetComponent<Text>().text = "High score: " + Mathf.RoundToInt(m_highScore);

                        break;
                    }
                case EGameState.PLAY:
                    {
                        obstacleSpawner.isSpawning = true;
                        beginText.SetActive(false);
                        scoreText.SetActive(true);

                        // Hide the high score text after a few seconds
                        Invoke("HideHighScoreText", gameHighScoreHideWait);

                        // Resume time during game
                        Time.timeScale = 1.0f;

                        break;
                    }
                case EGameState.GAMEOVER:
                    {
                        obstacleSpawner.isSpawning = false;
                        gameOverText.SetActive(true);

                        // Short wait before replaying
                        m_replayWait = 1.0f;

                        // Update high score
                        if (m_score >= m_highScore)
                        {
                            m_highScore = m_score;
                            PlayerPrefs.SetFloat("HighScore", m_highScore);
                        }

                        // Freeze time during game over
                        Time.timeScale = 0.0f;

                        break;
                    }
                case EGameState.NONE:
                    {
                        Debug.LogWarning("Changing to NONE gameplay state!");
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            // Apply state change
            m_prevState = m_currState;
            m_currState = a_state;
        }

        /// <summary>
        /// Returns the current game state.
        /// </summary>
        /// <returns>Current game state.</returns>
        public EGameState GetCurrentState()
        {
            return m_currState;
        }

        private static bool AnyPressDown()
        {
            return Input.touchCount > 0 || (Input.mousePresent && Input.GetMouseButton(0));
        }

        /// <summary>
        /// Splits the screen into vertical score multiplier sections, higher values are better score multipliers.
        /// </summary>
        /// <returns>Score multiplier value.</returns>
        private int CalcScoreMultiplier()
        {
            int outMult = 1;

            // Get player's position
            float playerScreenY = Camera.main.WorldToViewportPoint(m_playerTrans.position).y;
            int screenSection = Mathf.CeilToInt(playerScreenY * screenScoreMultDivs);

            // Calculate how far along to the maximum score multiplier the screen section the player occupies is
            outMult = Mathf.RoundToInt(screenSection * (maxScreenScoreMult / screenScoreMultDivs));

            return outMult;
        }

        /// <summary>
        /// Hides the high score text object.
        /// </summary>
        private void HideHighScoreText()
        {
            highScoreText.SetActive(false);
        }
    }
}