using System.Security.Cryptography;
using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json.Linq;
using REviewer.Modules.RE;
using REviewer.Modules.Utils;

namespace REviewer.Modules.Forms
{
    public partial class Race
    {
                
        private void Update_Character_State(object sender, EventArgs e) => InvokeUI(() =>
        {
            CheckCharacterHealthState(_game.Player.CharacterHealthState.Value);
        });

        private void Updated_Debug(object sender, EventArgs e) => InvokeUI(() =>
        {
            _raceDatabase.Debugs = CheckDebugWindow(_game.Rebirth.Debug.Value) == "Active" ? _raceDatabase.Debugs + 1 : _raceDatabase.Debugs;
            labelDebug.Text = _raceDatabase.Debugs.ToString();
        });

        private async void UpdateInventorySlotSelectedPicture()
        {
            // Wait 100 ms to avoid flickering

            await Task.Delay(50);
            var islot = (int)_game.Player.InventorySlotSelected.Value;

            var selected = (int)_game.Player.InventorySlotSelected.Value - 1;
            byte id = (int)_game.Player.InventorySlotSelected.Value == 0 ? (byte)0 : (byte)_game.Inventory.Slots[selected].Item.Value;

            var image = _itemDatabase.GetPropertyById(id).Img;
            pictureBoxItemSelected.Image = image;
            _previousSelectedSlot = islot;
        }

        private void Updated_Inventory_Capacity(object sender, EventArgs e) => InvokeUI(() =>
        {
            Logger.Instance.Debug($"Inventory capacity: {_game.Inventory.Capacity.Value} -> {_game.Inventory.Capacity.Value & 3}");
            CheckInventoryCapacity(_game.Inventory.Capacity.Value, pictureBoxItemSlot7, pictureBoxItemSlot8, labelSlot7Quantity, labelSlot8Quantity);
        });

        private void Updated_InventorySlotSelected(object sender, EventArgs e) => InvokeUI(() =>
        {
            UpdateInventorySlotSelectedPicture();
        });

        

        private static void UpdateKeyItemPicture(PictureBox pictureBox, KeyItem keyItem, int state = 0)
        {
            pictureBox.Image = keyItem.Data.Img;

            pictureBox.BackColor = state switch
            {
                0 => Color.Transparent,
                1 => CustomColors.Orange,
                2 => CustomColors.Default,
                _ => pictureBox.BackColor // Default case
            };
        }


        private void Updated_LastItemFound(object sender, EventArgs e) => InvokeUI(() =>
        {
            UpdateLastItemFoundPicture();
        });

        private void UpdateLastItemFoundPicture()
        {
            byte value = (byte)_game.Player.LastItemFound.Value;
            if (_itemDatabase.GetPropertyById(value).Type == "Key Item") // && _raceDatabase.FullRoomName == null)
            {
                _raceDatabase.Stage = (((int)_game.Player.Stage.Value % 5) + 1).ToString();
                _raceDatabase.Room = ((int)_game.Player.Room.Value).ToString("X2");
                _raceDatabase.FullRoomName = _raceDatabase.Stage + _raceDatabase.Room;
                UpdateRaceKeyItem(value, _raceDatabase.FullRoomName, 1);
            }

            byte item_id = (byte)_game.Player.LastItemFound.Value;
            pictureBoxLastItem.Image = _itemDatabase.GetPropertyById(item_id).Img;
        }

        
        private void UpdateRaceKeyItem(int value, string room, int state, bool force = false)
        {
            value = GetKeyItemPosition(value, room, state);

            // If 'force' is true or the current state is less than the new state, update the state and picture
            if (force || _raceDatabase.KeyItems[value].State < state)
            {
                Logger.Instance.Info($"Updating key item {value} to state {state} in room {room}");
                _raceDatabase.KeyItems[value].State = state;
                UpdatePictureKeyItemState(value);
            }

            _raceDatabase.KeyItems[value].Room = room;
        }

        private void Updated_Reset(object sender, EventArgs e) => InvokeUI(() =>
        {
            int health = Int32.Parse(labelHealth.Text);


            if (_game.Game.MainMenu.Value == 1 && health != 0 && !labelGameCompleted.Visible && (health <= _healthTable[(byte)_game.Player.Character.Value][0]))
            {
                _raceDatabase.Resets += 1;
                labelResets.Text = _raceDatabase.Resets.ToString();
            }

            buttonReset.Enabled = _game.Game.MainMenu.Value == 1;
            Logger.Instance.Info($"Main Menu -> {_game.Game.MainMenu.Value}");
        });

        
        private void UpdatePictureKeyItemState(int value)
        {
            var state = _raceDatabase.KeyItems[value].State;

            // change background color of the picture box
            _pictureKeyItems[value].BackColor = state switch
            {
                -1 => Color.Transparent,
                0 => Color.Transparent,
                1 => CustomColors.Orange,
                2 => CustomColors.Default,
                _ => Color.Transparent // Default case
            };
        }

        private void UpdateSlotPicture(int value)
        {
            if (value > _inventoryCapacitySize)
            {
                return;
            }

            var item_id = _game.Inventory.Slots[value].Item.Value;
            // if value is key item, update the key item
            if (_itemDatabase.GetPropertyById((byte)_game.Inventory.Slots[value].Item.Value).Type == "Key Item")
            {
                if ((_game.Game.State.Value & 0x0000FF00) == 0x8800)
                {
                    UpdateRaceKeyItem(item_id, _raceDatabase.FullRoomName, 2);
                }
            }

            var id = (byte)GetItemByPosition(_game.Inventory.Slots[value].Item.Value);
            var image = _itemDatabase.GetPropertyById(id).Img;
            _pictures[value].Image = image;

            UpdateSlotCapacity(_slotLabels[value], value);
        }
        private void UpdateSlotCapacity(Label label, int value) => InvokeUI(() =>
        {
            if (value > _inventoryCapacitySize)
            {
                return;
            }

            Property item = _itemDatabase.GetPropertyById((byte)_game.Inventory.Slots[value].Item.Value);
            label.Text = _game.Inventory.Slots[value].Quantity.Value.ToString();
            label.Font = _pixelBoySegments;
            label.Visible = !_itemTypes.Contains(item.Type);
            label.ForeColor = item.Color switch
            {
                "Yellow" => CustomColors.Yellow,
                "Orange" => CustomColors.Orange,
                "Red" => CustomColors.Red,
                _ => CustomColors.Default,
            };
        });


        private void Updated_Health(object sender, EventArgs e) => InvokeUI(() =>
        {
            CheckHealthLabel(_game.Player.Health.Value);
        });

        private void Updated_Character(object sender, EventArgs e) => InvokeUI(() =>
        {
            byte value = (byte)_game.Player.Character.Value;
            labelCharacter.Text = ((Dictionary<byte, string>)_game.Player.Character.Database)[value].ToString();
        });

        private void Updated_Stage(object sender, EventArgs e) => InvokeUI(() => _raceDatabase.Stage = ((_game.Player.Stage.Value % 5) + 1).ToString());

        private void Updated_Room(object sender, EventArgs e) => InvokeUI(() =>
        {
            _raceDatabase.Room = _game.Player.Room.Value.ToString("X2");
            _raceDatabase.LastRoomName = _raceDatabase.FullRoomName;
            _raceDatabase.FullRoomName = _raceDatabase.Stage + _raceDatabase.Room;

            UpdateKeyRooms();
        });

        private void UpdateKeyRooms()
        {
            string fullRoomName = _raceDatabase.FullRoomName;
            string lastRoomName = _raceDatabase.LastRoomName ?? fullRoomName;

            foreach (var roomName in new[] { lastRoomName, fullRoomName })
            {
                if (_raceDatabase.KeyRooms.TryGetValue(roomName, out List<string>? value))
                {
                    List<string> keyRooms = value;
                    string otherRoomName = roomName == lastRoomName ? fullRoomName : lastRoomName;

                    if (!keyRooms.Contains(otherRoomName) && otherRoomName != roomName)
                    {
                        keyRooms.Add(otherRoomName);

                        if (keyRooms.Count == 2)
                        {
                            UpdateChronometers();
                            _raceDatabase.KeyRooms.Remove(roomName);
                        }
                    }

                    // Update the list in the dictionary
                    _raceDatabase.KeyRooms[roomName] = keyRooms;
                }
            }
        }

        private void UpdateChronometers()
        {
            _segmentWatch[_raceDatabase.Segments].Stop();
            _raceDatabase.Segments += 1;
            _segmentWatch[_raceDatabase.Segments].Start();
        }

        private void Updated_SaveState(object sender, EventArgs e) => InvokeUI(() =>
        {            
            int number = _game.Game.SaveContent.Value;
            int high = number >> 16;
            int low = number & 0xFFFF;

            if ((low & 0xFFFF) == 0xADDE)
            {
                LoadState(high);

            }
            else if ((high & 0xFFFF00000) == 0xDEAD)
            {
                LoadState(low);
            }
        });

        private void Updated_GameState(object sender, EventArgs e) => InvokeUI(() =>
        {
            int state = _game.Game.State.Value;

            //if ((state & 0xFF000000) == 0x54000000)
            //{
            //    LoadState();
            //}

            if ((state & 0x0FFF0000) == 0x1000000 && _raceDatabase.PreviousState != 0x1000000)
            {
                _raceDatabase.Deaths += 1;
                labelHealth.Text = "255";
                labelDeaths.Text = _raceDatabase.Deaths.ToString();
            }
            else if ((state & 0x0FFF0000) == 0x1100000 && _raceDatabase.PreviousState != 0x2000000)
            {
                labelHealth.Text = "WP!";
                labelGameCompleted.Visible = true;
                _raceWatch.Stop();
                _segmentWatch[_raceDatabase.Segments].Stop();
            }

            _raceDatabase.PreviousState = state & 0x0F000000;
        });

        private void Updated_SaveBuffer(object sender, EventArgs e) => InvokeUI(() =>
        {
            _raceDatabase.RealItembox = GetItemBoxData();
        });

        
        private void Updated_Timer(object sender, EventArgs e) => InvokeUI(() =>
        {
            labelTimer.Text = _raceWatch.Elapsed.ToString(@"hh\:mm\:ss\.ff");

            Label[] labelSegTimers = [labelSegTimer1, labelSegTimer2, labelSegTimer3];

            if (_raceDatabase.Segments >= 0 && _raceDatabase.Segments < labelSegTimers.Length)
            {
                labelSegTimers[_raceDatabase.Segments].Text = _segmentWatch[_raceDatabase.Segments].Elapsed.ToString(@"hh\:mm\:ss\.ff");
            }

            int currentTimerValue = _game.Game.Timer.Value;

            if (_previousTimerValue == currentTimerValue)
            {
                return;
            }
            if (_raceWatch.IsRunning)
            {
                _raceWatch.Stop();
                _segmentWatch[_raceDatabase.Segments].Stop();
            }
            else
            {
                _raceWatch.Start();
                _segmentWatch[_raceDatabase.Segments].Start();
            }
        });


    }
}