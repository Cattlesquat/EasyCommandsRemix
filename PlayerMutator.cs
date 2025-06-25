using XRL; // to abbreviate XRL.PlayerMutator and XRL.IPlayerMutator
using XRL.World; // to abbreviate XRL.World.GameObject

namespace EasyCommand
{
    [PlayerMutator]
    public class MyPlayerMutator : IPlayerMutator
    {
        public void mutate(GameObject player)
        {
            if (!player.HasPart(nameof(EasyCommand_Part)))
            {
                player.AddPart<EasyCommand_Part>();
            }
        }
    }
}