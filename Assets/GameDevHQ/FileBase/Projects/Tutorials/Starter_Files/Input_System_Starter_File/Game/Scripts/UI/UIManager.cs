using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.UI
{
    public class UIManager : MonoBehaviour
    {
        #region Singleton
        private static UIManager _instance;
        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                    Debug.LogError("UI Manager is NULL.");

                return _instance;
            }
        }
        #endregion

        [SerializeField]
        private TMP_Text _interactableZoneText;
        [SerializeField]
        private TMP_Text _laptopInstructionsText;
        [SerializeField]
        private TMP_Text _droneInstructionsText;
        [SerializeField]
        private TMP_Text _forkliftInstructionsText;
        [SerializeField]
        private Image _inventoryDisplay;
        [SerializeField]
        private RawImage _droneCamView;

        private void Awake()
        {
            _instance = this;
        }

        public void DisplayInteractableZoneMessage(bool showMessage, string message = null)
        {
            _interactableZoneText.text = message;
            _interactableZoneText.enabled = showMessage;
        }

        public void DisplayLaptopInstructions(bool showMessage, string message = null)
        {
            _laptopInstructionsText.text = message;
            _laptopInstructionsText.enabled = showMessage;
        }

        public void DisplayDroneInstructions(bool showMessage, string message = null)
        {
            _droneInstructionsText.text = message;
            _droneInstructionsText.enabled = showMessage;
        }

        public void DisplayForkliftInstructions(bool showMessage, string message = null)
        {
            _forkliftInstructionsText.text = message;
            _forkliftInstructionsText.enabled = showMessage;
        }

        public void UpdateInventoryDisplay(Sprite icon)
        {            
            _inventoryDisplay.sprite = icon;
        }

        public void DroneView(bool Active)
        {
            _droneCamView.enabled = Active;
        }
    }
}

