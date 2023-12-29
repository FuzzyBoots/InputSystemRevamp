using System;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using Game.Scripts.UI;
using System.Globalization;
using Unity.VisualScripting;

namespace Game.Scripts.LiveObjects
{
    public class Forklift : MonoBehaviour
    {
        [SerializeField]
        private GameObject _lift, _steeringWheel, _leftWheel, _rightWheel, _rearWheels;
        [SerializeField]
        private Vector3 _liftLowerLimit, _liftUpperLimit;
        [SerializeField]
        private float _speed = 5f, _liftSpeed = 1f;
        [SerializeField]
        private CinemachineVirtualCamera _forkliftCam;
        [SerializeField]
        private GameObject _driverModel;
        private bool _inDriveMode = false;
        [SerializeField]
        private InteractableZone _interactableZone;

        public static event Action onDriveModeEntered;
        public static event Action onDriveModeExited;

        private PlayerInputs _playerInput;

        [SerializeField]
        private Player.Player _player;

        private void OnEnable()
        {
            SetUpForkliftInputs();
            InteractableZone.onZoneInteractionComplete += EnterDriveMode;
        }

        private void SetUpForkliftInputs()
        {
            _playerInput = new PlayerInputs();
            _playerInput.Forklift.Escape.performed += Escape_performed;
        }

        private void Escape_performed(InputAction.CallbackContext obj)
        {
            ExitDriveMode();
        }

        private void EnterDriveMode(InteractableZone zone)
        {
            if (_inDriveMode !=true && zone.GetZoneID() == 5) //Enter ForkLift
            {
                _player.DisableWalkMode();
                _playerInput.Forklift.Enable();
                _inDriveMode = true;
                _forkliftCam.Priority = 11;
                InputAction drive = _playerInput.Forklift.Drive;
                string upKey = drive.bindings[1].ToDisplayString();
                string downKey = drive.bindings[2].ToDisplayString();
                string leftKey = drive.bindings[3].ToDisplayString();
                string rightKey = drive.bindings[4].ToDisplayString();
                string forkUp = _playerInput.Forklift.Fork.bindings[1].ToDisplayString();
                string forkDown = _playerInput.Forklift.Fork.bindings[2].ToDisplayString();
                string escapeKey = _playerInput.Forklift.Escape.bindings[0].ToDisplayString();

                string instructions = $"Turn Left/Right - {leftKey}/{rightKey}\tAccelerate / Decelerate : {upKey}/{downKey}\n" + 
                    $"Fork Up / Down: {forkUp}/{forkDown}\tEscape: {escapeKey}";
                UIManager.Instance.DisplayForkliftInstructions(true, instructions);
                onDriveModeEntered?.Invoke();
                _driverModel.SetActive(true);
                _interactableZone.CompleteTask(5);
            }
        }

        private void ExitDriveMode()
        {
            _inDriveMode = false;
            _forkliftCam.Priority = 9;            
            _driverModel.SetActive(false);
            onDriveModeExited?.Invoke();
            _playerInput.Forklift.Disable();
            _player.ActivateWalkMode();
        }

        private void Update()
        {
            if (_inDriveMode == true)
            {
                LiftControls();
                CalcutateMovement();
            }

        }

        private void CalcutateMovement()
        {
            Vector2 movement = _playerInput.Forklift.Drive.ReadValue<Vector2>();
            float h = movement.x;
            float v = movement.y;
            var direction = new Vector3(0, 0, v);
            var velocity = direction * _speed;

            transform.Translate(velocity * Time.deltaTime);

            if (Mathf.Abs(v) > 0)
            {
                var tempRot = transform.rotation.eulerAngles;
                tempRot.y += h * _speed / 2;
                transform.rotation = Quaternion.Euler(tempRot);
            }
        }

        private void LiftControls()
        {
            float forkValue = _playerInput.Forklift.Fork.ReadValue<float>();
            if (forkValue > 0)
            {
                LiftUpRoutine();
            }
            else if (forkValue < 0)
            {
                LiftDownRoutine();
            }
        }

        private void LiftUpRoutine()
        {
            if (_lift.transform.localPosition.y < _liftUpperLimit.y)
            {
                Vector3 tempPos = _lift.transform.localPosition;
                tempPos.y += Time.deltaTime * _liftSpeed;
                _lift.transform.localPosition = new Vector3(tempPos.x, tempPos.y, tempPos.z);
            }
            else if (_lift.transform.localPosition.y >= _liftUpperLimit.y)
                _lift.transform.localPosition = _liftUpperLimit;
        }

        private void LiftDownRoutine()
        {
            if (_lift.transform.localPosition.y > _liftLowerLimit.y)
            {
                Vector3 tempPos = _lift.transform.localPosition;
                tempPos.y -= Time.deltaTime * _liftSpeed;
                _lift.transform.localPosition = new Vector3(tempPos.x, tempPos.y, tempPos.z);
            }
            else if (_lift.transform.localPosition.y <= _liftUpperLimit.y)
                _lift.transform.localPosition = _liftLowerLimit;
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= EnterDriveMode;
        }

    }
}