using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AssetManagement.AppService.DTOs.User
{
    public class BaseUserRequest
    {
        [JsonIgnore]
        public int UserId { get; set; }
    }
}