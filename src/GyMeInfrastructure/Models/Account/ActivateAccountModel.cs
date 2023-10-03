﻿using GymAppInfrastructure.Models.User;

namespace GymAppInfrastructure.Models.Account;

public class ActivateAccountModel
{
    public string UserName { get; set; }
    public GenderDto GenderDto { get; set; }
    public bool PrivateAccount { get; set; }
    public string? Description { get; set; }
}