using XRL; // to abbreviate XRL.PlayerMutator and XRL.IPlayerMutator
using XRL.World; // to abbreviate XRL.World.GameObject
using UnityEngine;

namespace EasyCommand
{
    [PlayerMutator]
    public class MyPlayerMutator : IPlayerMutator
    {
        public void mutate(XRL.World.GameObject player)
        {
            Debug.Log("Easy Commands - MUTATE!");
            if (!player.HasPart(nameof(EasyCommand_Part)))
            {
                player.AddPart<EasyCommand_Part>();
            }
        }
    }
}