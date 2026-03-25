using System.Collections.Generic;
using UnityEngine;
using BeachRunner.Core;

namespace BeachRunner.World
{
    public class ObstacleSpawner : MonoBehaviour
    {
        [SerializeField] private List<GameObject> obstaclePrefabs;
        [SerializeField] private List<GameObject> pickupPrefabs;
        [SerializeField] private Transform[] laneSpawnPoints;
        [SerializeField] private float spawnInterval = 1.15f;
        [SerializeField] private float minInterval = 0.45f;
        [SerializeField] private float intervalDecayPerMinute = 0.12f;

        private float _timer;
        private float _runtimeInterval;

        private void Start()
        {
            _runtimeInterval = spawnInterval;
        }

        private void Update()
        {
            if (GameManager.Instance.State != GameState.Playing) return;

            _runtimeInterval = Mathf.Max(minInterval, spawnInterval - (Time.timeSinceLevelLoad / 60f) * intervalDecayPerMinute);
            _timer += Time.deltaTime;
            if (_timer < _runtimeInterval) return;
            _timer = 0f;

            SpawnPattern();
        }

        private void SpawnPattern()
        {
            var blockedLane = Random.Range(0, laneSpawnPoints.Length);
            for (var i = 0; i < laneSpawnPoints.Length; i++)
            {
                var roll = Random.value;
                if (i == blockedLane || roll > 0.65f)
                {
                    Spawn(obstaclePrefabs[Random.Range(0, obstaclePrefabs.Count)], laneSpawnPoints[i]);
                }
                else if (roll > 0.40f)
                {
                    Spawn(pickupPrefabs[Random.Range(0, pickupPrefabs.Count)], laneSpawnPoints[i]);
                }
            }
        }

        private static void Spawn(GameObject prefab, Transform point)
        {
            Instantiate(prefab, point.position, Quaternion.identity);
        }
    }
}
