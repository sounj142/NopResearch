using Autofac;
using Autofac.Core;
using Research.Admin.Controllers;
using Research.Core.Caching;
using Research.Core.Infrastructure;
using Research.Core.Infrastructure.DependencyManagement;

namespace Research.Admin.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            
        }

        public int Order
        {
            get { return 2; }
        }
    }
}