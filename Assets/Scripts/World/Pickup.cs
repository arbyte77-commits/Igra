using UnityEngine;
using BeachRunner.Player;
using BeachRunner.Core;
using BeachRunner.Audio;

namespace BeachRunner.World
{
    public enum PickupType { Coin, StarBoost, SpeedBoost, Shield }

    public class Pickup : MonoBehaviour
    {
        [SerializeField] private PickupType type;
        [SerializeField] private int coinAmount = 1;
        [SerializeField] private float speedBoostAmount = 2f;
        [SerializeField] private float shieldDuration = 5f;
        [SerializeField] private ParticleSystem collectFx;

        public void Collect(RunnerController runner)
        {
            switch (type)
            {
                case PickupType.Coin:
                    GameManager.Instance.AddCoin(coinAmount);
                    break;
                case PickupType.StarBoost:
                case PickupType.SpeedBoost:
                    RunnerSpeedRuntime.AddTemporaryBoost(speedBoostAmount);
                    break;
                case PickupType.Shield:
                    runner.ApplyShield(shieldDuration);
                    break;
            }

            if (collectFx != null)
            {
                Instantiate(collectFx, transform.position, Quaternion.identity);
            }

            AudioBus.Instance.PlayCoin();
            Destroy(gameObject);
        }
    }
}
