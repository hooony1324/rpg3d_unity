using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Entity entity;

    private void Start()
    {
        entity = GetComponent<Entity>();
        //entity.SkillSystem.onSkillTargetSelectionCompleted += ReserveSkill;

        //MouseController.Instance.onLeftClicked += SelectTarget;
    }
    private void OnEnable()
    => MouseController.Instance.onRightClicked += MoveToPosition;

    private void OnDisable()
        => MouseController.Instance.onRightClicked -= MoveToPosition;

    //private void OnDestroy()
    //=> MouseController.Instance.onLeftClicked -= SelectTarget;

    private void MoveToPosition(Vector2 mousePosition)
    {
        var ray = Camera.main.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, LayerMask.GetMask("Ground")))
        {
            entity.Movement.Destination = hitInfo.point;
            //entity.SkillSystem.CancelReservedSkill();
        }
    }

    private void Update()
    {
        if ( Input.GetKeyDown(KeyCode.D))
        {
            GetComponent<EntityMovement>().Roll(4.0f);
        }
    }
}
