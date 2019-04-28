using Svelto.ECS;

namespace Svelto.ECS.Debugger
{
    public static class DebuggerExtensions
    {
        public static void AttachDebugger(this EnginesRoot root)
        {
            Debugger.Instance.AddEnginesRoot(root);
        }
    }
}