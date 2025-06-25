using System;
using System.Linq;
using XRL;
using XRL.UI;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Skill;
using UnityEngine;

namespace EasyCommand
{
    public class EasyCommand_Part : IPart
    {
		public string[] CommandIDs = new[] { "Easy_Throwables", "Easy_Recoilers", "Easy_Tonics" };

		public override void Register(XRL.World.GameObject Object, IEventRegistrar Registrar)
		{
            Debug.Log("Easy Commands - register");
			foreach (var command in CommandIDs)
            {
				Object.RegisterPartEvent(this, command);
			}
			
			base.Register(Object, Registrar);
		}

		public override bool FireEvent(XRL.World.Event E)
		{
			if (E.ID == "Easy_Throwables")
			{
                Debug.Log("Easy Throwables");
				EasyThrowables(ParentObject);
			}
			if (E.ID == "Easy_Recoilers")
			{
                Debug.Log("Easy Recoilers");
				EasyRecoilers(ParentObject);
			}
			if (E.ID == "Easy_Tonics")
            {
                Debug.Log("Easy Tonics");
				EasyTonics(ParentObject);
            }

			return base.FireEvent(E);
		}

		public void EasyRecoilers(XRL.World.GameObject who)
		{
            Debug.Log("Easy Commands - Recoil!");

			if (who.IsPlayer())
            {
                var part = who.GetPart<RecoilAbility>();
                if (part != null) {
                    bool interfaceExitRequested = false;
                    CommandEvent.Send(Actor: who, Command: RecoilAbility.COMMAND_NAME);
                }
                else {
                    Popup.ShowFail("You do not have any recoilers.");
                }
                /*
				if (who.AreHostilesNearby())
				{
					Popup.ShowFail("You can't recoil with hostiles nearby!");
				}
				else
				{
					var recoilers = who.GetInventoryAndEquipment(x => x.HasPart("Teleporter")).ToArray();

					if (recoilers.Length == 0)
                    {
						Popup.ShowFail("You have no recoilers!");
						return;
					}

					var options = recoilers.Select(x => x.DisplayName).ToList();
                    var choice = Popup.PickOption(Title: $"Activate which recoiler?", Options: options, AllowEscape: true);
					//var choice = Popup.ShowOptionList($"Activate which recoiler?", options, Helpers.GetHotkeys(options.Length), AllowEscape: true);

					if (choice != -1)
					{
						var recoiler = recoilers[choice];

						var teleporter = recoiler.GetPart<Teleporter>();

						if (teleporter.AttemptTeleport(who, null))
						{
							who.UseEnergy(1000, "Item Recoiler");
						}
					}
				}
                */
			}			
		}

		
		public void EasyThrowables(XRL.World.GameObject who)
        {
            Debug.Log("Easy Commands - Throwable!");

			if (who.IsPlayer())
			{
				var throwingSlot = who.Body.GetPart("Thrown Weapon").FirstOrDefault();
				if (throwingSlot != null)
				{
					var throwables = who.GetInventory().Where(x => x.IsThrownWeapon).OrderBy(x => RankThrowable(x)).ToArray();

					if (throwables.Length == 0)
					{
						Popup.ShowFail("You have no throwable items!");
						return;
					}

					var options = throwables.Select(x => x.DisplayName).ToList();
					var choice = Popup.PickOption(Title: $"Equip which throwable item?", Options: options, AllowEscape: true);
					//var choice = Popup.ShowOptionList($"Equip which throwable item?", options, Helpers.GetHotkeys(options.Length), AllowEscape: true);

					if (choice != -1)
					{
						var throwable = throwables[choice];

						if (throwingSlot.Equipped != null)
                        {
							throwingSlot.TryUnequip(true, false);
                        }
						who.AutoEquip(throwable, false, false, false);
					}
				}
				else
                {
					Popup.ShowFail("You can't throw things!");
				}
			}
		}

		private static bool IsMedication(XRL.World.GameObject go)
        {
			if (go.HasPart("Tonic"))
				return true;
			if (go.HasPart("Medication"))
				return true;
			if (go.HasPart("GeometricHealOnEat"))
				return true;

			return false;
        }

		private static bool IsHealingFood(XRL.World.GameObject go)
        {
			var food = go.GetPart<Food>();
			return !String.IsNullOrEmpty(food?.Healing) && food?.Healing != "0";
		}

		private static bool IsUtility(XRL.World.GameObject who, XRL.World.GameObject go)
        {			
			var actionsEvent = GetInventoryActionsEvent.FromPool(who, go, new System.Collections.Generic.Dictionary<string, InventoryAction>());
			go.HandleEvent(actionsEvent);

			return actionsEvent.Actions.Values.Any(x => x.Name == "Activate");
		}

		private static int RankMedicationAndFood(XRL.World.GameObject go)
        {
			if (IsHealingFood(go))			
				return 0;

			if (go.HasPart("Tonic"))
				return 1;

			return 2;
		}

		private static int RankThrowable(XRL.World.GameObject go)
        {
			if (go.HasPart("GeomagneticDisc"))
				return 0;
			if (go.HasTag("Grenade"))
				return 1;

			return 2;
        }

		public void EasyTonics(XRL.World.GameObject who)
        {
            Debug.Log("Easy Commands - Tonic!");

			if (who.IsPlayer())
			{
				var tonics = who.GetInventory().Where(x => IsMedication(x) || IsHealingFood(x)).OrderBy(x => RankMedicationAndFood(x)).ToArray();

				if (tonics.Length == 0)
				{
					Popup.ShowFail("You have no medication!");
					return;
				}

				var options = tonics.Select(x => x.DisplayName).ToList();
				var choice = Popup.PickOption(Title: $"Apply which medication?", Options: options, AllowEscape: true);
				//var choice = Popup.ShowOptionList($"Apply which medication?", options, Helpers.GetHotkeys(options.Length), AllowEscape: true);

				if (choice != -1)
				{
					var tonic = tonics[choice];

					if (IsMedication(tonic))
					{
						InventoryActionEvent.Check(tonic, who, tonic, "Apply"); // who, tonic, "Apply");
					}
					else if (IsHealingFood(tonic))
					{
						InventoryActionEvent.Check(tonic, who, tonic, "Eat");
					}
				}
			}
		}
	}
}
