﻿using System.Linq;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace SendItemsMultiplayer
{
    public class ModEntry : Mod
    {
        private ModConfig _config;

        public override void Entry(IModHelper helper)
        {
            _config = Helper.ReadConfig<ModConfig>();

            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.Display.MenuChanged += OnMenuChanged;
            helper.Events.GameLoop.OneSecondUpdateTicked += OnSecondTicked;
        }

        private void OnSecondTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (!e.IsMultipleOf(210) || !Context.IsWorldReady || Game1.player.health >= Game1.player.maxHealth || (Game1.buffsDisplay.food is null && Game1.buffsDisplay.drink is null)) return;
            
            Game1.player.health += 1;
        }


        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || e.Button != _config.SendHotkey || Game1.player.CurrentItem == null ||
                !Game1.player.CanMove)
                return;

            var choices = Game1.getOnlineFarmers()
                .Where(f => f.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
                .Select(farmer => new Response(farmer.UniqueMultiplayerID.ToString(), farmer.Name)).ToList();
            
            if (choices.Count == 0)
            {
                Game1.addHUDMessage(new HUDMessage("You can't use this mod when you're alone man", 3));
                return;
            }

            choices.Add(new Response("", "Cancel Trade"){hotkey = Keys.Escape});
            var question = Game1.player.CurrentItem.Stack == 1
                ? $"Send '{Game1.player.CurrentItem.DisplayName}' to..."
                : $"Send {Game1.player.CurrentItem.Stack} '{Game1.player.CurrentItem.DisplayName}' to...";
            Game1.currentLocation.createQuestionDialogue(question, choices.ToArray(), DialogueSet);
        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (!Context.IsWorldReady || !Game1.player.hasMenuOpen.Value) return;
            
            var dialogue = Game1.activeClickableMenu as DialogueBox;
            if (dialogue is not { isQuestion: true }) return;

            for (var i = 0; i < dialogue.responses.Count - 1; i++)
            {
                dialogue.responses[i].hotkey = Keys.D1 + i;
                dialogue.responses[i].responseText = $"{i}. {dialogue.responses[i].responseText}";
            }
            
            dialogue.responses[^1].hotkey = Keys.Escape;
            dialogue.responses[^1].responseText = "ESC " + dialogue.responses[^1].responseText;
        }

        private static void DialogueSet(Farmer who, string dialogueId)
        {
            if (string.IsNullOrWhiteSpace(dialogueId)) return;

            var l1 = long.Parse(dialogueId);
            var receiver = Game1.getOnlineFarmers().First(f => f.UniqueMultiplayerID == l1);

            var gift = who.CurrentItem.getOne();
            gift.Stack = who.CurrentItem.Stack;

            who.team.SendProposal(receiver, ProposalType.Gift, gift);
            Game1.activeClickableMenu = new MyGiftDialog();
        }
    }
}