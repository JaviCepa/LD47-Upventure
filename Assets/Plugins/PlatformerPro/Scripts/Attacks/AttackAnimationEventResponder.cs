using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlatformerPro
{

    /// <summary>
    /// Add this to the character animator used to animate attacks in order to listen to HitBoxStart and hitBoxEnd events and send them to character hit box.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class AttackAnimationEventResponder : PlatformerProMonoBehaviour
    {
        override public string Header => "Add this to the character animator used to animate attacks in order to listen to HitBoxStart and hitBoxEnd events and send them to character hit box.";

        /// <summary>
        /// Layer of the animtor controller used for attacks, often 0.
        /// </summary>
        public int animationLayer = 0;
        
        private Character character;
        private Animator myAnimator;

        void Start()
        {
            myAnimator = GetComponent<Animator>();
            character = GetComponentInParent<Character>();
            if (character == null)
            {
                Debug.LogWarning("AttackAnimationEventResponder is expected to be the child of a component with a Character attached to it.");
            }
            else
            {
                foreach (BasicAttacks attack in character.GetComponents<BasicAttacks>())
                {
                    attack.AttackAnimator = myAnimator;
                    attack.AttackAnimatorLayer = animationLayer;
                }
            }
        }

        /// <summary>
        /// Listen for HitBoxStart Animation Event
        /// </summary>
        public void HitBoxStart()
        {
            if (character.ActiveMovement is BasicAttacks attack)
            {
                attack.EnableHitBox();
            }
            else
            {
                Debug.Log("Got a HitBoxStart message but the active movement was not an attack!");
            }
        }
        
        /// <summary>
        /// Listen for HitBoxEnd Animation Event
        /// </summary>
        public void HitBoxEnd()
        {
            if (character.ActiveMovement is BasicAttacks attack)
            {
                attack.DisableHitBox();
            }
            else
            {
                Debug.Log("Got a HitBoxEnd message but the active movement was not an attack!");
            }
        }
    }
}