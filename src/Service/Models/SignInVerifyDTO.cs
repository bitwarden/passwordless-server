﻿using System.Text.Json.Serialization;
using Passwordless.Common.Converters;
using Passwordless.Common.Models.Apps;

namespace Passwordless.Service.Models;

public class SignInVerifyDTO : RequestBase
{
    public string Token { get; set; }
}