using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectType
{
    None,
    Buff,
    Debuff
}

public enum EffectRemoveDuplicateTargetOption
{
    // 이미 적용중인 Effect를 제거
    Old,
    // 새로 적용된 Effect를 제거
    New,
}

public enum EffectRunningFinishOption
{
    // Effect가 설정된 적용 횟수만큼 적용된다면 완료
    // 지속 시간이 끝나도 완료됨
    // 타격, 치료, ...
    FinishWhenApplyCompleted,
    // 지속 시간이 끝나면 완료
    // Effect가 설정된 적용 횟수만큼 적용되도, 지속 시간이 남았다면 완료가 안됨
    // Buff, Debuff, ...
    FinishWhenDurationEnded,
}