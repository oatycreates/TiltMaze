/**
 * File: ObstacleSpawner.cs
 * Author: Patrick Ferguson
 * Maintainer: Patrick Ferguson
 * Created: 12/11/2015
 * Copyright: (c) 2015 Patrick Ferguson, All Rights Reserved.
 * Description: Handles the spawning of falling level obstacles.
 **/

using UnityEngine;
using System.Collections;

namespace ProjectLunar
{
    [RequireComponent(typeof(Collider2D))]
    /// <summary>
    /// Handles the spawning of falling level obstacles.
    /// </summary>
    class ObstacleSpawner : MonoBehaviour
    {
        /// <summary>
        /// Time to wait before each spawn.
        /// </summary>
        public float spawnDelay = 2.5f;

        /// <summary>
        /// Number of spawn seconds to lose per second.
        /// </summary>
        public float spawnSpeedUp = 0.015f;

        /// <summary>
        /// Absolute minimum spawn delay speed.
        /// </summary>
        public float minSpawnDelay = 0.01f;

        /// <summary>
        /// Whether the spawner is currently spawning.
        /// </summary>
        public bool isSpawning = true;

        /// <summary>
        /// Obstacle to spawn a copy of.
        /// </summary>
        public GameObject obstaclePrefab = null;

        /// <summary>
        /// Current spawn delay.
        /// </summary>
        private float m_currSpawnDelay = -1.0f;

        /// <summary>
        /// Holder for the obstacles.
        /// </summary>
        private static GameObject ms_holder = null;

        // Cached references
        private Transform m_trans = null;
        private Collider2D m_col = null;

        /// <summary>
        /// Called while objects are being initialised.
        /// </summary>
        void Awake()
        {
            // Cache variables
            m_trans = transform;
            m_col = GetComponent<Collider2D>();

            // Reset variables
            m_currSpawnDelay = spawnDelay;
        }

        /// <summary>
        /// Called once all objects have been created and initialised.
        /// </summary>
        void Start()
        {
            if (ms_holder == null)
            {
                ms_holder = new GameObject();
                ms_holder.name = "ObstacleHolder";
            }
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
            if (isSpawning)
            {
                // Speed up the spawn
                spawnDelay -= spawnSpeedUp * Time.deltaTime;
                spawnDelay = Mathf.Max(spawnDelay, minSpawnDelay);

                // Count down the timer
                if (m_currSpawnDelay > 0)
                {
                    m_currSpawnDelay -= Time.deltaTime;
                }

                if (m_currSpawnDelay <= 0)
                {
                    // Time to spawn
                    SpawnObstacle();
                }
            }
        }

        /// <summary>
        /// Spawns an obstacle to fall towards the player.
        /// </summary>
        private void SpawnObstacle()
        {
            // Calculate random spawn offset within XY bounds
            Vector2 boundsExtents = m_col.bounds.extents;
            float xOffset = Random.Range(-boundsExtents.x, boundsExtents.x);
            float yOffset = Random.Range(-boundsExtents.y, boundsExtents.y);

            // Spawn the prefab
            GameObject obj = 
                GameObject.Instantiate(obstaclePrefab, m_trans.position + new Vector3(xOffset, yOffset, 0), m_trans.rotation) as GameObject;
            obj.transform.parent = ms_holder.transform;

            // Reset the spawn delay
            m_currSpawnDelay = spawnDelay;
        }
    }
}