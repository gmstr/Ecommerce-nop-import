﻿using System;
using System.Collections.Generic;
using System.Linq;
using MrCMS.Services;
using MrCMS.Web.Apps.Ecommerce.Entities.Products;
using MrCMS.Web.Apps.Ecommerce.Pages;
using MrCMS.Web.Apps.Ecommerce.Services.ImportExport.DTOs;
using MrCMS.Web.Apps.Ecommerce.Services.Products;

namespace MrCMS.Web.Apps.Ecommerce.Services.ImportExport
{
    public class ImportProductsService : IImportProductsService
    {
        private readonly IDocumentService _documentService;
        private readonly IBrandService _brandService;
        private readonly IImportProductSpecificationsService _importSpecificationsService;
        private readonly IImportProductVariantsService _importProductVariantsService;
        private readonly IImportProductImagesService _importProductImagesService;
        private readonly IImportProductUrlHistoryService _importUrlHistoryService;

        public ImportProductsService(IDocumentService documentService, IBrandService brandService,
             IImportProductSpecificationsService importSpecificationsService, IImportProductVariantsService importProductVariantsService,
            IImportProductImagesService importProductImagesService, IImportProductUrlHistoryService importUrlHistoryService)
        {
            _documentService = documentService;
            _brandService = brandService;
            _importSpecificationsService = importSpecificationsService;
            _importProductVariantsService = importProductVariantsService;
            _importProductImagesService = importProductImagesService;
            _importUrlHistoryService = importUrlHistoryService;
        }

        /// <summary>
        /// Do import
        /// </summary>
        /// <param name="productsToImport"></param>
        public void ImportProductsFromDTOs(IEnumerable<ProductImportDataTransferObject> productsToImport)
        {
            foreach (var dataTransferObject in productsToImport)
            {
                ImportProduct(dataTransferObject);
            }
        }

        /// <summary>
        /// Import from DTOs
        /// </summary>
        /// <param name="dataTransferObject"></param>
        public Product ImportProduct(ProductImportDataTransferObject dataTransferObject)
        {
            var product = _documentService.GetDocumentByUrl<Product>(dataTransferObject.UrlSegment) ?? new Product();

            product.Parent = _documentService.GetUniquePage<ProductSearch>();
            product.UrlSegment = dataTransferObject.UrlSegment;
            product.Name = dataTransferObject.Name;
            product.BodyContent = dataTransferObject.Description;
            product.MetaTitle = dataTransferObject.SEOTitle;
            product.MetaDescription = dataTransferObject.SEODescription;
            product.MetaKeywords = dataTransferObject.SEOKeywords;
            product.Abstract = dataTransferObject.Abstract;

            //Brand
            if (!String.IsNullOrWhiteSpace(dataTransferObject.Brand))
            {
                var brand = _brandService.GetBrandByName(dataTransferObject.Brand);
                if (brand == null)
                {
                    brand = new Brand {Name = dataTransferObject.Brand};
                    _brandService.Add(brand);
                }
                product.Brand = brand;
            }

            //Categories
            product.Categories.Clear();
            foreach (var item in dataTransferObject.Categories)
            {
                var category = _documentService.GetDocument<Category>(item);
                if (category != null && !product.Categories.Any(x => x.Id == category.Id))
                    product.Categories.Add(category);
            }

            product.AttributeOptions.Clear();

            if (product.Id == 0)
            {
                _documentService.AddDocument(product);
                product.Variants.Clear();
            }
            else
                _documentService.SaveDocument(product);

            //Url History
            _importUrlHistoryService.ImportUrlHistory(dataTransferObject, product);

            //Images
            _importProductImagesService.ImportProductImages(dataTransferObject, product);

            //Specifications
            _importSpecificationsService.ImportSpecifications(dataTransferObject, product);

            //Variants
            _importProductVariantsService.ImportVariants(dataTransferObject, product);

            _documentService.SaveDocument(product);

            return product;
        }

    }
}