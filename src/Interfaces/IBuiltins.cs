public interface IBuiltins
{
    Dictionary<string, Action<IInstruction>> Commands { get; }
}