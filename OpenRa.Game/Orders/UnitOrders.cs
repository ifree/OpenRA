﻿using System.Drawing;
using OpenRa.FileFormats;
using OpenRa.GameRules;
using OpenRa.Graphics;
using OpenRa.Traits;

namespace OpenRa.Orders
{
	static class UnitOrders
	{
		public static void ProcessOrder( Order order )
		{
			switch( order.OrderString )
			{
			case "PlaceBuilding":
				{
					Game.world.AddFrameEndTask( _ =>
					{
						var queue = order.Player.PlayerActor.traits.Get<ProductionQueue>();
						var unit = Rules.ActorInfo[ order.TargetString ];
						var producing = queue.CurrentItem(unit.Category);
						if( producing == null || producing.Item != order.TargetString || producing.RemainingTime != 0 )
							return;

						Game.world.CreateActor( order.TargetString, order.TargetLocation - Footprint.AdjustForBuildingSize( unit.Traits.Get<BuildingInfo>() ), order.Player );
						if (order.Player == Game.LocalPlayer)
						{
							Sound.Play("placbldg.aud");
							Sound.Play("build5.aud");
						}

						queue.FinishProduction(unit.Category);
					} );
					break;
				}
			case "Chat":
				{
					Game.chat.AddLine(order.Player, order.TargetString);
					break;
				}
			case "ToggleReady":
				{
					Game.chat.AddLine(order.Player, "is " + order.TargetString );
					break;
				}
			case "AssignPlayer":
				{
					Game.LocalPlayer = order.Player;
					Game.chat.AddLine(order.Player, "is now YOU.");
					break;
				}
			case "StartGame":
				{
					Game.chat.AddLine(Color.White, "Server", "The game has started.");
					Game.StartGame();
					break;
				}
			case "SyncInfo":
				{
					Game.SyncLobbyInfo(order.TargetString);
					break;
				}
			case "FileChunk":
				{
					PackageDownloader.ReceiveChunk(order.TargetString);
					break;
				}

			default:
				{
					foreach (var t in order.Subject.traits.WithInterface<IResolveOrder>())
						t.ResolveOrder(order.Subject, order);
					break;
				}
			}
		}
	}
}
