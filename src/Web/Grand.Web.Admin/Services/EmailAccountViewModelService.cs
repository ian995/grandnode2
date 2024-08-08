﻿using AutoMapper;
using Grand.Business.Core.Interfaces.Messages;
using Grand.Domain.Messages;
using Grand.Web.Admin.Extensions.Mapping;
using Grand.Web.Admin.Interfaces;
using Grand.Web.Admin.Models.Messages;

namespace Grand.Web.Admin.Services;

public class EmailAccountViewModelService : IEmailAccountViewModelService
{
    private readonly IEmailAccountService _emailAccountService;
    private readonly IEmailSender _emailSender;
    private readonly IMapper _mapper;

    public EmailAccountViewModelService(IEmailAccountService emailAccountService, IEmailSender emailSender, IMapper mapper)
    {
        _emailAccountService = emailAccountService;
        _emailSender = emailSender;
        _mapper = mapper;
    }

    public virtual EmailAccountModel PrepareEmailAccountModel()
    {
        var model = new EmailAccountModel {
            //default values
            Port = 25
        };
        return model;
    }

    public virtual async Task<EmailAccount> InsertEmailAccountModel(EmailAccountModel model)
    {
        var emailAccount = _mapper.Map<EmailAccount>(model);
        //set password manually
        emailAccount.Password = model.Password;
        await _emailAccountService.InsertEmailAccount(emailAccount);
        return emailAccount;
    }

    public virtual async Task<EmailAccount> UpdateEmailAccountModel(EmailAccount emailAccount, EmailAccountModel model)
    {
        emailAccount = _mapper.Map(model, emailAccount);
        if (!string.IsNullOrEmpty(model.Password))
            emailAccount.Password = model.Password;

        await _emailAccountService.UpdateEmailAccount(emailAccount);
        return emailAccount;
    }

    public virtual async Task SendTestEmail(EmailAccount emailAccount, EmailAccountModel model)
    {
        var subject = "Testing email functionality.";
        var body = "Email works fine.";
        await _emailSender.SendEmail(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName,
            model.SendTestEmailTo, null);
    }
}