namespace BoosterBot.Core.Input;

public record DragOptions(
    int Steps = 50,
    int StepDelayMs = 7,
    bool AddJitter = true
);
