using ETH_HUNTER;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETH_Generator.Controllers
{
    public class DatabaseController
    {
        private SQLiteConnection con;
        DatabaseController() {
            con = new SQLiteConnection(Paths.database_connection);
        }
        
        

    }
}
