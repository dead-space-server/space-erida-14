using Content.Client.UI.HealthAnalyzer;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.UI
{
    public sealed class HealthAnalyzerWindow : Window
    {
        private TabContainer _tabContainer;

        public HealthAnalyzerWindow()
        {
            // Контейнер вкладок
            _tabContainer = new TabContainer();
            Content = _tabContainer;

            // Основной экран анализа здоровья
            var mainHealthScreen = new MainHealthScreen();
            _tabContainer.AddChild(mainHealthScreen);

            // Добавляем вкладку с информацией о толерантности
            var drugToleranceTab = new HealthAnalyzerTab();
            _tabContainer.AddChild(drugToleranceTab);
        }
    }
}
