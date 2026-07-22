//                               .-') _      .-') _    ('-.    .-. .-')    ('-.                 _ (`-.  
//                              ( OO ) )    ( OO ) )  ( OO ).-.\  ( OO ) _(  OO)               ( (OO  ) 
//   ,----.     .-'),-----. ,--./ ,--,' ,--./ ,--,'   / . --. / ;-----.\(,------. ,--. ,--.   _.`     \ 
//  '  .-./-') ( OO'  .-.  '|   \ |  |\ |   \ |  |\   | \-.  \  | .-.  | |  .---' |  | |  |  (__...--'' 
//  |  |_( O- )/   |  | |  ||    \|  | )|    \|  | ).-'-'  |  | | '-' /_)|  |     |  | | .-') |  /  | | 
//  |  | .--, \\_) |  |\|  ||  .     |/ |  .     |/  \| |_.'  | | .-. `.(|  '--.  |  |_|( OO )|  |_.' | 
// (|  | '. (_/  \ |  | |  ||  |\    |  |  |\    |    |  .-.  | | |  \  ||  .--'  |  | | `-' /|  .___.' 
//  |  '--'  |    `'  '-'  '|  | \   |  |  | \   |    |  | |  | | '--'  /|  `---.('  '-'(_.-' |  |      
//   `------'       `-----' `--'  `--'  `--'  `--'    `--' `--' `------' `------'  `-----'    `--'     

using Ascon.Plm.Loodsman.PluginSDK;
using LoodsmanKompasExporter;
using LoodsmanKompasExporter.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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
