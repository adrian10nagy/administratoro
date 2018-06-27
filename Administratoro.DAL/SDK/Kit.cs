
namespace Administratoro.DAL.SDK
{
    using System;

    public interface IKit : IDisposable
    {
        // Users Users { get; }
    }

    public class Kit : IKit
    {
        private static Kit _instance = new Kit();
        public static Kit Instance
        {
            get
            {
                return _instance ?? getInstance();
            }
        }

        private static Kit getInstance()
        {
            return new Kit();
        }

        public SDK.Localities Localities { get { return new SDK.Localities(); } }
        public ErrorLogs ErrorLogs { get { return new ErrorLogs(); } }
        public SDK.Partners Partners { get { return new SDK.Partners(); } }
        public SDK.Apartments Apartments { get { return new SDK.Apartments(); } }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
