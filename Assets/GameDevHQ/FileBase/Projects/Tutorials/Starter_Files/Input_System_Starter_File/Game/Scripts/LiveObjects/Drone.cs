using System;
using UnityEngine;
using Cinemachine;
using Game.Scripts.UI;
using Unity.VisualScripting;
using UnityEngine.InputSystem;

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
        private Player.Player _player;

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

        public void EscapeFlightMode()
        {
            _inFlightMode = false;
            OnExitFlightmode?.Invoke();
            ExitFlightMode();
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
                InputAction move = _playerInput.Drone.Move;
                string upKey = move.bindings[1].ToDisplayString();
                string downKey = move.bindings[2].ToDisplayString();
                string bankLKey = move.bindings[3].ToDisplayString();
                string bankRKey = move.bindings[4].ToDisplayString();
                string bankFKey = move.bindings[5].ToDisplayString();
                string bankBKey = move.bindings[6].ToDisplayString();
                string rotLKey = _playerInput.Drone.Rotation.bindings[1].ToDisplayString();
                string rotRKey = _playerInput.Drone.Rotation.bindings[2].ToDisplayString();
                string escapeKey = _playerInput.Drone.Escape.bindings[0].ToDisplayString();

                string instructions = $"Bank Left/Right - {bankLKey}/{bankRKey}\nTilt Forward/Back : {bankFKey}/{bankBKey}\n" + 
                    $"Up/Down: {upKey}/{downKey}\nRotate Left/Right: {rotLKey}/{rotRKey}\nEscape: {escapeKey}";
                UIManager.Instance.DisplayDroneInstructions(true, instructions);
                _interactableZone.CompleteTask(4);
            }
        }

        private void ExitFlightMode()
        {
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
