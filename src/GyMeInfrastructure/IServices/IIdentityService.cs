﻿using GymAppCore.Models;
using GymAppInfrastructure.Models.Account;
using GymAppInfrastructure.Models.User;
using GymAppInfrastructure.Results;
using GymAppInfrastructure.Results.Authorization;
using Microsoft.AspNetCore.Http;

namespace GymAppInfrastructure.IServices;

public interface IIdentityService
{
    Task<AuthenticationRegisterResult> Register(RegisterUserDto registerUserDto, byte[] profilePicture, Func<string, string, string> generateCallbackToken);
    Task<AuthenticationLoginResult> Login(LoginUserDto loginUserDto);
    Task<AuthenticationLoginResult> ExternalLogin(string? email, string? nameSurname);
    Task<bool> ConfirmEmail(string userId, string code);
    Task<ActivateUserResult> ActivateUser(ActivateAccountModel activateAccountModel, byte[] profilePicture);
    Task<bool> SendResetPasswordToken(string email);
    Task<ResetPasswordResult> ResetPassword(ResetPassword model);
}