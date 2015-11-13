/**
 * File: ObstacleFall.cs
 * Author: Patrick Ferguson
 * Maintainer: Patrick Ferguson
 * Created: 12/11/2015
 * Copyright: (c) 2015 Patrick Ferguson, All Rights Reserved.
 * Description: Handles the falling of level obstacles.
 **/

using UnityEngine;
using System.Collections;

namespace ProjectLunar
{
    [RequireComponent(typeof(Rigidbody2D))]
    /// <summary>
    /// Handles the falling of level obstacles.
    /// </summary>
    class ObstacleFall : MonoBehaviour
    {
        /// <summary>
        /// Constant fall vector. Defaults to gravity.
        /// </summary>
        public Vector2 fallAccel = Physics2D.gravity;

        /// <summary>
        /// Obstacles below this height will be removed.
        /// </summary>
        public float killY = -6.5f;

        // Cached references
        private Transform m_trans = null;
        private Rigidbody2D m_rb2D = null;

        /// <summary>
        /// Called while objects are being initialised.
        /// </summary>
        void Awake()
        {
            // Cache variables
            m_trans = transform;
            m_rb2D = GetComponent<Rigidbody2D>();
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
            // If off screen (bottom)
            if (m_trans.position.y < killY)
            {
                // Destroy
                GameObject.Destroy(gameObject);
            }
        }

        /// <summary>
        /// Handles physics value modification.
        /// </summary>
        private void PhysicsTick()
        {
            Vector2 velChange = Vector2.zero;

            // Update velocity change
            velChange += fallAccel;

            // Apply velocity change ignoring mass
            m_rb2D.AddForce(velChange, ForceMode2D.Force);
        }
    }
}