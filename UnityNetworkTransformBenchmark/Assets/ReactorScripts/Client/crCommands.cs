using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using KS.Reactor.Client.Unity;
using KS.Reactor.Client;
using KS.Reactor;

namespace KS.Benchmark.Reactor.Client
{
    /// <summary>
    /// Sends commands to start/stop motion and change the number of objects in response to UI events.
    /// </summary>
    public class crCommands : ksRoomScript
    {
        private bool m_frozen;

        // Called after all other scripts/entities are attached/spawned.
        public override void Initialize()
        {
            SetEditingObjects(false);
            HUD.Get().EditButton.onClick.AddListener(EditObjects);
            HUD.Get().CancelEditButton.onClick.AddListener(CancelEditObjects);
            HUD.Get().ConfirmEditButton.onClick.AddListener(SubmitObjects);
            HUD.Get().ObjectsInput.onSubmit.AddListener(SubmitObjects);

            HUD.Get().MovementToggle.onValueChanged.AddListener(ChangeMovement);
            m_frozen = Time.TimeScale < .5f;
            HUD.Get().MovementToggle.isOn = !m_frozen;
        }
        
        // Called every frame.
        private void Update()
        {
            bool frozen = Time.TimeScale < .5f;
            if (frozen != m_frozen)
            {
                m_frozen = frozen;
                HUD.Get().MovementToggle.SetIsOnWithoutNotify(!frozen);
            }
        }

        private void ChangeMovement(bool movement)
        {
            Room.CallRPC(RPC.FREEZE, !movement);
        }

        private void SetEditingObjects(bool editing)
        {
            HUD.Get().ViewingObjects.SetActive(!editing);
            HUD.Get().EditingObjects.SetActive(editing);
        }

        private void EditObjects()
        {
            SetEditingObjects(true);
            HUD.Get().ObjectsInput.text = Room.DynamicEntities.Count.ToString();
            HUD.Get().ObjectsInput.Select();
        }

        private void CancelEditObjects()
        {
            SetEditingObjects(false);
        }

        private void SubmitObjects()
        {
            SubmitObjects(HUD.Get().ObjectsInput.text);
        }

        private void SubmitObjects(string input)
        {
            SetEditingObjects(false);
            uint value;
            if (uint.TryParse(input, out value))
            {
                Room.CallRPC(RPC.OBJECT_COUNT, value);
            }
        }
    }
}