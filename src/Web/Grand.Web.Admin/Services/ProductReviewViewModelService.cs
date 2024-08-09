﻿using AutoMapper;
using Grand.Business.Core.Commands.Catalog;
using Grand.Business.Core.Events.Catalog;
using Grand.Business.Core.Interfaces.Catalog.Products;
using Grand.Business.Core.Interfaces.Common.Directory;
using Grand.Business.Core.Interfaces.Common.Localization;
using Grand.Business.Core.Interfaces.Common.Stores;
using Grand.Business.Core.Interfaces.Customers;
using Grand.Domain.Catalog;
using Grand.SharedKernel.Extensions;
using Grand.Web.Admin.Extensions.Mapping;
using Grand.Web.Admin.Interfaces;
using Grand.Web.Admin.Models.Catalog;
using Grand.Web.Common.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Grand.Web.Admin.Services;

public class ProductReviewViewModelService : IProductReviewViewModelService
{
    private readonly ICustomerService _customerService;
    private readonly IDateTimeService _dateTimeService;
    private readonly IMediator _mediator;
    private readonly IProductReviewService _productReviewService;
    private readonly IProductService _productService;
    private readonly IStoreService _storeService;
    private readonly ITranslationService _translationService;
    private readonly IMapper _mapper;

    public ProductReviewViewModelService(
        IProductService productService,
        IProductReviewService productReviewService,
        ICustomerService customerService,
        IStoreService storeService,
        IDateTimeService dateTimeService,
        ITranslationService translationService,
        IMediator mediator,
        IMapper mapper)
    {
        _productService = productService;
        _productReviewService = productReviewService;
        _customerService = customerService;
        _storeService = storeService;
        _dateTimeService = dateTimeService;
        _translationService = translationService;
        _mediator = mediator;
        _mapper = mapper;
    }

    public virtual async Task PrepareProductReviewModel(ProductReviewModel model,
        ProductReview productReview, bool excludeProperties, bool formatReviewText)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(productReview);

        var product = await _productService.GetProductById(productReview.ProductId);
        var customer = await _customerService.GetCustomerById(productReview.CustomerId);
        var store = await _storeService.GetStoreById(productReview.StoreId);
        model.Id = productReview.Id;
        model.StoreName = store != null ? store.Shortcut : "";
        model.ProductId = productReview.ProductId;
        model.ProductName = product.Name;
        model.CustomerId = productReview.CustomerId;
        model.CustomerInfo = customer != null
            ? !string.IsNullOrEmpty(customer.Email)
                ? customer.Email
                : _translationService.GetResource("Admin.Customers.Guest")
            : "";
        model.Rating = productReview.Rating;
        model.CreatedOn = _dateTimeService.ConvertToUserTime(productReview.CreatedOnUtc, DateTimeKind.Utc);
        model.Signature = productReview.Signature;
        if (!excludeProperties)
        {
            model.Title = productReview.Title;
            if (formatReviewText)
            {
                model.ReviewText = FormatText.ConvertText(productReview.ReviewText);
                model.ReplyText = FormatText.ConvertText(productReview.ReplyText);
            }
            else
            {
                model.ReviewText = productReview.ReviewText;
                model.ReplyText = productReview.ReplyText;
            }

            model.IsApproved = productReview.IsApproved;
        }
    }

    public virtual async Task<ProductReviewListModel> PrepareProductReviewListModel(string storeId)
    {
        var model = new ProductReviewListModel();

        model.AvailableStores.Add(new SelectListItem
            { Text = _translationService.GetResource("Admin.Common.All"), Value = "" });
        var stores = (await _storeService.GetAllStores())
            .Where(x => x.Id == storeId || string.IsNullOrWhiteSpace(storeId)).Select(st =>
                new SelectListItem { Text = st.Shortcut, Value = st.Id.ToString() });
        foreach (var selectListItem in stores)
            model.AvailableStores.Add(selectListItem);
        return model;
    }

    public virtual async Task<(IEnumerable<ProductReviewModel> productReviewModels, int totalCount)>
        PrepareProductReviewsModel(ProductReviewListModel model, int pageIndex, int pageSize)
    {
        DateTime? createdOnFromValue = model.CreatedOnFrom == null
            ? null
            : _dateTimeService.ConvertToUtcTime(model.CreatedOnFrom.Value, _dateTimeService.CurrentTimeZone);

        DateTime? createdToFromValue = model.CreatedOnTo == null
            ? null
            : _dateTimeService.ConvertToUtcTime(model.CreatedOnTo.Value, _dateTimeService.CurrentTimeZone).AddDays(1);

        var productReviews = await _productReviewService.GetAllProductReviews("", null,
            createdOnFromValue, createdToFromValue, model.SearchText, model.SearchStoreId, model.SearchProductId);

        var items = new List<ProductReviewModel>();
        foreach (var x in productReviews.PagedForCommand(pageIndex, pageSize))
        {
            var m = new ProductReviewModel();
            await PrepareProductReviewModel(m, x, false, true);
            items.Add(m);
        }

        return (items, productReviews.Count);
    }

    public virtual async Task<ProductReview> UpdateProductReview(ProductReview productReview, ProductReviewModel model)
    {
        productReview = _mapper.Map(model, productReview);
        await _productReviewService.UpdateProductReview(productReview);

        //update product totals
        var product = await _productService.GetProductById(productReview.ProductId);

        //update product totals
        await _mediator.Send(new UpdateProductReviewTotalsCommand { Product = product });

        return productReview;
    }

    public virtual async Task DeleteProductReview(ProductReview productReview)
    {
        await _productReviewService.DeleteProductReview(productReview);

        var product = await _productService.GetProductById(productReview.ProductId);
        //update product totals
        await _mediator.Send(new UpdateProductReviewTotalsCommand { Product = product });
    }

    public virtual async Task ApproveSelected(IEnumerable<string> selectedIds, string storeId)
    {
        foreach (var id in selectedIds)
        {
            var idReview = id.Split(':').First();
            var idProduct = id.Split(':').Last();
            var product = await _productService.GetProductById(idProduct);
            var productReview = await _productReviewService.GetProductReviewById(idReview);
            if (productReview != null && (string.IsNullOrEmpty(storeId) || productReview.StoreId == storeId))
            {
                var previousIsApproved = productReview.IsApproved;
                productReview.IsApproved = true;
                await _productReviewService.UpdateProductReview(productReview);
                //update product totals
                await _mediator.Send(new UpdateProductReviewTotalsCommand { Product = product });


                //raise event (only if it wasn't approved before)
                if (!previousIsApproved)
                    await _mediator.Publish(new ProductReviewApprovedEvent(productReview));
            }
        }
    }

    public virtual async Task DisapproveSelected(IEnumerable<string> selectedIds, string storeId)
    {
        foreach (var id in selectedIds)
        {
            var idReview = id.Split(':').First();
            var idProduct = id.Split(':').Last();

            var product = await _productService.GetProductById(idProduct);
            var productReview = await _productReviewService.GetProductReviewById(idReview);
            if (productReview != null && (string.IsNullOrEmpty(storeId) || productReview.StoreId == storeId))
            {
                productReview.IsApproved = false;
                await _productReviewService.UpdateProductReview(productReview);
                //update product totals
                await _mediator.Send(new UpdateProductReviewTotalsCommand { Product = product });
            }
        }
    }
}