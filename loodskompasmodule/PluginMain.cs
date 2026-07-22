using Ascon.Plm.Loodsman.PluginSDK;
using LoodsmanKompasExporter.Services;

namespace LoodsmanBrowser2000
{
    [LoodsmanPlugin]
    internal class Plugin : ILoodsmanNetPlugin
    {
        public void BindMenu(IMenuDefinition menu)
        {
            menu.AddMenuItem("LoodsmanKompasExporter#Export", Start, CanStart);
        }
        private void Start(INetPluginCall npc)
        {
            var workflow = new ExportWorkflowService(npc);
            workflow.Run();
        }
        private bool CanStart(INetPluginCall npc)
        {
            return true;
        }

        public void OnCloseDb()
        {

        }

        public void OnConnectToDb(INetPluginCall npc)
        {

        }

        public void PluginLoad()
        {

        }

        public void PluginUnload()
        {

        }
    }
}
