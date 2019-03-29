using Smod2;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using SMitem = Smod2.API.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace EnterTheJuggernaut
{
	class EventHandler : IEventHandlerRoundStart, IEventHandlerWaitingForPlayers, IEventHandlerElevatorUse, IEventHandlerDoorAccess, IEventHandlerPlayerPickupItem,
		IEventHandlerPlayerTriggerTesla, IEventHandlerSummonVehicle, IEventHandlerRoundEnd, IEventHandlerPlayerDie, IEventHandlerPlayerJoin, IEventHandlerReload
	{
		private readonly EnterTheJuggernaut plugin;

		public static bool WaitingForPlayers = false;
		public static bool GhostRound = false;
		public static Player Juggernaut { get; set; }
		public int spawncount = 0;
		public bool roundstart;

		public EventHandler(EnterTheJuggernaut plugin) => this.plugin = plugin;

		public void TaskForcespawn(Player taskforce)
		{
			taskforce.ChangeRole(Role.NTF_LIEUTENANT, false, false);
			taskforce.Teleport(plugin.Server.Map.GetRandomSpawnPoint(Role.SCP_939_53));

			foreach (SMitem item in taskforce.GetInventory())
			{
				item.Remove();
			}

			taskforce.GiveItem(ItemType.E11_STANDARD_RIFLE);
			taskforce.GiveItem(ItemType.P90);
			taskforce.GiveItem(ItemType.MEDKIT);
			taskforce.GiveItem(ItemType.RADIO);
			taskforce.GiveItem(ItemType.WEAPON_MANAGER_TABLET);

			taskforce.SetAmmo(AmmoType.DROPPED_5, 80);
			taskforce.SetAmmo(AmmoType.DROPPED_7, 0);
			taskforce.SetAmmo(AmmoType.DROPPED_9, 100);

			taskforce.PersonalBroadcast(10, "<color=blue>YOU ARE THE TASKFORCE!</color> <color=red>Destroy the Juggernaut!</color> ", false);
		}
		public void Juggernuaghtspawn(Player player)
		{
			Juggernaut.ChangeRole(Role.CHAOS_INSURGENCY, false, false);
			Juggernaut.Teleport(plugin.Server.Map.GetRandomSpawnPoint(Role.SCP_106));
			foreach (SMitem item in Juggernaut.GetInventory())
			{
				item.Remove();
			}
			Juggernaut.SetHealth(plugin.ETJhp);
			Juggernaut.GiveItem(ItemType.LOGICER);
			Juggernaut.SetAmmo(AmmoType.DROPPED_7, 200);
			Juggernaut.PersonalBroadcast(10, "<color=red>YOU ARE THE JUGGERNAUT!</color> <color=yellow>You have high HP, and can break down doors!</color> <color=blue>Kill them all!</color>", false);
			Juggernaut.SetRank("light_green", "JUGGERNAUT");
		}

		public void OnRoundStart(RoundStartEvent ev)
		{
			WaitingForPlayers = false;
			if (plugin.Enabled)
			{
				foreach (Elevator elevator in plugin.Server.Map.GetElevators())
				{
					switch (elevator.ElevatorType)
					{
						case ElevatorType.SCP049Chamber when elevator.ElevatorStatus == ElevatorStatus.Up:
						case ElevatorType.LiftA when elevator.ElevatorStatus == ElevatorStatus.Down:
						case ElevatorType.LiftB when elevator.ElevatorStatus == ElevatorStatus.Down:
						case ElevatorType.WarheadRoom when elevator.ElevatorStatus == ElevatorStatus.Up:
							elevator.Use();
							break;
					}
				}

				List<Smod2.API.Door> doors = plugin.Server.Map.GetDoors();
				doors.First(x => x.Name == "CHECKPOINT_ENT").Locked = true;
				doors.First(x => x.Name == "HCZ_ARMORY").Locked = true;
				doors.First(x => x.Name == "096").Locked = true;
				doors.First(x => x.Name == "HID").Locked = true;
				// Start choosing players

				List<Player> players = plugin.Server.GetPlayers();
				foreach (Player p in players)
				{

					if (Juggernaut == null || !players.Contains(Juggernaut))
						Juggernaut = p;
					else
					{
						TaskForcespawn(p);
					}
				}
				Juggernuaghtspawn(Juggernaut);

				spawncount = 1;
			}
		}
		public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
		{
			plugin.RefreshConfig();
		}
		public void OnElevatorUse(PlayerElevatorUseEvent ev)
		{
			if (plugin.Enabled)
			{
				ev.AllowUse = false;
			}
		}
		public void OnDoorAccess(PlayerDoorAccessEvent ev)
		{
			if (plugin.Enabled && Juggernaut.PlayerId == ev.Player.PlayerId)
			{
				ev.Destroy = true;
			}
		}
		public void OnPlayerPickupItem(PlayerPickupItemEvent ev)
		{
			if (plugin.Enabled && Juggernaut.PlayerId == ev.Player.PlayerId && ev.Item.ItemType != ItemType.LOGICER)
			{
				ev.Allow = false;
			}
		}
		public void OnPlayerTriggerTesla(PlayerTriggerTeslaEvent ev)
		{
			if (plugin.Enabled && Juggernaut.PlayerId != ev.Player.PlayerId)
			{
				ev.Triggerable = false;
			}
		}
		public void OnSummonVehicle(SummonVehicleEvent ev)
		{
			if (plugin.Enabled)
			{
				ev.AllowSummon = false;
			}
		}
		public void OnRoundEnd(RoundEndEvent ev)
		{
			if (plugin.Enabled)
			{
				spawncount = 0;
				roundstart = false;
				plugin.Enabled = false;
				Juggernaut.SetRank();
			}
		}
		public void OnPlayerDie(PlayerDeathEvent ev)
		{
			if (plugin.Enabled && Juggernaut.PlayerId != ev.Player.PlayerId && plugin.Server.Round.Duration > 31f)
			{
				Juggernaut.PersonalBroadcast(2, plugin.Server.GetPlayers(Role.NTF_LIEUTENANT).Count + " TARGETS REMAINING", false);
				if (plugin.Server.GetPlayers(Role.NTF_LIEUTENANT).Count <= 5 && spawncount < 5)
				{
					spawncount++;
					List<Player> players = plugin.Server.GetPlayers();

					foreach (Player player in plugin.Server.GetPlayers(Role.SPECTATOR))
					{
						TaskForcespawn(player);
					}
				}


			}
		}
		public void OnPlayerJoin(PlayerJoinEvent ev)
		{
           if (plugin.Enabled && roundstart == true)
			{
				TaskForcespawn(ev.Player);
			}
		}
		public void OnReload(PlayerReloadEvent ev)
		{
			if (plugin.Enabled && Juggernaut.PlayerId == ev.Player.PlayerId)
			{
				ev.Player.SetAmmo(AmmoType.DROPPED_7, 200);
			}
			else if (plugin.Enabled && Juggernaut.PlayerId != ev.Player.PlayerId)
			{
				ev.Player.SetAmmo(AmmoType.DROPPED_5, 40);
				ev.Player.SetAmmo(AmmoType.DROPPED_9, 50);
			}
		}
	}
}
