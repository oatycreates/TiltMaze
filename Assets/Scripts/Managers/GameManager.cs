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
        /// 'Press anywhere to play' text object.
        /// </summary>
        public GameObject beginText = null;

        /// <summary>
        /// 'Game over!' text object.
        /// </summary>
        public GameObject gameOverText = null;

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
                        beginText.SetActive(true);
                        gameOverText.SetActive(false);

                        break;
                    }
                case EGameState.PLAY:
                    {
                        beginText.SetActive(false);

                        // Resume time during game
                        Time.timeScale = 1.0f;

                        break;
                    }
                case EGameState.GAMEOVER:
                    {
                        gameOverText.SetActive(true);

                        // Short wait before replaying
                        m_replayWait = 1.0f;

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
    }
}