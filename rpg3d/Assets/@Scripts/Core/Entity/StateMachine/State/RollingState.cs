using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingState : State<Entity>
{
    private PlayerController playerController;

    protected override void Setup()
    {
        playerController = Entity.GetComponent<PlayerController>();
    }

    public override void Enter()
    {
        if (playerController)
            playerController.enabled = false;
    }

    public override void Exit()
    {
        if (playerController)
            playerController.enabled = true;
    }
}
