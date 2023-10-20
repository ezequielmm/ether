using Combat.VFX;
using Spine.Unity;
using UnityEngine;

internal class PlayerIdleSolver : IIdleSolver
{
    private PlayerData data;
    
    public PlayerIdleSolver(PlayerData playerData)
    {
        data = playerData;
        if(PlayerPrefs.GetInt("enable_injured_idle") == 1)
            GameSettings.SHOW_PLAYER_INJURED_IDLE = true;
    }

    public string DetermineIdleSequence()
    => data != null && data.hpCurrent < (data.hpMax / 4)  && GameSettings.SHOW_PLAYER_INJURED_IDLE ? 
        "injured_idle" : 
        "idle";

    public VFX DetermineIdleVFX()
    {
        return VFX.None;
    }
}