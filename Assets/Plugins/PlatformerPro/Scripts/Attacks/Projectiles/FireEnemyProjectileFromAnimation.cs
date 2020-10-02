using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlatformerPro
{ 

    public class FireEnemyProjectileFromAnimation : PlatformerProMonoBehaviour
    {
        public EnemyMovement_SimpleShoot shootMovement;

        private void Start()
        {
            if (shootMovement == null)
            {
                shootMovement = GetComponentInParent<EnemyMovement_SimpleShoot>();
            }
        }

        /// <summary>
        /// Fired a previously prepared projectile on associated movement.
        /// </summary>
        virtual public void FirePreparedProjectile()
        {
            shootMovement.FirePreparedProjectile();
        }
    }
}