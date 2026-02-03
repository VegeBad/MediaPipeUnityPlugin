using System;
using System.Collections.Generic;
using EugeneC.Singleton;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace EugeneC.Utilities
{
// Combination of input manager and spawn manager
	public class PlayerSpawnerManager : GenericSpawnManager<PlayerSpawnerManager.PlayerTypeEnum>
	{
		public enum PlayerTypeEnum
		{
			PlayerType1
		}

		[SerializeField] InputActionAsset actionAsset;
		[SerializeField] PlayerTypeEnum playerType;
		[SerializeField] Transform defaultSpawnLocation;
		[SerializeField] int playerLimitCount = 3;
		[SerializeField] bool allowNewJoin = true;
		[SerializeField] bool allowKeyboard;

		private Dictionary<InputDevice, MultiInputSystem> _deviceRegistry = new();
		private Dictionary<MultiInputSystem, IControlBinder> _playerRegistry = new();

		private void OnEnable()
		{
			KeepSingleton(true);
			InputSystem.onDeviceChange += OnDeviceChange;
			SceneManager.sceneLoaded += OnSceneLoaded;

			PlayerSpawnController.Instance.SubOnAllowNewPlayers(OnAllowNewJoinChange);
			PlayerSpawnController.Instance.SubOnAbleControls(AllInputControl);
			PlayerSpawnController.Instance.SubOnResetGame(ResetGame);

			CheckForExistingDevices();
		}

		private void OnDisable()
		{
			InputSystem.onDeviceChange -= OnDeviceChange;
			SceneManager.sceneLoaded -= OnSceneLoaded;

			PlayerSpawnController.Instance.UnsubOnAllowNewPlayers(OnAllowNewJoinChange);
			PlayerSpawnController.Instance.UnsubOnAbleControls(AllInputControl);
			PlayerSpawnController.Instance.UnsubOnResetGame(ResetGame);
		}

		void OnAllowNewJoinChange(bool change) => allowNewJoin = change;

		void OnDeviceChange(InputDevice device, InputDeviceChange change)
		{
			if (change == InputDeviceChange.Added)
			{
				Debug.Log($"Device added: {device.displayName}");
				TryRegisterDevice(device);
			}
			else if (change == InputDeviceChange.Removed)
			{
				Debug.Log($"Device removed: {device.displayName}");
				UnregisterDevice(device);
			}
		}

		void CheckForExistingDevices()
		{
			foreach (var device in InputSystem.devices)
			{
				TryRegisterDevice(device);
			}
		}

		void TryRegisterDevice(InputDevice device)
		{
			if (allowNewJoin)
			{
				if (device is Gamepad || (device is Keyboard && allowKeyboard))
				{
					if (!_deviceRegistry.ContainsKey(device))
					{
						if (_playerRegistry.Count < playerLimitCount)
						{
							GameObject playerObj = SpawnObject(playerType, defaultSpawnLocation.position,
								Quaternion.identity);
							if (playerObj != null)
							{
								IControlBinder controlBinder = playerObj.GetComponent<IControlBinder>();

								if (controlBinder != null)
									RegisterPlayer(controlBinder, device);
								else
									Debug.LogError(
										"Spawned object does not have a component that implements IControlBinder.");
							}
						}
						else
							Debug.Log("Player limit reached. Cannot register new player.");
					}
				}
			}
		}

		void RegisterPlayer(IControlBinder playerBinder, InputDevice device)
		{
			ControlSchemeEnum controlScheme = UtilityMethods.GetDeviceType(device);
			MultiInputSystem registry = new MultiInputSystem(device, actionAsset, controlScheme);

			_deviceRegistry.Add(device, registry);
			_playerRegistry.Add(registry, playerBinder);

			registry.BindObject(playerBinder);
		}

		void UnregisterDevice(InputDevice device)
		{
			if (_deviceRegistry.TryGetValue(device, out MultiInputSystem registry))
			{
				if (_playerRegistry.TryGetValue(registry, out IControlBinder playerBinder))
				{
					registry.UnbindObject();

					MonoBehaviour binderBehaviour = playerBinder as MonoBehaviour;
					if (binderBehaviour != null)
					{
						DespawnObject(binderBehaviour.gameObject);
						_playerRegistry.Remove(registry);
						_deviceRegistry.Remove(device);

						Debug.Log(
							$"Unregistered player {playerBinder.GetType().Name} and removed device {device.displayName}");
					}
					else
						Debug.LogError($"{playerBinder.GetType().Name} is not a MonoBehaviour. Cannot despawn object.");
				}
			}
		}

		void UnregisterAll()
		{
			foreach (var registry in _playerRegistry.Keys)
				registry.UnbindObject();

			foreach (var playerBinder in _playerRegistry.Values)
			{
				MonoBehaviour binderBehaviour = playerBinder as MonoBehaviour;
				if (binderBehaviour != null)
					DespawnObject(binderBehaviour.gameObject);
			}

			_playerRegistry.Clear();
			_deviceRegistry.Clear();

			Debug.Log("All players have been unregistered.");
		}

		void ResetGame(object sender, EventArgs e)
		{
			UnregisterAll();
			CheckForExistingDevices();
		}

		void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			Debug.Log($"Scene loaded: {scene.name}");
			ReinstantiatePlayers();
		}

		void ReinstantiatePlayers()
		{
			List<MultiInputSystem> registries = new List<MultiInputSystem>(_playerRegistry.Keys);
			_playerRegistry.Clear();

			foreach (var registry in registries)
			{
				GameObject playerObj = SpawnObject(PlayerTypeEnum.PlayerType1, Vector3.zero, Quaternion.identity);
				if (playerObj != null)
				{
					IControlBinder controlBinder = playerObj.GetComponent<IControlBinder>();
					if (controlBinder != null)
					{
						registry.BindObject(controlBinder);
						_playerRegistry.Add(registry, controlBinder);

						Debug.Log(
							$"Reinstantiated player {controlBinder.GetType().Name} and bound to device {registry.Device.displayName}");
					}
					else
						Debug.LogError("Spawned object does not have a component that implements IControlBinder.");
				}
				else
					Debug.LogError("Failed to spawn player object.");
			}
		}

		void AllInputControl(bool able)
		{
			foreach (var players in spawnedObjects)
			{
				IControlBinder controlBinder = players.GetComponent<IControlBinder>();
				if (controlBinder != null)
				{
					if (able)
						controlBinder.Registry.EnableInput();
					else
						controlBinder.Registry.DisableInput();
				}
			}
		}
	}
}