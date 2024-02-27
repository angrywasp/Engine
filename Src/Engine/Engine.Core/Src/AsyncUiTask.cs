using System;
using System.Threading;
using System.Threading.Tasks;
using AngryWasp.Logger;
using Microsoft.Xna.Framework;

namespace Engine
{
    public class AsyncUiTask
    {
        private EventWaitHandle waitHandle;
        
        public async Task Run(Action action)
        {
            if (Threading.IsOnUIThread())
            {
                action();
                return;
            }

            this.waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

            Threading.BlockOnUIThread(() => 
            {
#if DEBUG
                try
                {
#endif
                    action();
                    this.waitHandle.Set();
#if DEBUG
                }
                catch (Exception ex)
                {
                    Log.Instance.WriteFatalException(ex);
                }
#endif

            });

            await Task.Run(() => waitHandle.WaitOne());
        }
    }
}
