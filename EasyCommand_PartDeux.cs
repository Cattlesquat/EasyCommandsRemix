using System;
using System.Collections.Generic;
using System.Linq;
using XRL;
using XRL.UI;
using XRL.World;
using XRL.World.Anatomy;
using XRL.World.Parts;
using XRL.World.Parts.Skill;
using ConsoleLib.Console;

namespace EasyCommand
{
    [XRL.UI.HasLookerHooks]
    public partial class EasyCommand_Part : IPlayerPart
    {
        [XRL.UI.LookerMessage]
        public static string LookerString()
        {
            return " | {{hotkey|" + ControlManager.getCommandInputFormatted("Easy_Look", false) +
                   "}} points of interest";
        }

        [XRL.UI.LookerCommand]
        public static void LookerCommand(Keys c, ref int X, ref int Y, ref bool bDone, ref bool bUpdateTooltip,
            ref int pickObject, ref int TopLine)
        {
            if (c == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:Easy_Look")
            {
                UpdatePointsOfInterest();

                tryagain:
                if (LookIndex < PointCount)
                {
                    GameObject go = PointsOfInterest[LookIndex].Object;
                    if (go != null)
                    {
                        X = go.CurrentCell.X;
                        Y = go.CurrentCell.Y;
                    }
                    else if (PointsOfInterest[LookIndex].Location != null)
                    {
                        X = PointsOfInterest[LookIndex].Location.X;
                        Y = PointsOfInterest[LookIndex].Location.Y;
                    }
                    else
                    {
                        LookIndex++;
                        goto tryagain;
                    }

                    bUpdateTooltip = true;
                }
                else
                {
                    Cell ourCell = XRL.The.Player.CurrentCell;
                    X = ourCell.X;
                    Y = ourCell.Y;
                    bUpdateTooltip = true;
                }
            }
        }
    }
}