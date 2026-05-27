using Microsoft.Xna.Framework;
using MortalDao.Content.Items.Specials;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace MortalDao.Content.Tiles
{
    public class Bloody_Chest : ModTile
    {
        public static LocalizedText LockedText { get; private set; }

        public override void SetStaticDefaults()
        {
            Main.tileContainer[Type] = true;//可以放置容器
            Main.tileShine2[Type] = true;//发光
            Main.tileShine[Type] = 1200;
            Main.tileFrameImportant[Type] = true;//多格家具必须开启，否则会显示成一格一格的
            Main.tileNoAttach[Type] = true;//不与其他物块连接
            Main.tileOreFinderPriority[Type] = 500;
            TileID.Sets.BasicChest[Type] = true;
            TileID.Sets.DisableSmartCursor[Type] = true;
            TileID.Sets.AvoidedByNPCs[Type] = true;//NPC回避
            TileID.Sets.IsAContainer[Type] = true;//可以放置容器
            TileID.Sets.GeneralPlacementTiles[Type] = false;
            TileID.Sets.HasOutlines[Type] = true;
            DustType = DustID.Blood;
            AdjTiles = [TileID.Containers];
            VanillaFallbackOnModDeletion = TileID.Containers; //模组被禁用时使用宝箱ai
            AddMapEntry(new Color(255, 0, 0), this.GetLocalization("MapEntry0"), MapChestName);
            AddMapEntry(new Color(180, 0, 0), this.GetLocalization("MapEntry1"), MapChestName);

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.Origin = new Point16(0, 1);//设定左下角的的方块为原点
            TileObjectData.newTile.CoordinateHeights = [16, 18];
            TileObjectData.newTile.StyleHorizontal = true;//水平放置
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(Chest.FindEmptyChest, -1, 0, true);//检查箱子数据
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(Chest.AfterPlacement_Hook, -1, 0, false);//创建箱子数据
            TileObjectData.newTile.AnchorInvalidTiles = [ //无法放置在以下物块上
                TileID.MagicalIceBlock,
                TileID.Boulder,
                TileID.BouncyBoulder,
                TileID.LifeCrystalBoulder,
                TileID.RollingCactus
            ];
            TileObjectData.newTile.AnchorBottom = new AnchorData(Terraria.Enums.AnchorType.SolidTile | Terraria.Enums.AnchorType.SolidWithTop | Terraria.Enums.AnchorType.SolidSide, TileObjectData.newTile.Width, 0);//可以放置的位置
            TileObjectData.addTile(Type);

            LockedText = this.GetLocalization("Locked");
        }

        public override ushort GetMapOption(int i, int j) //获取箱子类型
        {
            int option = Main.tile[i, j].TileFrameX / 36;
            if (option < 0) option = 0;
            if (option > 1) option = 1;
            return (ushort)option;
        }

        public override LocalizedText DefaultContainerName(int frameX, int frameY) //箱子名字
        {
            int option = frameX / 36;
            return this.GetLocalization("MapEntry" + option);
        }

        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) //可以交互
        {
            return true;
        }

        public override bool IsLockedChest(int i, int j) //是否是锁着的箱子
        {
            return Main.tile[i, j].TileFrameX / 36 == 1;
        }

        public override bool UnlockChest(int i, int j, ref short frameXAdjustment, ref int dustType, ref bool manual) 
        {
            DustType = DustID.Blood;
			return true;
        }
        public static string MapChestName(string name, int i, int j) 
        {
            int left = i;
            int top = j;
            Tile tile = Main.tile[i, j];//定位左上格
            if (tile.TileFrameX % 36 != 0)
            {
                left--;
            }
            if (tile.TileFrameY != 0)
            {
                top--;
            }
            int chest = Chest.FindChest(left, top);
            if (chest < 0)
            {
                return Language.GetTextValue("LegacyChestType.0");
            }
            if (Main.chest[chest].name == "")
            {
                return name;
            }
            return name + ": " + Main.chest[chest].name;
        }
        public override void NumDust(int i, int j, bool fail, ref int num) 
        {
			num = 3;
		}
        public override void KillMultiTile(int i, int j, int frameX, int frameY) 
        {
            Chest.DestroyChest(i, j);
        }
        public override bool RightClick(int i, int j) 
        {
            Player player = Main.LocalPlayer;
            Tile tile = Main.tile[i, j];
            Main.mouseRightRelease = false;
            int left = i;
            int top = j;
            if (tile.TileFrameX % 36 != 0)
            {
                left--;
            }
            if (tile.TileFrameY != 0)
            {
                top--;
            }
            player.CloseSign();//关闭玩家当前正在编辑/查看的牌子（Sign）界面
            player.SetTalkNPC(-1);//结束与 NPC 的对话状态（-1 表示不和任何 NPC 对话）
            Main.npcChatCornerItem = 0;//清除 NPC 聊天角色的物品
            Main.npcChatText = "";//清除 NPC 聊天文本
            if (Main.editChest) //编辑箱子
            {
				SoundEngine.PlaySound(SoundID.MenuTick);
				Main.editChest = false;
				Main.npcChatText = string.Empty;
			}

			if (player.editedChestName) //编辑箱子名字
            {
				NetMessage.SendData(MessageID.SyncPlayerChest, text: NetworkText.FromLiteral(Main.chest[player.chest].name), number: player.chest, number2: 1f);
				player.editedChestName = false;
			}
            bool isLocked = Chest.IsLocked(left, top);//检查箱子是否被锁定
            if (Main.netMode == NetmodeID.MultiplayerClient && !isLocked)
            {
                if (left == player.chestX && top == player.chestY && player.chest != -1) 
                {
                    player.chest = -1;
                    Recipe.FindRecipes();
                    SoundEngine.PlaySound(SoundID.MenuClose);
                }
                else
                {
                    NetMessage.SendData(MessageID.RequestChestOpen, number: left, number2: top);
                    Main.stackSplit = 600;
                }
            }
            else
            {
                if (isLocked)
                {
                    int key = ModContent.ItemType<Bloody_key>();
                    if (player.HasItemInInventoryOrOpenVoidBag(key) && Chest.Unlock(left, top) && player.ConsumeItem(key, includeVoidBag: true))
                    {
                        if (Main.netMode == NetmodeID.MultiplayerClient)
                        {
                            NetMessage.SendData(MessageID.LockAndUnlock, number2: 1f, number3: left, number4: top);
                        }
                    }
                }
                else
                {
                    int chest = Chest.FindChest(left, top);
					if (chest != -1) {
						Main.stackSplit = 600;
						if (chest == player.chest) {
							player.chest = -1;
							SoundEngine.PlaySound(SoundID.MenuClose);
						}
						else 
                        {
							SoundEngine.PlaySound(player.chest < 0 ? SoundID.MenuOpen : SoundID.MenuTick);
							player.OpenChest(left, top, chest);
						}
                        Recipe.FindRecipes();
					}
				}
			}
			return true;
        }
        public override void MouseOver(int i, int j) {
			Player player = Main.LocalPlayer;
			Tile tile = Main.tile[i, j];
			int left = i;
			int top = j;
			if (tile.TileFrameX % 36 != 0) {
				left--;
			}

			if (tile.TileFrameY != 0) {
				top--;
			}

			int chest = Chest.FindChest(left, top);
			player.cursorItemIconID = -1;
			if (chest < 0) {
				player.cursorItemIconText = Language.GetTextValue("LegacyChestType.0");
			}
			else {
				string defaultName = TileLoader.DefaultContainerName(tile.TileType, tile.TileFrameX, tile.TileFrameY); // This gets the ContainerName text for the currently selected language
				player.cursorItemIconText = Main.chest[chest].name.Length > 0 ? Main.chest[chest].name : defaultName;
				if (player.cursorItemIconText == defaultName) {
					player.cursorItemIconID = ItemID.Chest;
					if (Main.tile[left, top].TileFrameX / 36 == 1) {
						player.cursorItemIconID = ModContent.ItemType<Bloody_key>();
					}

					player.cursorItemIconText = "";
				}
			}

			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
		}

		public override void MouseOverFar(int i, int j) {
			MouseOver(i, j);
			Player player = Main.LocalPlayer;
			if (player.cursorItemIconText == "") {
				player.cursorItemIconEnabled = false;
				player.cursorItemIconID = ItemID.None;
			}
		}
    }
}