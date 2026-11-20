namespace SurvivalGame.Core
{
    public interface IInteractable
    {
        string GetInteractionPrompt();
        void Interact(SurvivalGame.Player.PlayerController player);
    }
}
