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
            else if (E.Command == "Easy_Water")
            {
                EasyWater(E.Actor);
            }
            else if (E.Command == "Easy_Clean")
            {
                EasyClean(E.Actor);
            }
            return base.HandleEvent(E);
        }

		public void EasyThrowables(GameObject who)
        {
			if (!who.IsPlayer()) return;

   			var throwingSlot = who.Body.GetPart("Thrown Weapon").FirstOrDefault();
            if (throwingSlot == null) {
                Popup.ShowFail("You can't throw things!");
                return;
            }

			var throwables = who.GetInventory().Where(x => x.IsThrownWeapon).OrderBy(x => RankThrowable(x)).ToArray();

			if (throwables.Length == 0)
			{
				Popup.ShowFail("You have no throwable items!");
				return;
			}

            var throwable = Popup.PickGameObject(Title: $"Equip which throwable item?", throwables, AllowEscape: true);
			if (throwable == null) return;

			if (throwingSlot.Equipped != null)
            {
				throwingSlot.TryUnequip(true, false);
            }
            who.FireEvent(Event.New("CommandEquipObject", "Object", throwable, "BodyPart", throwingSlot)); // Use explicit equip to throwing slot, as many things appearing on the valid list won't default there. 
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
			if (IsHealingFood(go)) return 0; // Basically urberry
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
			if (!who.IsPlayer()) return;

			var tonics = who.GetInventory().Where(x => IsMedication(x) || IsHealingFood(x)).OrderBy(x => RankMedicationAndFood(x)).ToArray();
			if (tonics.Length == 0)
			{
				Popup.ShowFail("You have no medication!");
				return;
			}

            var tonic = Popup.PickGameObject(Title: $"Choose Meds or Tonics", tonics, AllowEscape: true);
            if (tonic == null) return;

			if (IsMedication(tonic))
			{
				InventoryActionEvent.Check(tonic, who, tonic, tonic.HasPart<Food>() ? "Eat" : "Apply"); // Witchwood Bark is a "med" but you "eat" it.
			}
			else if (IsHealingFood(tonic))
			{
				InventoryActionEvent.Check(tonic, who, tonic, "Eat");
            }
		}


		private static int RankWaterContainer(GameObject go)
        {
            var vol = go.LiquidVolume;
            if (vol == null) return 999999;

            if (vol.IsFreshWater()) {
                return 1000 - vol.Volume;
            }

            return 2000 - vol.Volume;
        }


        public void EasyWater(GameObject who) {
            if (!who.IsPlayer()) return;

            var waterContainers = who.GetInventory().Where(x => (x.GetInventoryCategory() == "Water Containers")).OrderBy(x => RankWaterContainer(x)).ToArray();
            if (waterContainers.Length == 0) {
                Popup.ShowFail("You have no water containers.");
                return;
            }

            var container = Popup.PickGameObject(Title: $"Choose Container", waterContainers, AllowEscape: true);
            if (container == null) return;

            container.Twiddle();
        }

		private static int RankCleanContainer(GameObject go)
        {
            var vol = go.LiquidVolume;
            if (vol == null) return 999999;
            return vol.Volume;
        }

        public void EasyClean(GameObject who) {
            if (!who.IsPlayer()) return;
            var waterContainers = who.GetInventory().Where(x => (x.GetInventoryCategory() == "Water Containers")).Where(x => (x.LiquidVolume.IsFreshWater() && x.LiquidVolume.Volume > 0)).OrderBy(x => RankCleanContainer(x)).ToArray();
            if (waterContainers.Length == 0) {
                Popup.ShowFail("You have no fresh water available.");
                return;
            }

            var container = waterContainers[0];
            InventoryActionEvent.Check(container, who, container, "CleanWithLiquid");
        }
	}
}
