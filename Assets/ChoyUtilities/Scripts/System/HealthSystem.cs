using System;

namespace EugeneC.Utilities
{
	public class HealthSystem
	{
		int _health;
		int _maxHealth;

		public HealthSystem(int hp)
		{
			_maxHealth = hp;
			_health = _maxHealth;
		}

		event Action<int> OnHealthChanged;
		public void SubOnHealthChanged(Action<int> sub) => OnHealthChanged += sub;
		public void UnsubOnHealthChanged(Action<int> unsub) => OnHealthChanged -= unsub;

		event Action<int> OnHealthMaxChanged;
		public void SubOnHealthMaxChanged(Action<int> sub) => OnHealthMaxChanged += sub;
		public void UnsubOnHealthMaxChanged(Action<int> unsub) => OnHealthMaxChanged -= unsub;

		event EventHandler OnDamaged;
		public void SubOnDamaged(EventHandler sub) => OnDamaged += sub;
		public void UnsubOnDamaged(EventHandler unsub) => OnDamaged -= unsub;

		event EventHandler OnHealed;
		public void SubOnHealed(EventHandler sub) => OnHealed += sub;
		public void UnsubOnHealed(EventHandler unsub) => OnHealed -= unsub;

		event EventHandler OnDead;
		public void SubOnDead(EventHandler sub) => OnDead += sub;
		public void UnsubOnDead(EventHandler unsub) => OnDead -= unsub;

		public int Health
		{
			get => _health;
		}

		public int MaxHealth
		{
			get => _maxHealth;
		}

		public float HealthPercentage
		{
			get => (float)_health / _maxHealth;
		}

		public void Damage(int damageNumber)
		{
			_health -= damageNumber;
			if (_health < 0) _health = 0;

			OnHealthChanged?.Invoke(-damageNumber);
			OnDamaged?.Invoke(this, EventArgs.Empty);

			if (_health <= 0) Die();
		}

		public void Die() => OnDead?.Invoke(this, EventArgs.Empty);

		public bool IsDead
		{
			get => _health <= 0;
		}

		public void Heal(int healAmount)
		{
			_health += healAmount;
			if (_health > _maxHealth) _health = _maxHealth;
			OnHealthChanged?.Invoke(healAmount);
			OnHealed?.Invoke(this, EventArgs.Empty);
		}

		public void OnHealthMaxChangedEvent(int hp)
		{
			_maxHealth += hp;
			if (_maxHealth < 0) _maxHealth = 1;
			OnHealthMaxChanged?.Invoke(hp);
		}
	}

	public interface IDamage
	{
		void Damaged(int tagTeam, int dmg);
	}

	public interface IHeal
	{
		bool CanInteract { get; }
		void Healed(int heal);
	}
}