namespace los.BLM.SlotResolver;

public enum BlmLoopPhase
{
    None,       // 未进入循环
    IceCore,    // 冰相核心（冰三 + 冰澈 + UI悖论）
    ToFire,     // 转火阶段（UI悖论后 → 星灵移位 → 火三）
    FireCore,   // 火相核心（火悖论 + 火四 xN）
    Finisher    // 收尾（耀星 + 绝望）
}