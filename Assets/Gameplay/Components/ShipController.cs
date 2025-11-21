using UnityEngine;
using Gameplay.Interfaces;

namespace Gameplay.Components
{
    /// <summary>
    /// Base controller for all ship types.
    /// </summary>
    public abstract class ShipController : MonoBehaviour
    {
        [Header("Ship Configuration")]
        [SerializeField] protected ShipType shipType;
        [SerializeField] protected float maxSpeed = 20f;
        [SerializeField] protected float acceleration = 5f;
        [SerializeField] protected float turnSpeed = 90f;
        [SerializeField] protected float health = 100f;

        [Header("State")]
        [SerializeField] protected bool isPlayerControlled = false;
        [SerializeField] protected Vector3 velocity;
        [SerializeField] protected Vector3 angularVelocity;

        protected Rigidbody rigidbody;
        protected bool isActive = true;

        // Properties
        public ShipType ShipType => shipType;
        public float MaxSpeed => maxSpeed;
        public float CurrentSpeed => rigidbody != null ? rigidbody.linearVelocity.magnitude : 0f;
        public float Health => health;
        public bool IsPlayerControlled => isPlayerControlled;
        public bool IsActive => isActive;
        public Vector3 Velocity => velocity;
        public Vector3 AngularVelocity => angularVelocity;

        // Events
        public event System.Action<ShipController> OnDestroyed;
        public event System.Action<ShipController> OnDisabled;
        public event System.Action<float> OnHealthChanged;

        protected virtual void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = gameObject.AddComponent<Rigidbody>();
            }

            ConfigureRigidbody();
        }

        protected virtual void ConfigureRigidbody()
        {
            rigidbody.mass = 1f;
            rigidbody.linearDamping = 0.5f;
            rigidbody.angularDamping = 2f;
            rigidbody.useGravity = false;
            rigidbody.isKinematic = false;
        }

        protected virtual void Update()
        {
            if (!isActive) return;

            UpdateShip(Time.deltaTime);
        }

        protected virtual void FixedUpdate()
        {
            if (!isActive) return;

            PhysicsUpdate(Time.fixedDeltaTime);
        }

        protected virtual void UpdateShip(float deltaTime)
        {
            // Override in derived classes
        }

        protected virtual void PhysicsUpdate(float deltaTime)
        {
            // Override in derived classes for physics-based movement
        }

        public virtual void Initialize()
        {
            isActive = true;
            health = 100f;
        }

        public virtual void SetPlayerControlled(bool playerControlled)
        {
            isPlayerControlled = playerControlled;
        }

        public virtual void TakeDamage(float damage)
        {
            health = Mathf.Max(0f, health - damage);
            OnHealthChanged?.Invoke(health);

            if (health <= 0f)
            {
                Destroy();
            }
        }

        public virtual void Destroy()
        {
            if (!isActive) return;

            isActive = false;
            OnDestroyed?.Invoke(this);

            // Play destruction effects, spawn debris, etc.
            // For now, just disable the object
            gameObject.SetActive(false);
        }

        public virtual void Disable()
        {
            if (!isActive) return;

            isActive = false;
            OnDisabled?.Invoke(this);
        }

        public virtual void Enable()
        {
            isActive = true;
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            // Handle collision damage
            float collisionSpeed = collision.relativeVelocity.magnitude;
            if (collisionSpeed > 5f)
            {
                float damage = collisionSpeed * 2f;
                TakeDamage(damage);
            }
        }

        // Utility methods
        public Vector3 GetForwardVelocity()
        {
            return transform.forward * Vector3.Dot(rigidbody.linearVelocity, transform.forward);
        }

        public Vector3 GetLateralVelocity()
        {
            return rigidbody.linearVelocity - GetForwardVelocity();
        }

        public void ReduceLateralVelocity(float factor)
        {
            Vector3 lateralVelocity = GetLateralVelocity();
            rigidbody.linearVelocity -= lateralVelocity * factor;
        }

        public void ApplyForce(Vector3 force, ForceMode mode = ForceMode.Force)
        {
            if (rigidbody != null)
            {
                rigidbody.AddForce(force, mode);
            }
        }

        public void ApplyTorque(Vector3 torque, ForceMode mode = ForceMode.Force)
        {
            if (rigidbody != null)
            {
                rigidbody.AddTorque(torque, mode);
            }
        }

        // Visual feedback methods
        protected virtual void SetEngineVisuals(float throttle)
        {
            // Override in derived classes to show engine effects
        }

        protected virtual void ShowDamageEffects(float damageAmount)
        {
            // Override in derived classes to show damage effects
        }
    }
}