using UnityEngine;
using System.Collections;

namespace PlatformerPro
{
	/// <summary>
	/// Character event arguments.
	/// </summary>
	public class RespawnEventArgs : CharacterEventArgs
	{
	
		/// <summary>
		/// At what respawn point are we respawning.
		/// </summary>
		/// <value>The player identifier.</value>
		public string RespawnPoint
		{
			get; 
			protected set;
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="PlatformerPro.RespawnEventArgs"/> class.
		/// </summary>
		/// <param name="character">Character.</param>
		/// <param name="playerId">Id of the player (0 for player 1).</param>
		/// <param name="respawnPoint">Respawn point character is spawning at.</param>
		public RespawnEventArgs(Character character, int playerId, string respawnPoint) : base (character, playerId)
		{
			RespawnPoint = respawnPoint;
		}
		
	}
}
