using NUnit.Framework.Internal.Commands;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Effect : IdentifiedObject
{
    private const int kInfinity = 0;

    public delegate void StartedHandler(Effect effect);
    public delegate void AppliedHandler(Effect effect, int currentApplyCount, int prevApplyCount);
    public delegate void ReleasedHandler(Effect effect);
    public delegate void StackChangedHandler(Effect effect, int currentApplyCount, int prevApplyCount);

    [SerializeField]
    private EffectType type;
    // Effect의 중복 적용 가능 여부
    [SerializeField]
    private bool isAllowDuplicate = true;
    [SerializeField]
    private EffectRemoveDuplicateTargetOption removeDuplicateTargetOption;

    [SerializeField]
    private bool isShowInUI;

    // maxLevel이 effectDatas의 Length를 초과할 수 있는지 여부
    // false면 maxLevel은 effectDatas의 Length로 고정됨
    [SerializeField]
    private bool isAllowLevelExceedDatas;
    [SerializeField]
    private int maxLevel;

    // Level별 Data
    [SerializeField]
    private EffectData[] effectDatas;
    // Level에 맞는 현재 Data
    private EffectData currentData;

    // 현재 Effect Level
    private int level;
    // 현재 쌓인 Stack
    private int currentStack = 1;
    private float currentDuration;
    private int currentApplyCount;
    private float currentApplyCycle;
    // Action의 Apply함수 실행 시도 여부
    // Apply성공 시, currentApplyCycle변수 다르게 초기화됨
    // Apply실행될 때 true가 되고 Apply가 적용되면(true를 리턴하면) false로 초기화
    private bool isApplyTried;

    // 쌓인 Stack에 따라 현재 적용된 Stack Actions
    private readonly List<EffectStackAction> appliedStackActions = new();

    public EffectType Type => type;
    public bool IsAllowDuplicate => isAllowDuplicate;
    public EffectRemoveDuplicateTargetOption RemoveDuplicateTargetOption => removeDuplicateTargetOption;

    public bool IsShowInUI => isShowInUI;

    public IReadOnlyList<EffectData> EffectDatas => effectDatas;
    public IReadOnlyList<EffectStackAction> StackActions => currentData.stackActions;

    public int MaxLevel => maxLevel;
    public int Level
    {
        get => level;
        set
        {
            Debug.Assert(value > 0 && value <= MaxLevel, $"Effect.Rank = {value} - value는 0보다 크고 MaxLevel보다 같거나 작아야합니다.");

            if (level == value)
                return;

            level = value;

            // 현재 Effect Level보다 작으면서 가장 가까운 Level인 Data를 찾아옴
            // 예를 들어, Data가 Level 1, 3, 5 이렇게 있을 때, Effect의 Level이 4일 경우,
            // Level 3의 Data를 찾아옴
            var newData = effectDatas.Last(x => x.level <= level);
            if (newData.level != currentData.level)
                currentData = newData;
        }
    }

    public bool IsMaxLevel => level == maxLevel;

    // 현재 Effect와 EffectData의 레벨 차이만큼 Bonus수치 줄 수 있음
    public int DataBonusLevel => Mathf.Max(level - currentData.level, 0);
    public float Duration => currentData.duration.GetValue(User.Stats);
    public bool IsTimeless => Mathf.Approximately(Duration, kInfinity);
    public float CurrentDuration
    {
        get => currentDuration;
        set => currentDuration = Mathf.Clamp(value, 0f, Duration);
    }
    public float RemainDuration => Mathf.Max(0f, Duration - currentDuration);

    public int MaxStack => currentData.maxStack;
    public int CurrentStack
    {
        get => currentStack;
        set
        {
            var prevStack = currentStack;
            currentStack = Mathf.Clamp(value, 1, MaxStack);

            // Stack이 쌓이면 currentDuration을 초기화하여 Effect의 지속 시간을 늘려줌
            if (currentStack >= prevStack)
                currentDuration = 0f;

            if (currentStack != prevStack)
            {
                // Action에 쌓인 Stack 수가 바뀌었다고 알려줘서, Stack에 따른 수치를 Update 할 수 있게함
                Action?.OnEffectStackChanged(this, User, Target, level, currentStack, Scale);

                // 바뀐 Stack에 따라 기존에 적용된 Stack 효과를 Release하고, 현재 Stack에 맞는 새로운 Stack 효과들을 Apply함
                TryApplyStackActions();

                // Stack 수가 바뀌었음을 Event를 통해 외부에 알려줌
                onStackChanged?.Invoke(this, currentStack, prevStack);
            }
        }
    }

    // 0이면 횟수 무한
    public int ApplyCount => currentData.applyCount;
    public bool IsInfinitelyApplicable => ApplyCount == kInfinity;

    public int CurrentApplyCount
    {
        get => currentApplyCount;
        set => currentApplyCount = IsInfinitelyApplicable ? value : Mathf.Clamp(value, 0, ApplyCount);
    }
    
    // ApplyCycle: 0, ApplyCount: 3, Duration = 5
    // ApplyCycle: 5 / (3-1) = 2.5 
    // 0 => 2.5 => 5
    public float ApplyCycle => Mathf.Approximately(currentData.applyCycle, 0f) && ApplyCount > 1 ?
    (Duration / (ApplyCount - 1)) : currentData.applyCycle;

    // CurrentDuration은 Stack쌓이고 0으로 초기화 됨 
    // CurrentApplyCycle은 계속 쌓임
    // ApplyCycle을 확인하기 위한 변수
    public float CurrentApplyCycle
    {
        get => currentApplyCycle;
        set => currentApplyCycle = Mathf.Clamp(value, 0f, ApplyCycle);
    }

    private EffectAction Action => currentData.action;

    private CustomAction[] CustomActions => currentData.customActions;

    public object Owner { get; private set; }
    public Entity User { get; private set; }
    public Entity Target { get; private set; }

    // Effect위력 조절
    // ex. Casting시 ChargeTime만큼 위력 증가
    public float Scale { get; set; }

    public override string Description => BuildDescription(base.Description, 0);

    private bool IsApplyAllWhenDurationExpires => currentData.isApplyAllWhenDurationExpires;
    private bool IsDurationEnded => !IsTimeless && Mathf.Approximately(Duration, CurrentDuration);
    private bool IsApplyCompleted => !IsInfinitelyApplicable && CurrentApplyCount == ApplyCount;

    // Effect의 적용 완료 여부
    // 지속 시간 끝났거나
    // RunningFinishOption: FinishWhenApplyCompleted + Apply카운트가 최대 상황 
    public bool IsFinished => IsDurationEnded ||
    (currentData.runningFinishOption == EffectRunningFinishOption.FinishWhenApplyCompleted && IsApplyCompleted);

    // Effect완료 여부 상관 없이 Release됬는지 확인(다른 Effect에 의해 Effect제거되는 상황 있음)
    // Effect의 Release함수 실행시 true
    public bool IsReleased { get; private set; }

    public bool IsApplicable => Action != null && (
        currentApplyCount < ApplyCount || ApplyCount == kInfinity) && 
        CurrentApplyCycle >= ApplyCycle;

    public event StartedHandler onStarted;
    public event AppliedHandler onApplied;
    public event ReleasedHandler onReleased;
    public event StackChangedHandler onStackChanged;

    public void Setup(object owner, Entity user, int level, float scale = 1f)
    {
        Owner = owner;
        User = user;
        Level = level;
        CurrentApplyCycle = ApplyCycle;
        Scale = scale;
    }

    public void SetTarget(Entity target) => Target = target;

    private void ReleaseStackActionsAll()
    {
        appliedStackActions.ForEach(x => x.Release(this, level, User, Target, Scale));
        appliedStackActions.Clear();
    }

    private void ReleaseStackActions(System.Func<EffectStackAction, bool> predicate)
    {
        var stackActions = appliedStackActions.Where(predicate).ToList();
        foreach (var stackAction in stackActions)
        {
            stackAction.Release(this, level, User, Target, Scale);
            appliedStackActions.Remove(stackAction);
        }
    }

    private void TryApplyStackActions()
    {
        // 현재 적용된 StackAction들 중, 현재 Stack보다 더 큰 Stack을 요구하는 StackAction들 Release
        // ex. 3스택 쌓았는데 다른 효과에 의해 2스택으로 내려감 -> 3스택 효과 없앰
        ReleaseStackActions(x => x.Stack > currentStack);

        // 적용 가능한 StackAction목록
        var stackActions = StackActions.Where(x => x.Stack <= CurrentStack && !appliedStackActions.Contains(x) && x.IsApplicable);

        // 현재 적용된 StackActions들과 적용 가능한 StackActions에서 가장 높은 Stack값 찾아옴
        // ex. 현재 스택(5), 1 3 4 스택 중 4스택 가져옴
        int aplliedStackHighestStack = appliedStackActions.Any() ? appliedStackActions.Max(x => x.Stack) : 0;
        int stackActionsHighestStack = stackActions.Any() ? stackActions.Max(x => x.Stack) : 0;
        var highestStack = Mathf.Max(aplliedStackHighestStack, stackActionsHighestStack);
        
        if (highestStack > 0)
        {
            // 현재 3스택이면 2스택, 1스택 StatActions는 제외
            var except = stackActions.Where(x => x.Stack < highestStack && x.IsReleaseOnNextApply);
            stackActions = stackActions.Except(except);
        }

        if (stackActions.Any())
        {
            // 적용 된 Actioins에서는 IsReleaseOnNextApply였던 Actions를 Release
            // x.Stack < currentStack
            // -> 현재 스택은 적용되고 다음 프레임에 Release되도록 currentStack보다 낮은 Stack들 정리
            ReleaseStackActions(x => x.Stack < currentStack && x.IsReleaseOnNextApply);

            foreach (var stackAction in stackActions)
                stackAction.Apply(this, level, User, Target, Scale);

            appliedStackActions.AddRange(stackActions);
        }
    }

    public void Start()
    {
        Debug.Assert(!IsReleased, "Effect::Start - 이미 종료된 Effect입니다.");

        Action?.Start(this, User, Target, Level, Scale);

        TryApplyStackActions();

        foreach (var customAction in CustomActions)
            customAction.Start(this);

        onStarted?.Invoke(this);
    }

    public void Update()
    {
        CurrentDuration += Time.deltaTime;
        currentApplyCycle += Time.deltaTime;

        if (IsApplicable)
            Apply();

        if (IsApplyAllWhenDurationExpires && IsDurationEnded && !IsInfinitelyApplicable)
        {
            for (int i = currentApplyCount; i < ApplyCount; i++)
                Apply();
        }
    }

    public void Apply()
    {
        Debug.Assert(!IsReleased, "Effect::Apply - 이미 Released된 Effect입니다.");

        if (Action == null)
            return;

        // Apply가 false return하는 경우
        // ex. 캐릭터가 죽었을 때 살리는 버프, 캐릭터가 죽을 때까지 계속 false return
        //     캐릭터를 살리면 Apply적용되어 true return
        if (Action.Apply(this, User, Target, level, currentStack, Scale))
        {
            foreach (var customAction in CustomActions)
                customAction.Run(this);

            var prevApplyCount = CurrentApplyCount++;

            // Duration: 1.1f
            // CurrentDuration = 1.2f
            // CurrentApplyCycle = 1.02f(Time.DeltaTime오차로 인해)
            // Effect종료 해야되지만 Apply못하는 상황 발생
            if (isApplyTried)
                currentApplyCycle = 0f;
            else
                currentApplyCycle %= ApplyCycle;

            isApplyTried = false;

            onApplied?.Invoke(this, CurrentApplyCount, prevApplyCount);
        }
        else
            isApplyTried = true;
    }

    public void Release()
    {
        Debug.Assert(!IsReleased, "Effect::Release - 이미 종료된 Effect입니다.");

        Action?.Release(this, User, Target, level, Scale);
        ReleaseStackActionsAll();

        foreach (var customAction in CustomActions)
            customAction.Release(this);

        IsReleased = true;

        onReleased?.Invoke(this);
    }
    public EffectData GetData(int level) => effectDatas[level - 1];

    public string BuildDescription(string description, int effectIndex)
    {
        Dictionary<string, string> stringsByKeyword = new Dictionary<string, string>()
        {
            { "duration", Duration.ToString("0.##") },
            { "applyCount", ApplyCount.ToString() },
            { "applyCycle", ApplyCycle.ToString("0.##") }
        };

        description = TextReplacer.Replace(description, stringsByKeyword, effectIndex.ToString());

        description = Action.BuildDescription(this, description, 0, 0, effectIndex);

        var stackGroups = StackActions.GroupBy(x => x.Stack);
        foreach (var stackGroup in stackGroups)
        {
            int i = 0;
            foreach (var stackAction in stackGroup)
                description = stackAction.BuildDescription(this, description, i++, effectIndex);
        }

        return description;
    }

    public override object Clone()
    {
        var clone = Instantiate(this);

        if (Owner != null)
            clone.Setup(Owner, User, Level, Scale);

        return clone;
    }
}
