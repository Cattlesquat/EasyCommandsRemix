using System;
using System.Collections.Generic;
using System.Text;
using XRL;
using XRL.Core;

namespace EasyCommand
{
    [HasCallAfterGameLoadedAttribute]
    public class MyLoadGameHandler
    {
        [CallAfterGameLoadedAttribute]
        public static void LoadGameCallback()
        {
            // Called whenever loading a save game
            var player = XRLCore.Core?.Game?.Player?.Body;
            
            if (player != null && !player.HasPart(nameof(EasyCommand_Part)))
            {
                player.AddPart<EasyCommand_Part>();
            }
        }
    }
}
