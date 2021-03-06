﻿using System.Collections.Generic;
using FakeItEasy;
using FluentAssertions;
using MrCMS.Batching.Services;
using MrCMS.Entities.Documents.Media;
using MrCMS.Services;
using MrCMS.Web.Apps.Ecommerce.Areas.Admin.Services;
using MrCMS.Web.Apps.Ecommerce.Helpers;
using MrCMS.Web.Apps.Ecommerce.Pages;
using MrCMS.Web.Apps.Ecommerce.Services.ImportExport;
using MrCMS.Web.Apps.Ecommerce.Services.ImportExport.DTOs;
using Xunit;

namespace MrCMS.EcommerceApp.Tests.Services.ImportExport
{
    public class OldImportProductsServiceTests : InMemoryDatabaseTest
    {
        private readonly IDocumentService _documentService;
        private readonly IGetNewBrandPage _getNewBrandPage = A.Fake<IGetNewBrandPage>();
        private readonly IImportProductImagesService _importProductImagesService;
        private readonly IImportProductUrlHistoryService _importProductUrlHistoryService;
        private readonly IImportProductVariantsService _importProductVariantsService;
        private readonly ImportProductsService _importProductsService;
        private readonly IImportProductSpecificationsService _importSpecificationsService;
        private readonly IUniquePageService _uniquePageService;
        private ICreateBatch _createBatch;

        public OldImportProductsServiceTests()
        {
            _documentService = A.Fake<IDocumentService>();
            _importSpecificationsService = A.Fake<IImportProductSpecificationsService>();
            _importProductVariantsService = A.Fake<IImportProductVariantsService>();
            _importProductImagesService = A.Fake<IImportProductImagesService>();
            _importProductUrlHistoryService = A.Fake<IImportProductUrlHistoryService>();
            _uniquePageService = A.Fake<IUniquePageService>();
            _createBatch = A.Fake<ICreateBatch>();
            _importProductsService = new ImportProductsService(_documentService,
                _importSpecificationsService,
                _importProductVariantsService,
                _importProductImagesService,
                _importProductUrlHistoryService, Session, _uniquePageService, _createBatch, _getNewBrandPage);
        }

        [Fact(Skip = "To be refactored")]
        public void ImportProductsService_ImportProduct_ShouldCallGetGetDocumentByUrlOfDocumentService()
        {
            var product = new ProductImportDataTransferObject
            {
                UrlSegment = "test-url",
            };

            _importProductsService.ImportProduct(product);

            A.CallTo(() => _documentService.GetDocumentByUrl<Product>(product.UrlSegment)).MustHaveHappened();
        }

        [Fact(Skip = "To be refactored")]
        public void ImportProductsService_ImportProduct_ShouldTryToLoadTheCategoryFromTheDocumentService()
        {
            var product = new ProductImportDataTransferObject
            {
                UrlSegment = "test-url",
                Categories = new List<string> {"test-category"}
            };

            _importProductsService.ImportProduct(product);

            A.CallTo(() => _documentService.GetDocumentByUrl<Category>("test-category")).MustHaveHappened();
        }

        [Fact]
        public void ImportProductsService_ImportProduct_ShouldSetProductPrimaryProperties()
        {
            var product = new ProductImportDataTransferObject
            {
                UrlSegment = "test-url",
                Name = "Test Product",
                Abstract = "Test Abstract",
                Description = "Test Description",
                SEODescription = "Test SEO Description",
                SEOKeywords = "Test, Thought",
                SEOTitle = "Test SEO Title"
            };

            ProductContainer container = new ProductContainer().PersistTo(Session);
            A.CallTo(() => _uniquePageService.GetUniquePage<ProductContainer>()).Returns(container);
            MediaCategory category = new MediaCategory().PersistTo(Session);
            A.CallTo(() => _documentService.GetDocumentByUrl<MediaCategory>("product-galleries")).Returns(category);

            Product result = _importProductsService.ImportProduct(product);

            result.UrlSegment.ShouldBeEquivalentTo("test-url");
            result.Name.ShouldBeEquivalentTo("Test Product");
            result.ProductAbstract.ShouldBeEquivalentTo("Test Abstract");
            result.BodyContent.ShouldBeEquivalentTo("Test Description");
            result.MetaDescription.ShouldBeEquivalentTo("Test SEO Description");
            result.MetaKeywords.ShouldBeEquivalentTo("Test, Thought");
            result.MetaTitle.ShouldBeEquivalentTo("Test SEO Title");
        }

        [Fact(Skip = "To be refactored")]
        public void ImportProductsService_ImportProducts_ShouldSetProductBrandIfItAlreadyExists()
        {
            var product = new ProductImportDataTransferObject
            {
                UrlSegment = "test-url",
                Brand = "Test Brand"
            };

            var brand = new Brand {Name = "Test BrandPage"};

            Product importProduct = _importProductsService.ImportProduct(product);

            importProduct.BrandPage.Should().Be(brand);
        }

        [Fact(Skip = "To be refactored")]
        public void ImportProductsService_ImportProducts_ShouldSetTheBrandToOneWithTheCorrectNameIfItDoesNotExist()
        {
            var product = new ProductImportDataTransferObject
            {
                UrlSegment = "test-url",
                Brand = "Test Brand"
            };

            Product importProduct = _importProductsService.ImportProduct(product);

            importProduct.BrandPage.Name.Should().Be("Test Brand");
        }

        [Fact(Skip = "To be refactored")]
        public void ImportProductsService_ImportProducts_ShouldSetCategoriesIfTheyExist()
        {
            var productDTO = new ProductImportDataTransferObject
            {
                UrlSegment = "test-url",
                Categories = new List<string> {"test-category"}
            };

            var category = new Category {Id = 1, Name = "Test Category"};
            A.CallTo(() => _documentService.GetDocument<Category>(1)).Returns(category);

            var product = new Product {Name = "Test Product"};
            A.CallTo(() => _documentService.GetDocumentByUrl<Product>(productDTO.UrlSegment)).Returns(product);

            Product importProduct = _importProductsService.ImportProduct(productDTO);

            importProduct.Categories.Should().HaveCount(1);
        }

        [Fact(Skip = "To be refactored")]
        public void ImportProductService_ImportProducts_ShouldCallImportUrlHistoryOfImportProductUrlHistoryService()
        {
            var productDTO = new ProductImportDataTransferObject
            {
                UrlSegment = "test-url",
                UrlHistory = new List<string> {"test-url-old"}
            };

            var product = new Product {Name = "Test Product"};
            A.CallTo(() => _documentService.GetDocumentByUrl<Product>(productDTO.UrlSegment)).Returns(product);

            _importProductsService.ImportProduct(productDTO);

            A.CallTo(() => _importProductUrlHistoryService.ImportUrlHistory(productDTO, product)).MustHaveHappened();
        }
    }
}