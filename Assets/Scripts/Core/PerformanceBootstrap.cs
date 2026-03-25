using UnityEngine;

namespace BeachRunner.Core
{
    public class PerformanceBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
        }
    }
}
