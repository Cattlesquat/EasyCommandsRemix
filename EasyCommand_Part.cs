using System;
using System.Linq;
using XRL;
using XRL.UI;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Skill;

namespace EasyCommand
{
    public class EasyCommand_Part : IPart
    {
        public override bool WantEvent(int ID, int cascade)
        {
            return base.WantEvent(ID, cascade) || ID == CommandEvent.ID;
        }

        public override bool HandleEvent(CommandEvent E)
        {
            if (E.Command == "Easy_Throwables")
            {
                EasyThrowables(E.Actor);
            }
            else if (E.Command == "Easy_Tonics")
            {
                EasyTonics(E.Actor);
            }
            return base.HandleEvent(E);
        }

		public void EasyThrowables(GameObject who)
        {
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

					//var options = throwables.Select(x => x.DisplayName).ToList();
					//var choice = Popup.PickOption(Title: $"Equip which throwable item?", Options: options, AllowEscape: true);

                    var throwable = Popup.PickGameObject(Title: $"Equip which throwable item?", throwables, AllowEscape: true);

					//var choice = Popup.ShowOptionList($"Equip which throwable item?", options, Helpers.GetHotkeys(options.Length), AllowEscape: true);

					if (throwable != null)
					{
						//var throwable = throwables[choice];
						if (throwingSlot.Equipped != null)
                        {
							throwingSlot.TryUnequip(true, false);
                        }
						//who.AutoEquip(throwable, false, false, false);
                        who.FireEvent(Event.New("CommandEquipObject", "Object", throwable, "BodyPart", throwingSlot)); // Use explicit equip to throwing slot, as many things appearing on the valid list won't default there. 
					}
				}
				else
                {
					Popup.ShowFail("You can't throw things!");
				}
			}
		}

		private static bool IsMedication(GameObject go)
        {
            return go.HasPart<Tonic>() || go.HasPart<Medication>() || go.HasPart<GeometricHealOnEat>(); 
        }

        private static bool IsHealingFood(GameObject go)
        {
            return go.TryGetPart<Food>(out var food) && !string.IsNullOrEmpty(food.Healing) && food.Healing != "0";
        }

		private static int RankMedicationAndFood(GameObject go)
        {
			if (IsHealingFood(go)) return 0;
            if (go.HasPart<Tonic>()) return 1;
			return 2;
		}

		private static int RankThrowable(GameObject go)
        {
            if (go.HasPart<GeomagneticDisc>()) return 0;
			if (go.HasTag("Grenade")) return 1;
			return 2;
        }

		public void EasyTonics(GameObject who)
        {
			if (who.IsPlayer())
			{
				var tonics = who.GetInventory().Where(x => IsMedication(x) || IsHealingFood(x)).OrderBy(x => RankMedicationAndFood(x)).ToArray();

				if (tonics.Length == 0)
				{
					Popup.ShowFail("You have no medication!");
					return;
				}

				//var options = tonics.Select(x => x.DisplayName).ToList();
				//var choice = Popup.PickOption(Title: $"Choose Meds or Tonics", Options: options, Hotkeys: Helpers.GetHotkeys(options.Count), AllowEscape: true);
				//var choice = Popup.ShowOptionList($"Apply which medication?", options, Helpers.GetHotkeys(options.Length), AllowEscape: true);

                var tonic = Popup.PickGameObject(Title: $"Choose Meds or Tonics", tonics, AllowEscape: true);
				if (tonic != null)
				{
					if (IsMedication(tonic))
					{
						InventoryActionEvent.Check(tonic, who, tonic, tonic.HasPart<Food>() ? "Eat" : "Apply"); // Witchwood Bark is a "med" but you "eat" it.
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
