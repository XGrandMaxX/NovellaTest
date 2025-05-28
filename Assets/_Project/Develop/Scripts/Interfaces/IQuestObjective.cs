public interface IQuestObjective
{
    string Description { get; }
    bool IsCompleted { get; }
    void Reset();
    void UpdateCompletedPreview();
}