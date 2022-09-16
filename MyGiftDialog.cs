using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Menus;

namespace SendItemsMultiplayer
{
    public class MyGiftDialog : ConfirmationDialog
    {
        public MyGiftDialog()
            : base(Game1.content.LoadString("Strings\\UI:PendingProposal"), null)
        {
            okButton.visible = false;
            onCancel = CancelProposal;
            SetCancelable(true);
        }

        private void CancelProposal(Farmer who)
        {
            var outgoingProposal = Game1.player.team.GetOutgoingProposal();
            if (outgoingProposal?.receiver.Value == null ||
                !outgoingProposal.receiver.Value.isActive())
                return;
            outgoingProposal.canceled.Value = true;
            message = Game1.content.LoadString("Strings\\UI:PendingProposal_Canceling");
            SetCancelable(false);
        }

        private void SetCancelable(bool cancelable)
        {
            cancelButton.visible = cancelable;
            if (!Game1.options.SnappyMenus)
                return;
            populateClickableComponentList();
            snapToDefaultClickableComponent();
        }

        public override bool readyToClose() => false;

        private static bool ConsumesItem(ProposalType pt) => pt is ProposalType.Gift or ProposalType.Marriage;

        public override void update(GameTime time)
        {
            base.update(time);
            var outgoingProposal = Game1.player.team.GetOutgoingProposal();
            if (outgoingProposal?.receiver.Value == null || !outgoingProposal.receiver.Value.isActive())
            {
                Game1.player.team.RemoveOutgoingProposal();
                closeDialog(Game1.player);
            }
            else if (outgoingProposal.cancelConfirmed.Value &&
                     outgoingProposal.response.Value != ProposalResponse.Accepted)
            {
                Game1.player.team.RemoveOutgoingProposal();
                closeDialog(Game1.player);
            }
            else
            {
                if (outgoingProposal.response.Value == ProposalResponse.None) return;
                if (outgoingProposal.response.Value == ProposalResponse.Accepted)
                {
                    if (ConsumesItem(
                            (ProposalType)(NetFieldBase<ProposalType, NetEnum<ProposalType>>)outgoingProposal
                                .proposalType))
                        Game1.player.removeItemFromInventory(Game1.player.CurrentItem);
                    if (outgoingProposal.proposalType.Value == ProposalType.Dance)
                        Game1.player.dancePartner.Value = outgoingProposal.receiver.Value;
                    outgoingProposal.receiver.Value.doEmote(20);
                }

                Game1.player.team.RemoveOutgoingProposal();
                closeDialog(Game1.player);
                if (outgoingProposal.responseMessageKey.Value == null)
                    return;
                Game1.drawObjectDialogue(Game1.content.LoadString(outgoingProposal.responseMessageKey.Value,
                    outgoingProposal.receiver.Value.Name));
            }
        }
    }
}