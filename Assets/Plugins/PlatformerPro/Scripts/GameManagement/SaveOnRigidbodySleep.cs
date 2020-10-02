using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlatformerPro
{
    public class SaveOnRigidbodySleep : PlatformerProMonoBehaviour
    {
        public PersistableObject target;

        private float velocityThreshold;
        private const float stillTime = 0.5f;
        private float stillTimer;
        private bool isSleeping;
        private Rigidbody2D myRigidbody;

        void Start()
        {
            myRigidbody = GetComponent<Rigidbody2D>();
            velocityThreshold = Physics2D.linearSleepTolerance;
            if (target == null) target = GetComponentInChildren<PersistableObject>();
            isSleeping = myRigidbody.IsSleeping();
        }
        
        void Update()
        {
            if (PlatformerProGameManager.Instance.GamePhase != GamePhase.READY || isSleeping) return;
            if (IsSleeping() && target != null && target.isActiveAndEnabled)
            {
                isSleeping = true;
                target.SetPersistenceState(true);
            } 
            if (myRigidbody.velocity.magnitude < velocityThreshold)
            {
                stillTimer += TimeManager.FrameTime;
            }
            else
            {
                stillTimer = 0;
            }
        }

        private bool IsSleeping()
        {
            if (myRigidbody.IsSleeping()) return true;
            // Actual rigidbody2D sleep is very depednenct on phsyics settings, and often never triggers, here we use our own 
            if (stillTimer >= stillTime) return true;
            return false;
        }

        override public string Header
        {
            get
            {
                return "When attached to a Rigidbody this will update a PersistableObjects state when the rigidbody sleeps.";
            }
        }
    }
}