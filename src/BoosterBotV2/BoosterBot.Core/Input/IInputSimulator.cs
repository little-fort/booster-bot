namespace BoosterBot.Core.Input;

public interface IInputSimulator
{
    void Click(int x, int y);
    void DragAndDrop(int fromX, int fromY, int toX, int toY, DragOptions? options = null);
    void SendKey(int virtualKeyCode);
}
