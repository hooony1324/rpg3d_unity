using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadState : State<Entity>
{
    private PlayerController playerController;
    private EntityMovement movement;
    protected override void Setup()
    {
        playerController = Entity.GetComponent<PlayerController>();
        movement = Entity.GetComponent<EntityMovement>();
    }

    public override void Enter()
    {
        if (playerController)
            playerController.enabled = false;

        if (movement)
            movement.enabled = false;
    }

    public override void Exit()
    {
        if (playerController)
            playerController.enabled = true;

        if (movement)
            movement.enabled = true;
    }
}
