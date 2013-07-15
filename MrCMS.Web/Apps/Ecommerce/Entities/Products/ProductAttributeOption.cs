﻿using System.Collections.Generic;
using MrCMS.Entities;
using MrCMS.Web.Apps.Ecommerce.Pages;

namespace MrCMS.Web.Apps.Ecommerce.Entities.Products
{
    public class ProductAttributeOption : SiteEntity
    {
        public ProductAttributeOption()
        {
            Products = new List<Product>();
            Values = new List<ProductAttributeValue>();
        }
        public virtual string Name { get; set; }
        public virtual IList<Product> Products { get; set; }
        public virtual IList<ProductAttributeValue> Values { get; set; }
        public virtual int DisplayOrder { get; set; }
    }
}