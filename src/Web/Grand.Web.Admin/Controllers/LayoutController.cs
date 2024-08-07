﻿using AutoMapper;
using Grand.Business.Core.Interfaces.Catalog.Brands;
using Grand.Business.Core.Interfaces.Catalog.Categories;
using Grand.Business.Core.Interfaces.Catalog.Collections;
using Grand.Business.Core.Interfaces.Catalog.Products;
using Grand.Business.Core.Interfaces.Cms;
using Grand.Business.Core.Utilities.Common.Security;
using Grand.Domain.Catalog;
using Grand.Domain.Pages;
using Grand.Web.Admin.Models.Layouts;
using Grand.Web.Common.DataSource;
using Grand.Web.Common.Extensions;
using Grand.Web.Common.Security.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.Admin.Controllers;

[PermissionAuthorize(PermissionSystemName.Maintenance)]
public class LayoutController : BaseAdminController
{
    #region Constructors

    public LayoutController(ICategoryLayoutService categoryLayoutService,
        IBrandLayoutService brandLayoutService,
        ICollectionLayoutService collectionLayoutService,
        IProductLayoutService productLayoutService,
        IPageLayoutService pageLayoutService,
        IMapper mapper)
    {
        _categoryLayoutService = categoryLayoutService;
        _brandLayoutService = brandLayoutService;
        _collectionLayoutService = collectionLayoutService;
        _productLayoutService = productLayoutService;
        _pageLayoutService = pageLayoutService;
        _mapper = mapper;
    }

    #endregion

    #region Fields

    private readonly ICategoryLayoutService _categoryLayoutService;
    private readonly IBrandLayoutService _brandLayoutService;
    private readonly ICollectionLayoutService _collectionLayoutService;
    private readonly IProductLayoutService _productLayoutService;
    private readonly IPageLayoutService _pageLayoutService;
    private readonly IMapper _mapper;
    #endregion

    #region Category layouts

    public IActionResult CategoryLayouts()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CategoryLayouts(DataSourceRequest command)
    {
        var layoutsModel = (await _categoryLayoutService.GetAllCategoryLayouts())
            .Select(x => _mapper.Map<CategoryLayoutModel>(x))
            .ToList();
        var gridModel = new DataSourceResult {
            Data = layoutsModel,
            Total = layoutsModel.Count
        };

        return Json(gridModel);
    }

    [HttpPost]
    public async Task<IActionResult> CategoryLayoutUpdate(CategoryLayoutModel model)
    {
        if (!ModelState.IsValid) return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });

        var layout = await _categoryLayoutService.GetCategoryLayoutById(model.Id);
        if (layout == null)
            throw new ArgumentException("No layout found with the specified id");
        if (ModelState.IsValid)
        {
            layout = _mapper.Map(model, layout);
            await _categoryLayoutService.UpdateCategoryLayout(layout);

            return new JsonResult("");
        }

        return ErrorForKendoGridJson(ModelState);
    }

    [HttpPost]
    public async Task<IActionResult> CategoryLayoutAdd(CategoryLayoutModel model)
    {
        switch (ModelState.IsValid)
        {
            case false:
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            case true:
            {
                var layout = new CategoryLayout();
                layout = _mapper.Map(model, layout);
                await _categoryLayoutService.InsertCategoryLayout(layout);

                return new JsonResult("");
            }
        }
    }

    [HttpPost]
    public async Task<IActionResult> CategoryLayoutDelete(string id)
    {
        var layout = await _categoryLayoutService.GetCategoryLayoutById(id);
        if (layout == null)
            throw new ArgumentException("No layout found with the specified id");
        if (ModelState.IsValid)
        {
            await _categoryLayoutService.DeleteCategoryLayout(layout);

            return new JsonResult("");
        }

        return ErrorForKendoGridJson(ModelState);
    }

    #endregion

    #region Brand layouts

    public IActionResult BrandLayouts()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> BrandLayouts(DataSourceRequest command)
    {
        var layoutsModel = (await _brandLayoutService.GetAllBrandLayouts())
            .Select(x => _mapper.Map<BrandLayoutModel>(x))
            .ToList();
        var gridModel = new DataSourceResult {
            Data = layoutsModel,
            Total = layoutsModel.Count
        };
        return Json(gridModel);
    }

    [HttpPost]
    public async Task<IActionResult> BrandLayoutUpdate(BrandLayoutModel model)
    {
        if (!ModelState.IsValid) return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });

        var layout = await _brandLayoutService.GetBrandLayoutById(model.Id);
        if (layout == null)
            throw new ArgumentException("No layout found with the specified id");
        if (ModelState.IsValid)
        {
            layout = _mapper.Map(model, layout);
            await _brandLayoutService.UpdateBrandLayout(layout);
            return new JsonResult("");
        }

        return ErrorForKendoGridJson(ModelState);
    }

    [HttpPost]
    public async Task<IActionResult> BrandLayoutAdd(BrandLayoutModel model)
    {
        switch (ModelState.IsValid)
        {
            case false:
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            case true:
            {
                var layout = new BrandLayout();
                layout = _mapper.Map(model, layout);
                await _brandLayoutService.InsertBrandLayout(layout);
                return new JsonResult("");
            }
        }
    }

    [HttpPost]
    public async Task<IActionResult> BrandLayoutDelete(string id)
    {
        var layout = await _brandLayoutService.GetBrandLayoutById(id);
        if (layout == null)
            throw new ArgumentException("No layout found with the specified id");
        if (ModelState.IsValid)
        {
            await _brandLayoutService.DeleteBrandLayout(layout);
            return new JsonResult("");
        }

        return ErrorForKendoGridJson(ModelState);
    }

    #endregion

    #region Collection layouts

    public IActionResult CollectionLayouts()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CollectionLayouts(DataSourceRequest command)
    {
        var layoutsModel = (await _collectionLayoutService.GetAllCollectionLayouts())
            .Select(x => _mapper.Map<CollectionLayoutModel>(x))
            .ToList();
        var gridModel = new DataSourceResult {
            Data = layoutsModel,
            Total = layoutsModel.Count
        };
        return Json(gridModel);
    }

    [HttpPost]
    public async Task<IActionResult> CollectionLayoutUpdate(CollectionLayoutModel model)
    {
        if (!ModelState.IsValid) return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });

        var layout = await _collectionLayoutService.GetCollectionLayoutById(model.Id);
        if (layout == null)
            throw new ArgumentException("No layout found with the specified id");
        if (ModelState.IsValid)
        {
            layout = _mapper.Map(model, layout);
            await _collectionLayoutService.UpdateCollectionLayout(layout);
            return new JsonResult("");
        }

        return ErrorForKendoGridJson(ModelState);
    }

    [HttpPost]
    public async Task<IActionResult> CollectionLayoutAdd(CollectionLayoutModel model)
    {
        switch (ModelState.IsValid)
        {
            case false:
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            case true:
            {
                var layout = new CollectionLayout();
                layout = _mapper.Map(model, layout);
                await _collectionLayoutService.InsertCollectionLayout(layout);
                return new JsonResult("");
            }
        }
    }

    [HttpPost]
    public async Task<IActionResult> CollectionLayoutDelete(string id)
    {
        var layout = await _collectionLayoutService.GetCollectionLayoutById(id);
        if (layout == null)
            throw new ArgumentException("No layout found with the specified id");
        if (ModelState.IsValid)
        {
            await _collectionLayoutService.DeleteCollectionLayout(layout);
            return new JsonResult("");
        }

        return ErrorForKendoGridJson(ModelState);
    }

    #endregion

    #region Product layouts

    public IActionResult ProductLayouts()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ProductLayouts(DataSourceRequest command)
    {
        var layoutsModel = (await _productLayoutService.GetAllProductLayouts())
            .Select(x => _mapper.Map<ProductLayoutModel>(x))
            .ToList();
        var gridModel = new DataSourceResult {
            Data = layoutsModel,
            Total = layoutsModel.Count
        };
        return Json(gridModel);
    }

    [HttpPost]
    public async Task<IActionResult> ProductLayoutUpdate(ProductLayoutModel model)
    {
        if (!ModelState.IsValid) return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
        var layout = await _productLayoutService.GetProductLayoutById(model.Id);
        if (layout == null)
            throw new ArgumentException("No template found with the specified id");
        if (ModelState.IsValid)
        {
            layout = _mapper.Map(model, layout);
            await _productLayoutService.UpdateProductLayout(layout);
            return new JsonResult("");
        }

        return ErrorForKendoGridJson(ModelState);
    }

    [HttpPost]
    public async Task<IActionResult> ProductLayoutAdd(ProductLayoutModel model)
    {
        switch (ModelState.IsValid)
        {
            case false:
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            case true:
            {
                var layout = new ProductLayout();
                layout = _mapper.Map(model, layout);
                await _productLayoutService.InsertProductLayout(layout);
                return new JsonResult("");
            }
        }
    }

    [HttpPost]
    public async Task<IActionResult> ProductLayoutDelete(string id)
    {
        var layout = await _productLayoutService.GetProductLayoutById(id);
        if (layout == null)
            throw new ArgumentException("No template found with the specified id");
        if (!ModelState.IsValid) return ErrorForKendoGridJson(ModelState);
        await _productLayoutService.DeleteProductLayout(layout);
        return new JsonResult("");
    }

    #endregion

    #region Page layouts

    public IActionResult PageLayouts()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> PageLayouts(DataSourceRequest command)
    {
        var layoutsModel = (await _pageLayoutService.GetAllPageLayouts())
            .Select(x => _mapper.Map<PageLayoutModel>(x))
            .ToList();
        var gridModel = new DataSourceResult {
            Data = layoutsModel,
            Total = layoutsModel.Count
        };
        return Json(gridModel);
    }

    [HttpPost]
    public async Task<IActionResult> PageLayoutUpdate(PageLayoutModel model)
    {
        if (!ModelState.IsValid) return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });

        var layout = await _pageLayoutService.GetPageLayoutById(model.Id);
        if (layout == null)
            throw new ArgumentException("No template found with the specified id");
        if (!ModelState.IsValid) return ErrorForKendoGridJson(ModelState);
        layout = _mapper.Map(model, layout);
        await _pageLayoutService.UpdatePageLayout(layout);
        return new JsonResult("");
    }

    [HttpPost]
    public async Task<IActionResult> PageLayoutAdd(PageLayoutModel model)
    {
        switch (ModelState.IsValid)
        {
            case false:
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            case true:
            {
                var layout = new PageLayout();
                layout = _mapper.Map(model, layout);
                await _pageLayoutService.InsertPageLayout(layout);
                return new JsonResult("");
            }
        }
    }

    [HttpPost]
    public async Task<IActionResult> PageLayoutDelete(string id)
    {
        var layout = await _pageLayoutService.GetPageLayoutById(id);
        if (layout == null)
            throw new ArgumentException("No layout found with the specified id");
        if (!ModelState.IsValid) return ErrorForKendoGridJson(ModelState);
        await _pageLayoutService.DeletePageLayout(layout);
        return new JsonResult("");
    }

    #endregion
}