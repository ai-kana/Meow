using SDG.Unturned;
using UnityEngine;

namespace Meow.Core.Players.Components;

public class MeowPlayerQuests
{
    public HashSet<string> Roles => Owner.SaveData.Roles;

    public readonly MeowPlayer Owner; 
    private PlayerQuests _Quests => Owner.Player.quests;
    private PlayerSkills _Skills => Owner.Player.skills;

    public MeowPlayerQuests(MeowPlayer owner)
    {
        Owner = owner;
    }

    public int Reputation => _Skills.reputation;
    
    public bool TryGetMarkerPosition(out Vector3 position)
    {
        position = Vector3.zero;
        if (!_Quests.isMarkerPlaced)
        {
            return false;
        }

        Vector3 marker = _Quests.markerPosition;
        Ray ray = new(new Vector3(marker.x, Level.HEIGHT, marker.y), Vector3.down);
        if (!Physics.Raycast(ray, out RaycastHit info, Level.HEIGHT, RayMasks.BLOCK_COLLISION))
        {
            return false;
        }

        position = info.point;
        return true;
    }

    public bool FlagExists(ushort id)
    {
        return _Quests.flagsList.Exists(x => x.id == id);
    }

    public void SetFlag(ushort id, short value)
    {
        _Quests.setFlag(id, value);
    }

    public void RemoveFlag(ushort id)
    {
        _Quests.removeFlag(id);
    }

    public bool TryGetFlag(ushort id, out short value)
    {
        return _Quests.getFlag(id, out value);
    }
    
        
    public void GiveReputation(int rep)
    {
        _Skills.askRep(rep);
    }
    
    public void RemoveReputation(int rep)
    {
        _Skills.askRep(-rep);
    }
    
    public void SetReputation(int rep)
    {
        _Skills.askRep(rep - _Skills.reputation);
    }
}
