
namespace Administratoro.DAL.SDK
{
    using System;

    public interface IKit : IDisposable
    {
        // Users Users { get; }
    }

    public class Kit : IKit
    {
        private static readonly Kit _instance = new Kit();
        public static Kit Instance
        {
            get
            {
                return _instance ?? GetInstance();
            }
        }

        private static Kit GetInstance()
        {
            return new Kit();
        }

        public Localities Localities { get { return new Localities(); } }
        public ErrorLogs ErrorLogs { get { return new ErrorLogs(); } }
        public Partners Partners { get { return new Partners(); } }
        public Apartments Apartments { get { return new Apartments(); } }
        public Documents Documents { get { return new Documents(); } }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
