/**
 * File: PlayerInputManager.cs
 * Author: Patrick Ferguson
 * Maintainer: Patrick Ferguson
 * Created: 12/11/2015
 * Copyright: (c) 2015 Team Gyro Mazr, All Rights Reserved.
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
        /// Player rotation slerp speed per second.
        /// </summary>
        public float rotSlerpFactor = 15.0f;

        /// <summary>
        /// Gyro device filter factor.
        /// </summary>
        public float gyroFilterFactor = 0.95f;

        /// <summary>
        /// How quickly to lerp the camera's position.
        /// </summary>
        public float camPosLerpFactor = 10.0f;

        /// <summary>
        /// Camera reference.
        /// </summary>
        public GameObject sceneCam = null;

        /// <summary>
        /// Device gyroscope rotation.
        /// </summary>
        private Quaternion m_deviceRot = Quaternion.identity;

        // Cached references
        private Transform m_trans = null;
        private Rigidbody2D m_rb2D = null;
        private GameManager m_gameManager = null;
        private Gyroscope m_gyro = null;
        private Transform m_sceneCamTrans = null;

        /// <summary>
        /// Called while objects are being initialised.
        /// </summary>
        void Awake()
        {
            // Cache variables
            m_trans = transform;
            m_rb2D = GetComponent<Rigidbody2D>();
            m_gameManager = GameObject.FindObjectOfType<GameManager>();
            m_sceneCamTrans = sceneCam.transform;

            if (SystemInfo.supportsGyroscope)
            {
                // Enable and configure the gyro if supported
                m_gyro = Input.gyro;
                m_gyro.enabled = true;

                Input.compensateSensors = true;

                // Set initial device rotation value
                m_deviceRot = m_gyro.attitude;
            }
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

            // Only process input if in game state
            if (m_gameManager.GetCurrentState() == GameManager.EGameState.PLAY)
            {
                // Capture gyro state
                if (m_gyro != null)
                {
                    m_deviceRot = Quaternion.Slerp(m_deviceRot, m_gyro.attitude, gyroFilterFactor * Time.deltaTime);
                }
            }
        }

        /// <summary>
        /// Handles state updating.
        /// </summary>
        private void UpdateTick()
        {
            float offsetRotation = m_deviceRot.eulerAngles.z;

            Debug.Log("Rot: " + m_deviceRot.eulerAngles.z);

            // Update camera transform
            float origZ = m_sceneCamTrans.position.z;
            m_sceneCamTrans.position = Vector3.Lerp(m_sceneCamTrans.position, m_trans.position, camPosLerpFactor * Time.deltaTime);
            m_sceneCamTrans.position = new Vector3(m_sceneCamTrans.position.x, m_sceneCamTrans.position.y, origZ); // Exclude Z
            m_sceneCamTrans.rotation = Quaternion.AngleAxis(m_deviceRot.eulerAngles.z, Vector3.forward);

            // Slerp player rotation to match camera
            m_trans.rotation = Quaternion.Slerp(m_trans.rotation, m_sceneCamTrans.rotation, rotSlerpFactor);
        }

        /// <summary>
        /// Handles physics value modification.
        /// </summary>
        private void PhysicsTick()
        {
            Vector2 velChange = Vector2.zero;
            
            // Only process if in game state
            if (m_gameManager.GetCurrentState() == GameManager.EGameState.PLAY)
            {
                if (m_gyro != null)
                {
                    // Apply gravity relative to rotation
                    Vector2 transDown = -m_trans.up;
                    velChange += new Vector2(transDown.x, transDown.y) * Physics2D.gravity.magnitude;

#if UNITY_EDITOR
                    // Draw player up direction
                    Debug.DrawLine(m_trans.position, m_trans.position + new Vector3(transDown.x, transDown.y, 0) * 10, Color.red);
#endif
                }
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