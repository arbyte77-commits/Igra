using UnityEngine;

namespace BeachRunner.Player
{
    public class TouchInputController : MonoBehaviour
    {
        [SerializeField] private RunnerController runner;
        [SerializeField] private float swipeThreshold = 60f;

        private Vector2 _touchStart;

        public void JumpButtonPressed() => runner.Jump();
        public void SlideButtonPressed() => runner.Slide();

        private void Update()
        {
            if (Input.touchCount == 0) return;

            var touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    _touchStart = touch.position;
                    break;
                case TouchPhase.Ended:
                    var delta = touch.position - _touchStart;
                    if (delta.magnitude < swipeThreshold) return;
                    if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    {
                        runner.ShiftLane(delta.x > 0 ? 1 : -1);
                    }
                    else
                    {
                        if (delta.y > 0) runner.Jump();
                        else runner.Slide();
                    }
                    break;
            }
        }
    }
}
