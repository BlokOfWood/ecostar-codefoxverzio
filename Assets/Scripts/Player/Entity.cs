using UnityEngine;

[CreateAssetMenu(menuName ="New Entity")]
public class Entity : ScriptableObject
{
    [Tooltip("The amount of health points the entity has.")]
    public int Health;
    [Tooltip("The amount of force applied when the entity jumps.")]
    public float JumpForce;
    [Tooltip("The movement speed of the entity.")]
    public float MovementSpeed;
    [Tooltip("Is the player dead ? ")]
    public bool IsDead;
    [Tooltip("The amount of the damage the Entity can deal. Not used for player.")]
    public int Damage = -1;
    [Tooltip("The amount of cooldown between attacks. Not used for player.")]
    public float AttackCooldown = -1;
}