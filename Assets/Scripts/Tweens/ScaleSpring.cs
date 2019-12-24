using UnityEngine;

namespace Assets.Tweens.Scripts.Common.Tweens
{
    /// <summary>
    /// Change object scale over time
    /// </summary>
    public class ScaleSpring : TweenBase
    {
        public float From;
        public float To;
        public bool X, Y, Z;
        public float Dumping;

        private float _amplitude = 1;

        public void Update()
        {
            _amplitude = Mathf.Max(0, _amplitude - Dumping * Time.deltaTime);

            var localScale = transform.localScale;
            var value = From + (To - From) * Sin() * _amplitude;

            if (X) localScale.x = Mathf.Sign(localScale.x) * value;
            if (Y) localScale.y = Mathf.Sign(localScale.y) * value;
            if (Z) localScale.z = Mathf.Sign(localScale.z) * value;

            transform.localScale = localScale;
     
            if (_amplitude <= 0)
            {
                enabled = false;
            }
            Reset();
        }

        public override void OnEnable()
        {
            base.OnEnable();
            Reset();
        }

        public void OnDisable()
        {
            transform.localScale = From * new Vector3(Mathf.Sign(transform.localScale.x), Mathf.Sign(transform.localScale.y), Mathf.Sign(transform.localScale.z));
        }

        public void Reset()
        {
            _amplitude = 1;
        }
    }
}