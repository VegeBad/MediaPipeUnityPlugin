using System;
using System.Collections.Generic;
using UnityEngine;

namespace EugeneC.Utilities
{
	public abstract class GenericFsm<T, W> : MonoBehaviour
		where T : GenericFsm<T, W>
		where W : Enum
	{
		public abstract class State : MonoBehaviour
		{
			public abstract W StateEnum { get; }

			protected T StateMachine;

			protected T Parent
			{
				get
				{
					if (StateMachine == null)
						StateMachine = GetComponentInParent<T>();
					return StateMachine;
				}
			}

			public virtual void Init(T stateMachine)
			{
				StateMachine = stateMachine;
			}

			public virtual void OnEnter()
			{
			}

			public virtual void OnExit()
			{
			}

			public virtual void Run()
			{
			}

			public virtual bool CanTransitionTo(W newStateID)
			{
				return true;
			}
		}

		public State currentState;
		[SerializeField] private State previousState;
		private Dictionary<W, State> _allStates = new Dictionary<W, State>();

		public Animator characterAnimator;

		private void Awake()
		{
			if (characterAnimator == null)
				characterAnimator = GetComponentInChildren<Animator>();

			foreach (var state in GetComponentsInChildren<State>())
			{
				_allStates.Add(state.StateEnum, state);
				state.Init((T)this);
			}

			currentState?.OnEnter();
		}

		protected virtual void Update()
		{
			currentState?.Run();
		}

		public void ChangeState(W newStateID)
		{
			var newState = _allStates[newStateID];

			if (newState == currentState) return;
			else if (currentState != null && !currentState.CanTransitionTo(newStateID)) return;
			else
			{
				currentState?.OnExit();
				previousState = currentState;
				currentState = newState;
				currentState.OnEnter();
			}
		}

		public void BacktoPreviousState()
		{
			if (previousState != null) ChangeState(previousState.StateEnum);
		}
	}
}