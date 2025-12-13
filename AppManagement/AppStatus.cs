using GlobalShared;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppManagement
{
    public class AppStatus
    {
        public AppStatus() { }

        static Utils utils = new Utils("AppStatus");

        public static ApplicationStatus CurrentStatus { get; private set; } = ApplicationStatus.Loading;

        public static void UpdateStatus(ApplicationStatus newStatus)
        {
            utils.LogInfo($"Updating status from {CurrentStatus} to {newStatus}");
            CurrentStatus = newStatus;
        }

    }
}

public enum ApplicationStatus
{
    Loading,
    CustomerPresent,
    Supervisor,
    InserviceIdle,
    OOSIdle
}