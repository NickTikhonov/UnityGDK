using Improbable.Gdk.Core;

namespace Improbable.Gdk.Mobile
{
    public abstract class MobileWorkerConnectorBase : WorkerConnectorBase
    {
        protected abstract string GetHostIp();

        protected override ReceptionistConfig GetReceptionistConfig(string workerType)
        {
            return new ReceptionistConfig
            {
                ReceptionistHost = GetHostIp(),
                WorkerType = workerType,
                WorkerId = CreateNewWorkerId(workerType),
                UseExternalIp = UseExternalIp
            };
        }
    }
}
