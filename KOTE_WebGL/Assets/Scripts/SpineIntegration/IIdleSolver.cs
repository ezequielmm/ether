using Combat.VFX;

namespace Spine.Unity
{
    public interface IIdleSolver
    {
        public string DetermineIdleSequence();
        public VFX DetermineIdleVFX();
    }
}