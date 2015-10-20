using Autofac;
using Research.Core.Infrastructure;
using Research.Core.Infrastructure.DependencyManagement;

namespace Research.Plugin.Shipping.CanadaPost
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            System.Diagnostics.Debug.WriteLine("Da chay den day");
        }

        public int Order
        {
            get { return 3; }
        }
    }
}
