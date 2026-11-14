public interface IKineticCarryTarget
{
    bool TryBeginCarry();
    void EndCarry();
    void OnLaunched();
}
