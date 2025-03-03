using System;
using Microsoft.Xna.Framework;

namespace MyIslandGame.ECS.Components
{
    /// <summary>
    /// Component that marks an entity as the player character.
    /// </summary>
    public class PlayerComponent : Component
    {
        /// <summary>
        /// Gets or sets the player's movement speed.
        /// </summary>
        public float MoveSpeed { get; set; }

        /// <summary>
        /// Gets or sets the player's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the current player state.
        /// </summary>
        public PlayerState State { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerComponent"/> class.
        /// </summary>
        /// <param name="name">The player's name.</param>
        /// <param name="moveSpeed">The player's movement speed.</param>
        public PlayerComponent(string name = "Player", float moveSpeed = 200f)
        {
            Name = name;
            MoveSpeed = moveSpeed;
            State = PlayerState.Idle;
        }
    }

    /// <summary>
    /// Enumeration of player states.
    /// </summary>
    public enum PlayerState
    {
        /// <summary>
        /// Player is idle.
        /// </summary>
        Idle,

        /// <summary>
        /// Player is moving.
        /// </summary>
        Moving,

        /// <summary>
        /// Player is gathering resources.
        /// </summary>
        Gathering,

        /// <summary>
        /// Player is using an item.
        /// </summary>
        UsingItem
    }
}
