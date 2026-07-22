//                               .-') _      .-') _    ('-.    .-. .-')    ('-.                 _ (`-.  
//                              ( OO ) )    ( OO ) )  ( OO ).-.\  ( OO ) _(  OO)               ( (OO  ) 
//   ,----.     .-'),-----. ,--./ ,--,' ,--./ ,--,'   / . --. / ;-----.\(,------. ,--. ,--.   _.`     \ 
//  '  .-./-') ( OO'  .-.  '|   \ |  |\ |   \ |  |\   | \-.  \  | .-.  | |  .---' |  | |  |  (__...--'' 
//  |  |_( O- )/   |  | |  ||    \|  | )|    \|  | ).-'-'  |  | | '-' /_)|  |     |  | | .-') |  /  | | 
//  |  | .--, \\_) |  |\|  ||  .     |/ |  .     |/  \| |_.'  | | .-. `.(|  '--.  |  |_|( OO )|  |_.' | 
// (|  | '. (_/  \ |  | |  ||  |\    |  |  |\    |    |  .-.  | | |  \  ||  .--'  |  | | `-' /|  .___.' 
//  |  '--'  |    `'  '-'  '|  | \   |  |  | \   |    |  | |  | | '--'  /|  `---.('  '-'(_.-' |  |      
//   `------'       `-----' `--'  `--'  `--'  `--'    `--' `--' `------' `------'  `-----'    `--'     

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoodsmanKompasExporter.Models
{
    public enum KompasDocumentType
    {
        Unknown,
        Part3D,
        Assembly3D,
        Drawing2D,
        Specification,
        Fragment2D,
        Text
    }
}
