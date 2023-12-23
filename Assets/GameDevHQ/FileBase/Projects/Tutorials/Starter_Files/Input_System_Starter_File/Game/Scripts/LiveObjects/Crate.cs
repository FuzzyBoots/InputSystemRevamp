using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.LiveObjects
{
    public class Crate : MonoBehaviour
    {
        [SerializeField] private float _punchDelay;
        [SerializeField] private GameObject _wholeCrate, _brokenCrate;
        [SerializeField] private Rigidbody[] _pieces;
        [SerializeField] private BoxCollider _crateCollider;
        [SerializeField] private InteractableZone _interactableZone;
        private bool _isReadyToBreak = false;

        private List<Rigidbody> _breakingPieces = new List<Rigidbody>();
        private float _holdStarted = 0;
        [SerializeField] private float _holdDelay = 0.5f;

        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += InteractableZone_onZoneInteractionComplete;
            InteractableZone.onHoldStarted += InteractableZone_onHoldStarted;
            InteractableZone.onHoldEnded += InteractableZone_onHoldEnded;
        }

        private void InteractableZone_onHoldStarted(int obj)
        {
            if (_isReadyToBreak)
            {
                Debug.Log("Hold started");

                _holdStarted = Time.time;
            }
        }

        private void InteractableZone_onHoldEnded(int obj)
        {
            if (_isReadyToBreak)
            {
                Debug.Log("Hold ended");
                if (_holdStarted <= Time.time + _holdDelay)
                {
                    int parts = Random.Range(3, 6);
                    for (int i = 0; i < parts; i++)
                    {
                        BreakPart();
                    }
                }
                else
                {
                    // Otherwise, single strike
                    BreakPart();
                }
            }
        }

        private void InteractableZone_onZoneInteractionComplete(InteractableZone zone)
        {
            Debug.Log($"{_isReadyToBreak} - {_breakingPieces.Count} - {zone.GetZoneID()}");

            if (_isReadyToBreak == false && _breakingPieces.Count > 0 && zone.GetZoneID() == 6)
            {
                Debug.Log("Interaction Zone complete", this);
                _wholeCrate.SetActive(false);
                _brokenCrate.SetActive(true);
                _isReadyToBreak = true;
            }            
        }

        private void Start()
        {
            _breakingPieces.AddRange(_pieces);            
        }



        public void BreakPart()
        {
            if (_breakingPieces.Count > 0)
            {
                int rng = Random.Range(0, _breakingPieces.Count);
                _breakingPieces[rng].constraints = RigidbodyConstraints.None;
                _breakingPieces[rng].AddForce(new Vector3(1f, 1f, 1f), ForceMode.Force);
                _breakingPieces.Remove(_breakingPieces[rng]);
            } else
            {
                _isReadyToBreak = false;
                _crateCollider.enabled = false;
                _interactableZone.CompleteTask(6);
                Debug.Log("Completely Busted");
            }
        }

        IEnumerator PunchDelay()
        {
            float delayTimer = 0;
            while (delayTimer < _punchDelay)
            {
                yield return new WaitForEndOfFrame();
                delayTimer += Time.deltaTime;
            }

            _interactableZone.ResetAction(6);
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= InteractableZone_onZoneInteractionComplete;
        }
    }
}
