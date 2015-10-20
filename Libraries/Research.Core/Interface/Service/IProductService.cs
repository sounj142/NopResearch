using System;
using System.Collections.Generic;
using Research.Core;
using Research.Core.Domain.Catalog;

namespace Research.Core.Interface.Service
{
    public partial interface IProductService
    {
        Product GetById(int id);
    }
}
