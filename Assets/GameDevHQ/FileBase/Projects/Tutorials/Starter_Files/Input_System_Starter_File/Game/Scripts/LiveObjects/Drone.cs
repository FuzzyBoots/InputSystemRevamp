using System;
using UnityEngine;
using Cinemachine;
using Game.Scripts.UI;

namespace Game.Scripts.LiveObjects
{
    public class Drone : MonoBehaviour
    {
        private enum Tilt
        {
            NoTilt, Forward, Back, Left, Right
        }

        [SerializeField]
        private Rigidbody _rigidbody;
        [SerializeField]
        private float _speed = 5f;
        private bool _inFlightMode = false;
        [SerializeField]
        private Animator _propAnim;
        [SerializeField]
        private CinemachineVirtualCamera _droneCam;
        [SerializeField]
        private InteractableZone _interactableZone;

        [SerializeField]
        private Game.Scripts.Player.Player _player;

        private PlayerInputs _playerInput;
        

        public static event Action OnEnterFlightMode;
        public static event Action OnExitFlightmode;

        private void OnEnable()
        {
            SetUpDroneInputs();
            InteractableZone.onZoneInteractionComplete += EnterFlightMode;
        }

        private void SetUpDroneInputs()
        {
            _playerInput = new PlayerInputs();
            _playerInput.Drone.Escape.performed += Escape_performed;
        }

        private void Escape_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            EscapeFlightMode();
        }

        private void EnterFlightMode(InteractableZone zone)
        {
            if (_inFlightMode != true && zone.GetZoneID() == 4) // drone Scene
            {
                _player.DisableWalkMode();
                _playerInput.Drone.Enable();
                _propAnim.SetTrigger("StartProps");
                _droneCam.Priority = 11;
                _inFlightMode = true;
                OnEnterFlightMode?.Invoke();
                UIManager.Instance.DroneView(true);
                _interactableZone.CompleteTask(4);
            }
        }

        private void ExitFlightMode()
        {
            Debug.Log("Exiting");

            _droneCam.Priority = 9;
            _inFlightMode = false;
            UIManager.Instance.DroneView(false);
            _playerInput.Drone.Disable();
            _player.ActivateWalkMode();
        }

        private void Update()
        {
            if (_inFlightMode)
            {
                CalculateTilt();
                CalculateMovementUpdate();
            }
        }

        public void EscapeFlightMode()
        {
            _inFlightMode = false;
            OnExitFlightmode?.Invoke();
            ExitFlightMode();
        }

        private void FixedUpdate()
        {
            _rigidbody.AddForce(transform.up * (9.81f), ForceMode.Acceleration);
            if (_inFlightMode)
                CalculateMovementFixedUpdate();
        }

        private void CalculateMovementUpdate()
        {
            float rotation = _playerInput.Drone.Rotation.ReadValue<float>(); // goes from -1 to 1
            if (_playerInput.Drone.Rotation.IsPressed())
            {
                var tempRot = transform.localRotation.eulerAngles;
                tempRot.y += rotation * _speed / 3;
                transform.localRotation = Quaternion.Euler(tempRot);
            }
        }

        private void CalculateMovementFixedUpdate()
        {
            Vector3 movement = _playerInput.Drone.Move.ReadValue<Vector3>();
            if (Math.Abs(movement.y) > 0f)
            {
                _rigidbody.AddForce(movement.y * transform.up * _speed, ForceMode.Acceleration);
            }
        }

        private void CalculateTilt()
        {
            Vector3 movement = _playerInput.Drone.Move.ReadValue<Vector3>().normalized;
            transform.rotation = Quaternion.Euler(30 * movement.z, transform.localRotation.eulerAngles.y, -30 * movement.x);
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= EnterFlightMode;
        }
    }
}
