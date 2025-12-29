using GlobalShared;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppCommon
{
    public class ApplicationState
    {
        public ApplicationState() { }

        public ApplicationStatus CurrentStatus { get; set; } = ApplicationStatus.Loading;
        Action<ApplicationStatus, ApplicationStatus> AppLicationStatusChanged = null;
        Func<ApplicationStatus, ApplicationStatus, bool> AppLicationStatusBeforeChange = null;

        private readonly Utils utils = new Utils("AppState");

        public void UpdateStatus(ApplicationStatus newStatus)
        {
            utils.LogInfo($"Updating status from {CurrentStatus} to {newStatus}");
            bool? changestatus = null;
            changestatus = AppLicationStatusBeforeChange.Invoke(CurrentStatus, newStatus);
            if (changestatus.HasValue && !changestatus.Value)
            {
                utils.LogInfo($"Status Cahange from {CurrentStatus} to {newStatus} was canceled");
                return;
            }
            var temp = CurrentStatus;
            CurrentStatus = newStatus;
            AppLicationStatusChanged?.Invoke(temp, newStatus);
        }
    }


    public enum ApplicationStatus
    {
        Loading,
        OOS,
        Inservice,
        CustomerPresent,
        Supervisor,
        Dygnostic
    }
}
