using UnityEngine;

namespace BeachRunner.Core
{
    public static class RunnerSpeedRuntime
    {
        public static float CurrentSpeed { get; private set; }

        private static float _baseSpeed = 8f;
        private static float _maxSpeed = 20f;
        private static float _acceleration = 0.025f;

        public static void Configure(float baseSpeed, float maxSpeed, float acceleration)
        {
            _baseSpeed = baseSpeed;
            _maxSpeed = maxSpeed;
            _acceleration = acceleration;
            ResetRuntime();
        }

        public static void Tick()
        {
            CurrentSpeed = Mathf.Min(_maxSpeed, CurrentSpeed + _acceleration * Time.deltaTime * 60f);
        }

        public static void AddTemporaryBoost(float amount)
        {
            CurrentSpeed = Mathf.Min(_maxSpeed + 5f, CurrentSpeed + amount);
        }

        public static void ResetRuntime()
        {
            CurrentSpeed = _baseSpeed;
        }
    }
}
