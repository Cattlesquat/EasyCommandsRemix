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
    [XRL.UI.LookerUIPlugin]
    public class EasyCommand_Looker : XRL.UI.ILookerUIPlugin
    {
        public override string GetMessage(XRL.UI.Look.LookerState looker)
        {
            return " | {{hotkey|" + ControlManager.getCommandInputFormatted("Easy_Look", false) +
                   "}} points of interest";
        }
        
        public int PointCount = 0;
        public List<PointOfInterest> PointsOfInterest = new();

        public void UpdatePointsOfInterest()
        {
            PointsOfInterest = GetPointsOfInterestEvent.GetFor(XRL.The.Player);
            PointCount = PointsOfInterest?.Count ?? 0;
            
            if (PointCount > 0)
            {
                PointsOfInterest.Sort(PointOfInterest.Compare);

                if (GameManager.Instance.CurrentGameView != "Looker")
                {
                    EasyCommand.EasyCommand_Part.LookIndex = 0;
                }
                else
                {
                    EasyCommand.EasyCommand_Part.LookIndex = (EasyCommand.EasyCommand_Part.LookIndex + 1) % (PointsOfInterest.Count + 1);
                }
            }
            else
            {
                EasyCommand.EasyCommand_Part.LookIndex = 0;
            }
        }

        
        public override bool HandleKey(XRL.UI.Look.LookerState looker, Keys c)
        {
            if (c == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:Easy_Look")
            {
                UpdatePointsOfInterest();
                
                tryagain:
                if (EasyCommand.EasyCommand_Part.LookIndex < PointCount)
                {
                    GameObject go = PointsOfInterest[EasyCommand.EasyCommand_Part.LookIndex].Object;
                    if (go != null)
                    {
                        //XRL.Messages.MessageQueue.AddPlayerMessage(PointsOfInterest[EasyCommand.EasyCommand_Part.LookIndex].DisplayName + " (" + EasyCommand.EasyCommand_Part.LookIndex + ")");
                        looker.xp = go.CurrentCell.X;
                        looker.yp = go.CurrentCell.Y;
                    }
                    else if (PointsOfInterest[EasyCommand.EasyCommand_Part.LookIndex].Location != null)
                    {
                        //XRL.Messages.MessageQueue.AddPlayerMessage(PointsOfInterest[EasyCommand.EasyCommand_Part.LookIndex].DisplayName + " (" + EasyCommand.EasyCommand_Part.LookIndex + ")");

                        looker.xp = PointsOfInterest[EasyCommand.EasyCommand_Part.LookIndex].Location.X;
                        looker.yp = PointsOfInterest[EasyCommand.EasyCommand_Part.LookIndex].Location.Y;
                    }
                    else
                    {
                        EasyCommand.EasyCommand_Part.LookIndex++;
                        goto tryagain;
                    }

                    looker.bUpdateTooltip = true;
                }
                else
                {
                    Cell ourCell = XRL.The.Player.CurrentCell;
                    looker.xp = ourCell.X;
                    looker.yp = ourCell.Y;
                    looker.bUpdateTooltip = true;
                }
            }

            return true;
        }
    }
}