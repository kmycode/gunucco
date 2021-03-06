﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gunucco.ViewModels
{
    public class MessageViewModel
    {
        public bool HasMessage { get; set; }

        public bool IsError { get; set; } = true;

        public string Message { get; set; }

        public void AddMessage(string mes)
        {
            if (string.IsNullOrEmpty(this.Message))
            {
                this.Message = mes;
            }
            else
            {
                this.Message += "<br />" + mes;
            }
        }
    }
}
