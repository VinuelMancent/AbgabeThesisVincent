using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Trumpf.Sys.NetRemoting;

namespace OrchestratorPlugin
{
    public class Plugin : TcRemoteObject, TiObjectCallback
    {
        private TiObject o;

        public Plugin()
        {
            o = (TiObject)TcGlobalObjectFactory.Factory;
            o.RegisterCallback(this);
        }

        public async void StateChanged(TeState state)
        {
            if (state == TeState.PRODUCING)
            {
                HttpClient httpClient = new HttpClient();
                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync("http://localhost:4097/verifyServices");
                }
                catch(Exception e)
                {
                    Console.WriteLine($"Unable to send request to orchestrator. Reason: {e.Message}");
                }
                
                o.UnRegisterCallback(this);
            }
        }
    }
}
