using UnityEngine;

namespace Assets.Tweens.Scripts.Common.Tweens
{
    /// <summary>
    /// Axis enum
    /// </summary>
    public enum Axis
    {
        X,
        Y,
        Z
    }

    /// <summary>
    /// Base abstract class for all object transformations over time (moving, rotation, scaling)
    /// </summary>
    public abstract class TweenBase : MonoBehaviour
    {
        /// <summary>
        /// Tween speed, any value including negative
        /// </summary>
        public float Speed;

        /// <summary>
        /// Tween period, 0 - PI
        /// </summary>
        public float Period;

        /// <summary>
        /// Use random period
        /// </summary>
        public bool RandomPeriod;

        /// <summary>
        /// Use random speed
        /// </summary>
        public bool RandomSpeed;

        private float _time;

        public virtual void OnEnable()
        {
            _time = Time.time;

            if (RandomPeriod)
            {
                Period = Random.Range(0, 360);
            }

            if (RandomSpeed)
            {
                Speed *= Random.Range(0, 100) / 100f;
            }
        }

        protected float Sin()
        {
            return (Mathf.Sin(Speed * (Time.time - _time) + Period * Mathf.Deg2Rad) + 1) / 2;
        }
    }
}