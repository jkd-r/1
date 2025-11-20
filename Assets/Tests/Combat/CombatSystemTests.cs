using NUnit.Framework;
using UnityEngine;
using ProtocolEMR.Core.Combat;

namespace ProtocolEMR.Tests.Combat
{
    /// <summary>
    /// Unit tests for the combat system components.
    /// </summary>
    public class CombatSystemTests
    {
        private GameObject testObject;
        private Damageable damageable;

        [SetUp]
        public void Setup()
        {
            testObject = new GameObject("TestTarget");
            damageable = testObject.AddComponent<Damageable>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.Destroy(testObject);
        }

        [Test]
        public void TestDamageableInitialization()
        {
            Assert.AreEqual(damageable.MaxHealth, damageable.CurrentHealth);
            Assert.AreEqual(1f, damageable.HealthPercentage);
            Assert.IsFalse(damageable.IsDead);
        }

        [Test]
        public void TestTakeDamage()
        {
            float initialHealth = damageable.CurrentHealth;
            damageable.TakeDamage(10f);

            Assert.AreEqual(initialHealth - 10f, damageable.CurrentHealth);
        }

        [Test]
        public void TestHealing()
        {
            damageable.TakeDamage(50f);
            damageable.Heal(25f);

            Assert.AreEqual(damageable.MaxHealth - 25f, damageable.CurrentHealth);
        }

        [Test]
        public void TestDeathCondition()
        {
            damageable.TakeDamage(damageable.MaxHealth + 1f);

            Assert.IsTrue(damageable.IsDead);
            Assert.AreEqual(0f, damageable.CurrentHealth);
        }

        [Test]
        public void TestKnockback()
        {
            Vector3 knockbackDirection = Vector3.forward;
            damageable.ApplyKnockback(100f, knockbackDirection);

            Assert.IsTrue(damageable.IsKnockedBack);
        }

        [Test]
        public void TestMultipleDamageHits()
        {
            for (int i = 0; i < 5; i++)
            {
                damageable.TakeDamage(10f);
            }

            Assert.AreEqual(damageable.MaxHealth - 50f, damageable.CurrentHealth);
        }

        [Test]
        public void TestInvulnerability()
        {
            damageable.SetInvulnerable(true);
            float healthBeforeDamage = damageable.CurrentHealth;
            damageable.TakeDamage(50f);

            Assert.AreEqual(healthBeforeDamage, damageable.CurrentHealth);
        }

        [Test]
        public void TestRevive()
        {
            damageable.TakeDamage(damageable.MaxHealth + 1f);
            Assert.IsTrue(damageable.IsDead);

            damageable.Revive(0.5f);

            Assert.IsFalse(damageable.IsDead);
            Assert.AreEqual(damageable.MaxHealth * 0.5f, damageable.CurrentHealth);
        }

        [Test]
        public void TestHealthClamp()
        {
            damageable.SetHealth(damageable.MaxHealth * 2f);
            Assert.AreEqual(damageable.MaxHealth, damageable.CurrentHealth);

            damageable.SetHealth(-10f);
            Assert.AreEqual(0f, damageable.CurrentHealth);
        }

        [Test]
        public void TestMaxHealthChange()
        {
            float newMaxHealth = damageable.MaxHealth * 2f;
            damageable.SetMaxHealth(newMaxHealth);

            Assert.AreEqual(newMaxHealth, damageable.MaxHealth);
        }

        [Test]
        public void TestHealthPercentageCalculation()
        {
            damageable.SetHealth(damageable.MaxHealth * 0.5f);
            Assert.AreEqual(0.5f, damageable.HealthPercentage, 0.01f);
        }

        [Test]
        public void TestDamageTakenEvent()
        {
            float damageReceived = 0f;
            damageable.OnDamageTaken += (damage) => damageReceived = damage;

            damageable.TakeDamage(25f);

            Assert.AreEqual(25f, damageReceived);
        }

        [Test]
        public void TestHealthChangedEvent()
        {
            float healthValue = 0f;
            damageable.OnHealthChanged += (health) => healthValue = health;

            damageable.Heal(20f);

            Assert.AreEqual(damageable.CurrentHealth, healthValue);
        }

        [Test]
        public void TestKnockbackEvent()
        {
            Vector3 recordedDirection = Vector3.zero;
            float recordedForce = 0f;
            damageable.OnKnockbackApplied += (direction, force) =>
            {
                recordedDirection = direction;
                recordedForce = force;
            };

            Vector3 knockbackDir = Vector3.forward;
            damageable.ApplyKnockback(100f, knockbackDir);

            Assert.AreEqual(knockbackDir, recordedDirection);
            Assert.AreEqual(100f, recordedForce);
        }

        [Test]
        public void TestDeathEvent()
        {
            bool deathEventFired = false;
            damageable.OnDeath += () => deathEventFired = true;

            damageable.TakeDamage(damageable.MaxHealth + 1f);

            Assert.IsTrue(deathEventFired);
        }
    }
}
