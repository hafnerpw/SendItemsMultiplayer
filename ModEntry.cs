using System.Linq;
using StardewModdingAPI.Events;
using StardewModdingAPI;
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
        }
        
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || e.Button != _config.SendHotkey || Game1.player.CurrentItem == null || !Game1.player.CanMove)
                return;

            var choices = Game1.getOnlineFarmers()
                .Where(f => f.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
                .Select(farmer => new Response(farmer.UniqueMultiplayerID.ToString(), farmer.Name)).ToList();
            if (choices.Count == 0)
            {
                Game1.addHUDMessage(new HUDMessage("You can't use this mod when you're alone man", 3));
                return;
            }

            choices.Add(new Response("", "Cancel Trade"));
            Game1.currentLocation.createQuestionDialogue($"Send '{Game1.player.CurrentItem.Name}' to...", choices.ToArray(), DialogueSet);
        }
        
        private void DialogueSet(Farmer who, string dialogueId)
        {
            if (string.IsNullOrWhiteSpace(dialogueId)) return;
            
            var l1 = long.Parse(dialogueId);
            var receiver = Game1.getOnlineFarmers().First(f => f.UniqueMultiplayerID == l1);

            var gift = who.CurrentItem.getOne();
            gift.Stack = who.CurrentItem.Stack;
            
            who.team.SendProposal(receiver, ProposalType.Gift, gift);
            Game1.activeClickableMenu = new PendingProposalDialog();
        }
    }
}