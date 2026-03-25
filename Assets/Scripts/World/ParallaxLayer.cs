using UnityEngine;
using BeachRunner.Core;

namespace BeachRunner.World
{
    public class ParallaxLayer : MonoBehaviour
    {
        [SerializeField] private float speedFactor = 0.2f;
        [SerializeField] private float resetX = -40f;
        [SerializeField] private float wrapDistance = 80f;

        private void Update()
        {
            if (GameManager.Instance.State != GameState.Playing) return;

            var speed = RunnerSpeedRuntime.CurrentSpeed * speedFactor;
            transform.Translate(Vector3.left * (speed * Time.deltaTime));
            if (transform.position.x <= resetX)
            {
                transform.position += new Vector3(wrapDistance, 0f, 0f);
            }
        }
    }
}
