using GlobalShared;

namespace AppManagement
{
    public class CustomerApplication
    {
        private Utils utils = new Utils("CustomerApplication");

        public CustomerApplication()
        {
            utils.LogInfo("CustomerApplication initialized.");
        }

        public void Start()
        {
            utils.LogInfo("CustomerApplication has started.");
        }

        public void Idle()
        {
            utils.LogInfo("CustomerApplication is now idle.");
        }
    }
}
