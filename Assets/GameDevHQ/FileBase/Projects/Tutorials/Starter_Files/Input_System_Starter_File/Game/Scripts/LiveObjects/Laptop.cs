﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using UnityEngine.InputSystem;
using TMPro;
using Game.Scripts.UI;

namespace Game.Scripts.LiveObjects
{
    public class Laptop : MonoBehaviour
    {
        [SerializeField]
        private Slider _progressBar;
        [SerializeField]
        private int _hackTime = 5;
        private bool _hacked = false;
        [SerializeField]
        private CinemachineVirtualCamera[] _cameras;
        private int _activeCamera = 0;
        [SerializeField]
        private InteractableZone _interactableZone;

        public static event Action onHackComplete;
        public static event Action onHackEnded;

        private PlayerInputs _playerInput;

        private void OnEnable()
        {
            _playerInput = new PlayerInputs();
            _playerInput.Player.Enable();
            _playerInput.Player.Interact.performed += Interact_performed;
            _playerInput.Player.Escape.performed += Escape_performed;
            InteractableZone.onHoldStarted += InteractableZone_onHoldStarted;
            InteractableZone.onHoldEnded += InteractableZone_onHoldEnded;
        }

        private void Escape_performed(InputAction.CallbackContext obj)
        {
            if (_hacked == true)
            {
                _hacked = false;
                UIManager.Instance.DisplayLaptopInstructions(false);
                onHackEnded?.Invoke();
                ResetCameras();
            }
        }

        private void Interact_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (_hacked == true)
            {
                var previous = _activeCamera;
                _activeCamera++;


                if (_activeCamera >= _cameras.Length)
                    _activeCamera = 0;


                _cameras[_activeCamera].Priority = 11;
                _cameras[previous].Priority = 9;
            }
        }

        void ResetCameras()
        {
            foreach (var cam in _cameras)
            {
                cam.Priority = 9;
            }
        }

        private void InteractableZone_onHoldStarted(int zoneID)
        {
            if (zoneID == 3 && _hacked == false) //Hacking terminal
            {
                _progressBar.gameObject.SetActive(true);
                StartCoroutine(HackingRoutine());
                onHackComplete?.Invoke();
            }
        }

        private void InteractableZone_onHoldEnded(int zoneID)
        {
            if (zoneID == 3) //Hacking terminal
            {
                if (_hacked == true)
                    return;

                StopAllCoroutines();
                _progressBar.gameObject.SetActive(false);
                _progressBar.value = 0;
                onHackEnded?.Invoke();

            }
        }

        
        IEnumerator HackingRoutine()
        {
            while (_progressBar.value < 1)
            {
                _progressBar.value += Time.deltaTime / _hackTime;
                yield return new WaitForEndOfFrame();
            }

            //successfully hacked
            _hacked = true;
            _interactableZone.CompleteTask(3);

            //hide progress bar
            _progressBar.gameObject.SetActive(false);

            string interactKey = _playerInput.Player.Interact.bindings[0].ToDisplayString();
            string escapeKey = _playerInput.Player.Escape.bindings[0].ToDisplayString(); 
            UIManager.Instance.DisplayLaptopInstructions(true, $"Press {interactKey} to switch cameras. Press {escapeKey} to exit.");

            //enable Vcam1
            _cameras[0].Priority = 11;
        }
        
        private void OnDisable()
        {
            InteractableZone.onHoldStarted -= InteractableZone_onHoldStarted;
            InteractableZone.onHoldEnded -= InteractableZone_onHoldEnded;
        }
    }

}

