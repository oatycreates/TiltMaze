/**
 * File: PlayerInputManager.cs
 * Author: Patrick Ferguson
 * Maintainer: Patrick Ferguson
 * Created: 12/11/2015
 * Copyright: (c) 2015 Patrick Ferguson, All Rights Reserved.
 * Description: Handles the input of the player object.
 **/

using UnityEngine;
using System.Collections;

namespace ProjectLunar
{
    [RequireComponent(typeof(Rigidbody2D))]
    /// <summary>
    /// Handles the input of the player object.
    /// </summary>
    class PlayerInputManager : MonoBehaviour
    {
        /// <summary>
        /// Thrust velocity per tick when holding down the button.
        /// </summary>
        public float thrustVel = 15.0f;

        /// <summary>
        /// Rate to slerp the player's rotation at per second, instead of instantly snapping.
        /// </summary>
        public float rotSlerpRate = 15.0f;

        /// <summary>
        /// Particles to enable when thrusting.
        /// </summary>
        public ParticleSystem thrustParticles = null;

        /// <summary>
        /// Whether the user is attempting to thrust, using a single finger.
        /// </summary>
        private bool m_isThrusting = false;

        /// <summary>
        /// Single touch screen position in pixel coordinates.
        /// </summary>
        private Vector2 m_thrustPos = Vector2.zero;

        // Cached references
        private Transform m_trans = null;
        private Rigidbody2D m_rb2D = null;
        private GameManager m_gameManager = null;

        /// <summary>
        /// Called while objects are being initialised.
        /// </summary>
        void Awake()
        {
            // Cache variables
            m_trans = transform;
            m_rb2D = GetComponent<Rigidbody2D>();
            m_gameManager = GameObject.FindObjectOfType<GameManager>();
        }

        /// <summary>
        /// Called once all objects have been created and initialised.
        /// </summary>
        void Start()
        {

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
        /// Called once per physics increment, Time.deltaTime is fixed step here.
        /// </summary>
        void FixedUpdate()
        {
            PhysicsTick();
        }

        /// <summary>
        /// Called when a collision occurs.
        /// </summary>
        /// <param name="a_collision">Collision information.</param>
        void OnCollisionEnter2D(Collision2D a_collision)
        {
            /*if (a_collision.gameObject.CompareTag("Obstacle"))
            {
                // Only accept collisions from below
                if (Vector2.Dot(a_collision.contacts[0].normal, Vector2.down) > 0.1f)
                {
                    // Hit the obstacle
                    m_gameManager.ChangeToState(GameManager.EGameState.GAMEOVER);
                }
            }*/
        }

        /// <summary>
        /// Handles input sampling.
        /// </summary>
        private void InputTick()
        {
            // Reset input state
            m_isThrusting = false;
            m_thrustPos = Vector2.zero;

            // Only process input if in game state
            if (m_gameManager.GetCurrentState() == GameManager.EGameState.PLAY)
            {
                // Use click as first finger touch
                if (Input.mousePresent)
                {
                    // Player thrust movement
                    m_isThrusting = m_isThrusting || Input.GetMouseButton(0);

                    if (m_isThrusting)
                    {
                        m_thrustPos = Input.mousePosition;
                    }
                }

                // Process touch data
                Touch[] touches = Input.touches;
                foreach (Touch tch in touches)
                {
                    // Player thrust movement
                    if (IsTouchDown(tch))
                    {
                        // For now, just have any touch count as a 'single' touch, even if the user is holding down multiple fingers.
                        m_isThrusting = true;
                        m_thrustPos = tch.position;
                    }
                }
            }
        }

        /// <summary>
        /// Handles state updating.
        /// </summary>
        private void UpdateTick()
        {
            if (m_isThrusting)
            {
                // Convert input touch coordinates to world coordinates
                Vector3 worldSingleTouchPos = Camera.main.ScreenToWorldPoint(m_thrustPos);

                // Flatten input touch coordinates
                worldSingleTouchPos.z = 0;

                Vector3 offsetLook = (worldSingleTouchPos - m_trans.position).normalized;
                if (offsetLook.sqrMagnitude > 0)
                {
                    // Make unit length
                    offsetLook.Normalize();

                    // Face away from the touch position, slerps
                    float rotAngle = Mathf.Atan2(offsetLook.y, offsetLook.x);
                    Quaternion newRot = Quaternion.AngleAxis(rotAngle * Mathf.Rad2Deg, Vector3.forward);
                    m_trans.rotation = Quaternion.Slerp(m_trans.rotation, newRot, rotSlerpRate * Time.deltaTime);
                }
            }

            // Update thrust particle state
            if (thrustParticles != null)
            {
                thrustParticles.enableEmission = m_isThrusting;
            }
        }

        /// <summary>
        /// Handles physics value modification.
        /// </summary>
        private void PhysicsTick()
        {
            Vector2 velChange = Vector2.zero;

            if (m_isThrusting)
            {
                // Update velocity change
                velChange += (Vector2)(-m_trans.right) * thrustVel;
            }

            // Apply velocity change
            m_rb2D.AddForce(velChange, ForceMode2D.Force);
        }

        /// <summary>
        /// Whether the input touch is being held down.
        /// </summary>
        /// <param name="a_touch">Input touch data.</param>
        /// <returns>True if touch is down or held. False if not.</returns>
        private static bool IsTouchDown(Touch a_touch)
        {
            return a_touch.phase == TouchPhase.Began ||
                    a_touch.phase == TouchPhase.Stationary ||
                    a_touch.phase == TouchPhase.Moved;
        }
    }
}