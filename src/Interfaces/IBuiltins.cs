public interface IBuiltins
{
    Dictionary<string, Action<Instruction>> Commands { get; }
}