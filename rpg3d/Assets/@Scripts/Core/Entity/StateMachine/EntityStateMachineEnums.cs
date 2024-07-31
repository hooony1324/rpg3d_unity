public enum EntityStateCommand
{
    ToDefaultState,
    ToCastingSkillState,
    ToChargingSkillState,
    ToInSkillPrecedingActionState,
    ToInSkillActionState,
    ToStunningState,
    ToSleepingState
}

public enum EntityStateMessage { UsingSkill }