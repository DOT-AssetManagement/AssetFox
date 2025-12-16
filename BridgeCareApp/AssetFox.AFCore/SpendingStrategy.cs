namespace AssetFox.AFCore
{
    public enum SpendingStrategy
    {
        NoSpending,
        UnlimitedSpending,
        UntilTargetAndDeficientConditionGoalsMet,
        UntilTargetConditionGoalsMet,
        UntilDeficientConditionGoalsMet,
        AsBudgetPermits,
    }
}
