using Content.Server.Drugs.Components;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.GameObjects;
using Robust.Client.Graphics;

namespace Content.Client.UI.HealthAnalyzer
{
    public sealed class HealthAnalyzerTab : TabContainer.TabPage
    {
        private Label _drugToleranceLabel;

        public HealthAnalyzerTab()
        {
            Title = "Толерантность";
            _drugToleranceLabel = new Label();
            AddChild(_drugToleranceLabel);

            // Обновляем интерфейс каждые 1 секунду
            UpdateTimer.Instance.Every(1000, () => UpdateUI());
        }

        private void UpdateUI()
        {
            var playerEnt = Engine.Host.Instance.Session.AttachedEntity;
            if (playerEnt == null || !playerEnt.HasComponent<DrugToleranceComponent>())
                return;

            var toleranceComp = playerEnt.GetComponent<DrugToleranceComponent>();

            var info = "";
            foreach (var entry in toleranceComp.Tolerances)
            {
                info += $"{entry.Key}: {entry.Value}% ";
            }

            _drugToleranceLabel.Text = info;
        }
    }
}