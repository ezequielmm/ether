using System.Linq;

namespace Spine.Unity
{
    public class EnemyIdleSolver : IIdleSolver
    {
        private StatusManager statusManager;
        
        public EnemyIdleSolver(StatusManager statusManager)
        {
            this.statusManager = statusManager;
        }
        
        public string DetermineIdleSequence()
        {
            var particular = statusManager.GetStatusList()
                .FirstOrDefault(e => e.name == "hidden" || e.name == "chargingBeam");
            return particular != null ?
                particular.name switch
                {
                    "hidden" => "hidden_idle",
                    "chargingBeam" => "inhale_idle",
                    _ => "idle"
                }
                : "idle";
        }
    }
}