using System;
using System.Collections.Generic;
using System.Linq;
using XRL;
using XRL.UI;
using XRL.World;
using XRL.World.Anatomy;
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
            else if (E.Command == "Easy_Quench")
            {
                EasyQuench(E.Actor);
            }
            else if (E.Command == "Easy_Tools")
            {
                EasyTools(E.Actor);
            }
            else if (E.Command == "Easy_Equipment")
            {
                EasyEquipment(E.Actor);
            }
            return base.HandleEvent(E);
        }

		public void EasyThrowables(GameObject who)
        {
			if (!who.IsPlayer()) return;

            if (who.IsConfused)
            {
                Popup.ShowFail("You get confused about what type of object you were trying to find.");
                return;
            }

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
            if (who.IsConfused)
            {
                Popup.ShowFail("You get confused about what type of object you were trying to find.");
                return;
            }

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
            if (who.IsConfused)
            {
                Popup.ShowFail("You get confused about what type of object you were trying to find.");
                return;
            }

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
            if (who.IsConfused)
            {
                Popup.ShowFail("You get confused about what you are trying to do.");
                return;
            }
            var waterContainers = who.GetInventory().Where(x => (x.GetInventoryCategory() == "Water Containers")).Where(x => (x.LiquidVolume.IsFreshWater() && x.LiquidVolume.Volume > 0)).OrderBy(x => RankCleanContainer(x)).ToArray();
            if (waterContainers.Length == 0) {
                Popup.ShowFail("You have no fresh water available.");
                return;
            }

            var container = waterContainers[0];
            InventoryActionEvent.Check(container, who, container, "CleanWithLiquid");
        }

        public void EasyQuench(GameObject who) {
            if (!who.IsPlayer()) return;
            if (who.IsConfused)
            {
                Popup.ShowFail("You get confused about what you are trying to do.");
                return;
            }
            var waterContainers = who.GetInventory().Where(x => (x.GetInventoryCategory() == "Water Containers")).Where(x => (x.LiquidVolume.IsFreshWater() && x.LiquidVolume.Volume > 0)).OrderBy(x => RankCleanContainer(x)).ToArray();
            if (waterContainers.Length == 0) {
                Popup.ShowFail("You have no fresh water available.");
                return;
            }
        
            var container = waterContainers[0];

            // The below code is simplified a simplified version of what happens in LiquidVolume::Pour
            int PourAmount = 1;
            PlayWorldSound("Sounds/Interact/sfx_interact_liquidContainer_pourout");
            Popup.Show(PourAmount.Things("dram") + " of " + container.LiquidVolume.GetLiquidName() + " pours out all over you!");
            PourAmount -= container.LiquidVolume.ProcessContact(who, Initial: true, Poured: true, PouredBy: who, ContactVolume: PourAmount);
            if (PourAmount <= 0) return;

            bool RequestInterfaceExit = false;
            container.LiquidVolume.PourIntoCell(who, who.GetCurrentCell(), PourAmount, ref RequestInterfaceExit, CanPourOn: true);
        }


		private static int RankTool(GameObject who, GameObject go)
        {
            if (go.TryGetPart<Examiner>(out var examiner) && !go.Understood(examiner)) return 0;
            if (go.GetInventoryCategory() == "Artifacts") return 1;
            if (go.GetInventoryCategory() == "Applicators") return 2;
            return 3;
        }
            

        public void EasyTools(GameObject who) {
            if (!who.IsPlayer()) return;
            if (who.IsConfused)
            {
                Popup.ShowFail("You get confused about what type of object you were trying to find.");
                return;
            }

            var tools = who.GetInventory().Where(x => (x.GetInventoryCategory() == "Applicators") || (x.GetInventoryCategory() == "Artifacts") || (x.GetInventoryCategory() == "Tools")).OrderBy(x => RankTool(who, x)).ToArray();
            if (tools.Length == 0) {
                Popup.ShowFail("You have neither tools nor artifacts.");
                return;
            }

            var tool = Popup.PickGameObject(Title: $"Artifacts, Applicators and Tools", tools, AllowEscape: true);
            if (tool == null) return;

            tool.Twiddle();
        }


		private static int RankEquipment(GameObject go)
        {
            if (go.GetInventoryCategory() == "Armor") return 0;
            if (go.GetInventoryCategory() == "Melee Weapons") return 1;
            return 2;
        }


        public void EasyEquipment(GameObject who) {
            if (!who.IsPlayer()) return;
            if (who.IsConfused)
            {
                Popup.ShowFail("You get confused about what type of object you were trying to find.");
                return;
            }

            List<GameObject> equipment = new();
            List<BodyPart> bodyParts = who.Body.GetParts();
            bool needContext = false;
            foreach (var bodyPart in bodyParts)
            {
                if (bodyPart.Equipped == null) continue;
                var cat = bodyPart.Equipped.GetInventoryCategory();
                if ((cat != "Armor") && (cat != "Melee Weapons") && (cat != "Missile Weapons")) continue;
                equipment.Add(bodyPart.Equipped);
                needContext = true;
            }

            equipment.AddRange(who.GetInventory().Where(x => (x.GetInventoryCategory() == "Armor") || (x.GetInventoryCategory() == "Melee Weapons") || (x.GetInventoryCategory() == "Missile Weapons")).OrderBy(x => RankEquipment(x)));

            if (equipment.Count == 0) {
                Popup.ShowFail("You have ... no equipment at all?");
                return;
            }

            var item = Popup.PickGameObject(Title: $"Armor, Melee Weapons, and Missile Weapons", equipment, AllowEscape: true, ShowContext: needContext);
            if (item == null) return;

            item.Twiddle();
        }
	}
}
